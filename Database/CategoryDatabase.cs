using System.Data.SqlClient;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Database
{
    public class CategoryDatabase
    {
        private string connection_string = @"Data Source=(localdb)\api_raceday;Initial Catalog=raceday_eventDB;Integrated Security=True;TrustServerCertificate=True;";

        // ============================================================
        // GET ALL CATEGORIES
        // ============================================================
        public List<Category> get_all_categories()
        {
            List<Category> categories = new List<Category>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT CategoryId, CategoryName, CategoryType, Distance,
                                            AgeGroup, Gender, StandardFee, CreatedAt, UpdatedAt
                                     FROM Categories
                                     ORDER BY CategoryName;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new Category
                                {
                                    CategoryId = Convert.ToInt32(reader["CategoryId"]),
                                    CategoryName = reader["CategoryName"].ToString(),
                                    CategoryType = reader["CategoryType"].ToString(),
                                    Distance = reader["Distance"] == DBNull.Value ? null : reader["Distance"].ToString(),
                                    AgeGroup = reader["AgeGroup"] == DBNull.Value ? null : reader["AgeGroup"].ToString(),
                                    Gender = reader["Gender"] == DBNull.Value ? null : reader["Gender"].ToString(),
                                    StandardFee = Convert.ToDecimal(reader["StandardFee"]),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                    UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_all_categories] Error: {error.Message}");
                categories = new List<Category>();
            }

            return categories;
        }

        // ============================================================
        // GET CATEGORY BY ID
        // ============================================================
        public Category get_category_by_id(int category_id)
        {
            Category cat = null;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT CategoryId, CategoryName, CategoryType, Distance,
                                            AgeGroup, Gender, StandardFee, CreatedAt, UpdatedAt
                                     FROM Categories
                                     WHERE CategoryId = @CategoryId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@CategoryId", category_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cat = new Category
                                {
                                    CategoryId = Convert.ToInt32(reader["CategoryId"]),
                                    CategoryName = reader["CategoryName"].ToString(),
                                    CategoryType = reader["CategoryType"].ToString(),
                                    Distance = reader["Distance"] == DBNull.Value ? null : reader["Distance"].ToString(),
                                    AgeGroup = reader["AgeGroup"] == DBNull.Value ? null : reader["AgeGroup"].ToString(),
                                    Gender = reader["Gender"] == DBNull.Value ? null : reader["Gender"].ToString(),
                                    StandardFee = Convert.ToDecimal(reader["StandardFee"]),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                    UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_category_by_id] Error: {error.Message}");
                cat = null;
            }

            return cat;
        }

        // ============================================================
        // CREATE CATEGORY
        // ============================================================
        public string create_category(string category_name, string category_type, string distance,
                                       string age_group, string gender, decimal standard_fee)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"INSERT INTO Categories (CategoryName, CategoryType, Distance, AgeGroup, Gender, StandardFee)
                                     VALUES (@CategoryName, @CategoryType, @Distance, @AgeGroup, @Gender, @StandardFee);";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@CategoryName", category_name);
                        run_query.Parameters.AddWithValue("@CategoryType", category_type);
                        run_query.Parameters.AddWithValue("@Distance", string.IsNullOrEmpty(distance) ? DBNull.Value : distance);
                        run_query.Parameters.AddWithValue("@AgeGroup", string.IsNullOrEmpty(age_group) ? DBNull.Value : age_group);
                        run_query.Parameters.AddWithValue("@Gender", string.IsNullOrEmpty(gender) ? DBNull.Value : gender);
                        run_query.Parameters.AddWithValue("@StandardFee", standard_fee);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Category created successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[create_category] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // UPDATE CATEGORY
        // ============================================================
        public string update_category(int category_id, string category_name, string category_type, string distance,
                                       string age_group, string gender, decimal standard_fee)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"UPDATE Categories
                                     SET CategoryName = @CategoryName,
                                         CategoryType = @CategoryType,
                                         Distance = @Distance,
                                         AgeGroup = @AgeGroup,
                                         Gender = @Gender,
                                         StandardFee = @StandardFee,
                                         UpdatedAt = GETDATE()
                                     WHERE CategoryId = @CategoryId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@CategoryName", category_name);
                        run_query.Parameters.AddWithValue("@CategoryType", category_type);
                        run_query.Parameters.AddWithValue("@Distance", string.IsNullOrEmpty(distance) ? DBNull.Value : distance);
                        run_query.Parameters.AddWithValue("@AgeGroup", string.IsNullOrEmpty(age_group) ? DBNull.Value : age_group);
                        run_query.Parameters.AddWithValue("@Gender", string.IsNullOrEmpty(gender) ? DBNull.Value : gender);
                        run_query.Parameters.AddWithValue("@StandardFee", standard_fee);
                        run_query.Parameters.AddWithValue("@CategoryId", category_id);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Category updated successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[update_category] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // DELETE CATEGORY
        // ============================================================
        public string delete_category(int category_id)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "DELETE FROM Categories WHERE CategoryId = @CategoryId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@CategoryId", category_id);
                        run_query.ExecuteNonQuery();
                    }

                    message = "Category deleted successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[delete_category] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }
    }
}