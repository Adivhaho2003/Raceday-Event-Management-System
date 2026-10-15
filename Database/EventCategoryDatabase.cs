using System.Data.SqlClient;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Database
{
    public class EventCategoryDatabase
    {
        private string connection_string = @"Data Source=(localdb)\api_raceday;Initial Catalog=raceday_eventDB;Integrated Security=True;TrustServerCertificate=True;";

        // ============================================================
        // GET ALL CATEGORIES FOR AN EVENT
        // ============================================================
        public List<EventCategory> get_event_categories(int event_id)
        {
            List<EventCategory> eventCategories = new List<EventCategory>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT ec.EventCategoryId, ec.EventId, ec.CategoryId, ec.Fee, ec.Capacity,
                                            ec.CreatedAt, ec.UpdatedAt,
                                            c.CategoryName, c.CategoryType, c.Distance, c.AgeGroup, c.Gender, c.StandardFee
                                     FROM EventCategories ec
                                     JOIN Categories c ON ec.CategoryId = c.CategoryId
                                     WHERE ec.EventId = @EventId
                                     ORDER BY c.CategoryName;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                EventCategory ec = new EventCategory
                                {
                                    EventCategoryId = Convert.ToInt32(reader["EventCategoryId"]),
                                    EventId = Convert.ToInt32(reader["EventId"]),
                                    CategoryId = Convert.ToInt32(reader["CategoryId"]),
                                    Fee = reader["Fee"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Fee"]),
                                    Capacity = reader["Capacity"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Capacity"]),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                    UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                                };

                                ec.Category = new Category
                                {
                                    CategoryId = ec.CategoryId,
                                    CategoryName = reader["CategoryName"].ToString(),
                                    CategoryType = reader["CategoryType"].ToString(),
                                    Distance = reader["Distance"] == DBNull.Value ? null : reader["Distance"].ToString(),
                                    AgeGroup = reader["AgeGroup"] == DBNull.Value ? null : reader["AgeGroup"].ToString(),
                                    Gender = reader["Gender"] == DBNull.Value ? null : reader["Gender"].ToString(),
                                    StandardFee = Convert.ToDecimal(reader["StandardFee"])
                                };

                                eventCategories.Add(ec);
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_event_categories] Error: {error.Message}");
                eventCategories = new List<EventCategory>();
            }

            return eventCategories;
        }

        // ============================================================
        // GET SINGLE EVENT CATEGORY BY ID
        // ============================================================
        public EventCategory get_event_category_by_id(int event_category_id)
        {
            EventCategory ec = null;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT ec.EventCategoryId, ec.EventId, ec.CategoryId, ec.Fee, ec.Capacity,
                                            ec.CreatedAt, ec.UpdatedAt,
                                            c.CategoryName, c.CategoryType, c.Distance, c.AgeGroup, c.Gender, c.StandardFee
                                     FROM EventCategories ec
                                     JOIN Categories c ON ec.CategoryId = c.CategoryId
                                     WHERE ec.EventCategoryId = @EventCategoryId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventCategoryId", event_category_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ec = new EventCategory
                                {
                                    EventCategoryId = Convert.ToInt32(reader["EventCategoryId"]),
                                    EventId = Convert.ToInt32(reader["EventId"]),
                                    CategoryId = Convert.ToInt32(reader["CategoryId"]),
                                    Fee = reader["Fee"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Fee"]),
                                    Capacity = reader["Capacity"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Capacity"]),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                    UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                                };

                                ec.Category = new Category
                                {
                                    CategoryId = ec.CategoryId,
                                    CategoryName = reader["CategoryName"].ToString(),
                                    CategoryType = reader["CategoryType"].ToString(),
                                    Distance = reader["Distance"] == DBNull.Value ? null : reader["Distance"].ToString(),
                                    AgeGroup = reader["AgeGroup"] == DBNull.Value ? null : reader["AgeGroup"].ToString(),
                                    Gender = reader["Gender"] == DBNull.Value ? null : reader["Gender"].ToString(),
                                    StandardFee = Convert.ToDecimal(reader["StandardFee"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_event_category_by_id] Error: {error.Message}");
                ec = null;
            }

            return ec;
        }

        // ============================================================
        // CHECK IF LINK ALREADY EXISTS
        // ============================================================
        public bool link_exists(int event_id, int category_id)
        {
            bool exists = false;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "SELECT COUNT(*) FROM EventCategories WHERE EventId = @EventId AND CategoryId = @CategoryId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);
                        run_query.Parameters.AddWithValue("@CategoryId", category_id);

                        int count = Convert.ToInt32(run_query.ExecuteScalar());

                        if (count > 0)
                        {
                            exists = true;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[link_exists] Error: {error.Message}");
                exists = false;
            }

            return exists;
        }

        // ============================================================
        // ADD CATEGORY TO EVENT
        // ============================================================
        public string add_category_to_event(int event_id, int category_id, decimal? fee, int? capacity)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"INSERT INTO EventCategories (EventId, CategoryId, Fee, Capacity)
                                     VALUES (@EventId, @CategoryId, @Fee, @Capacity);";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);
                        run_query.Parameters.AddWithValue("@CategoryId", category_id);

                        if (fee.HasValue)
                            run_query.Parameters.AddWithValue("@Fee", fee.Value);
                        else
                            run_query.Parameters.AddWithValue("@Fee", DBNull.Value);

                        if (capacity.HasValue)
                            run_query.Parameters.AddWithValue("@Capacity", capacity.Value);
                        else
                            run_query.Parameters.AddWithValue("@Capacity", DBNull.Value);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Category added to event successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[add_category_to_event] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // UPDATE EVENT CATEGORY (fee / capacity)
        // ============================================================
        public string update_event_category(int event_category_id, decimal? fee, int? capacity)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"UPDATE EventCategories
                                     SET Fee = @Fee, Capacity = @Capacity, UpdatedAt = GETDATE()
                                     WHERE EventCategoryId = @EventCategoryId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        if (fee.HasValue)
                            run_query.Parameters.AddWithValue("@Fee", fee.Value);
                        else
                            run_query.Parameters.AddWithValue("@Fee", DBNull.Value);

                        if (capacity.HasValue)
                            run_query.Parameters.AddWithValue("@Capacity", capacity.Value);
                        else
                            run_query.Parameters.AddWithValue("@Capacity", DBNull.Value);

                        run_query.Parameters.AddWithValue("@EventCategoryId", event_category_id);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Event category updated successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[update_event_category] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // REMOVE CATEGORY FROM EVENT (by EventId + CategoryId)
        // ============================================================
        public string remove_category_from_event(int event_id, int category_id)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "DELETE FROM EventCategories WHERE EventId = @EventId AND CategoryId = @CategoryId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);
                        run_query.Parameters.AddWithValue("@CategoryId", category_id);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Category removed from event successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[remove_category_from_event] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }
    }
}