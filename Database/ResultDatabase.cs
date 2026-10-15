using System.Data.SqlClient;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Database
{
    public class ResultDatabase
    {
        private string connection_string = @"Data Source=(localdb)\api_raceday;Initial Catalog=raceday_eventDB;Integrated Security=True;TrustServerCertificate=True;";

        // ============================================================
        // GET ALL RESULTS
        // ============================================================
        public List<Result> get_all_results()
        {
            List<Result> results = new List<Result>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT r.ResultId, r.EnrolmentId, r.ParticipantId, r.EventId, r.CategoryId,
                                            r.GunTime, r.ChipTime, r.OverallPosition, r.CategoryPosition,
                                            r.Pace, r.Status, r.CreatedAt, r.UpdatedAt,
                                            e.EventName, c.CategoryName,
                                            u.FirstName, u.LastName, u.Email
                                     FROM Results r
                                     JOIN Events e ON r.EventId = e.EventId
                                     JOIN Categories c ON r.CategoryId = c.CategoryId
                                     JOIN Users u ON r.ParticipantId = u.UserId
                                     ORDER BY e.EventDate DESC, r.OverallPosition ASC;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(MapResult(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_all_results] Error: {error.Message}");
                results = new List<Result>();
            }

            return results;
        }

        // ============================================================
        // GET RESULT BY ID
        // ============================================================
        public Result get_result_by_id(int result_id)
        {
            Result result = null;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT r.ResultId, r.EnrolmentId, r.ParticipantId, r.EventId, r.CategoryId,
                                            r.GunTime, r.ChipTime, r.OverallPosition, r.CategoryPosition,
                                            r.Pace, r.Status, r.CreatedAt, r.UpdatedAt,
                                            e.EventName, c.CategoryName,
                                            u.FirstName, u.LastName, u.Email
                                     FROM Results r
                                     JOIN Events e ON r.EventId = e.EventId
                                     JOIN Categories c ON r.CategoryId = c.CategoryId
                                     JOIN Users u ON r.ParticipantId = u.UserId
                                     WHERE r.ResultId = @ResultId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@ResultId", result_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result = MapResult(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_result_by_id] Error: {error.Message}");
                result = null;
            }

            return result;
        }

        // ============================================================
        // CHECK IF RESULT EXISTS FOR ENROLMENT
        // ============================================================
        public bool result_exists_for_enrolment(int enrolment_id)
        {
            bool exists = false;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "SELECT COUNT(*) FROM Results WHERE EnrolmentId = @EnrolmentId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EnrolmentId", enrolment_id);

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
                Console.WriteLine($"[result_exists_for_enrolment] Error: {error.Message}");
                exists = false;
            }

            return exists;
        }

        // ============================================================
        // GET ENROLMENT BY ID (helper — for result creation)
        // ============================================================
        public Enrolment get_enrolment_by_id(int enrolment_id)
        {
            Enrolment en = null;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT EnrolmentId, EventId, ParticipantId, EventCategoryId,
                                            Status, RegistrationNumber, RegistrationDate,
                                            UpdatedAt, ConfirmationDate, PaymentStatus
                                     FROM Enrolments WHERE EnrolmentId = @EnrolmentId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EnrolmentId", enrolment_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                en = new Enrolment
                                {
                                    EnrolmentId = Convert.ToInt32(reader["EnrolmentId"]),
                                    EventId = Convert.ToInt32(reader["EventId"]),
                                    ParticipantId = Convert.ToInt32(reader["ParticipantId"]),
                                    EventCategoryId = Convert.ToInt32(reader["EventCategoryId"]),
                                    Status = reader["Status"].ToString(),
                                    RegistrationNumber = reader["RegistrationNumber"].ToString(),
                                    RegistrationDate = Convert.ToDateTime(reader["RegistrationDate"]),
                                    UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]),
                                    ConfirmationDate = reader["ConfirmationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ConfirmationDate"]),
                                    PaymentStatus = reader["PaymentStatus"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_enrolment_by_id] Error: {error.Message}");
                en = null;
            }

            return en;
        }

        // ============================================================
        // CREATE RESULT
        // ============================================================
        public string create_result(int enrolment_id, int participant_id, int event_id, int category_id,
                                     string? gun_time, string? chip_time, int? overall_position,
                                     int? category_position, double? pace, string status)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"INSERT INTO Results (EnrolmentId, ParticipantId, EventId, CategoryId,
                                                          GunTime, ChipTime, OverallPosition, CategoryPosition,
                                                          Pace, Status)
                                     VALUES (@EnrolmentId, @ParticipantId, @EventId, @CategoryId,
                                             @GunTime, @ChipTime, @OverallPosition, @CategoryPosition,
                                             @Pace, @Status);";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EnrolmentId", enrolment_id);
                        run_query.Parameters.AddWithValue("@ParticipantId", participant_id);
                        run_query.Parameters.AddWithValue("@EventId", event_id);
                        run_query.Parameters.AddWithValue("@CategoryId", category_id);

                        run_query.Parameters.AddWithValue("@GunTime",
                            string.IsNullOrEmpty(gun_time) ? DBNull.Value : gun_time);
                        run_query.Parameters.AddWithValue("@ChipTime",
                            string.IsNullOrEmpty(chip_time) ? DBNull.Value : chip_time);

                        if (overall_position.HasValue)
                            run_query.Parameters.AddWithValue("@OverallPosition", overall_position.Value);
                        else
                            run_query.Parameters.AddWithValue("@OverallPosition", DBNull.Value);

                        if (category_position.HasValue)
                            run_query.Parameters.AddWithValue("@CategoryPosition", category_position.Value);
                        else
                            run_query.Parameters.AddWithValue("@CategoryPosition", DBNull.Value);

                        if (pace.HasValue)
                            run_query.Parameters.AddWithValue("@Pace", pace.Value);
                        else
                            run_query.Parameters.AddWithValue("@Pace", DBNull.Value);

                        run_query.Parameters.AddWithValue("@Status", status);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Result created successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[create_result] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // UPDATE RESULT
        // ============================================================
        public string update_result(int result_id, string? gun_time, string? chip_time,
                                     int? overall_position, int? category_position,
                                     double? pace, string status)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"UPDATE Results
                                     SET GunTime = @GunTime,
                                         ChipTime = @ChipTime,
                                         OverallPosition = @OverallPosition,
                                         CategoryPosition = @CategoryPosition,
                                         Pace = @Pace,
                                         Status = @Status,
                                         UpdatedAt = GETDATE()
                                     WHERE ResultId = @ResultId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@GunTime",
                            string.IsNullOrEmpty(gun_time) ? DBNull.Value : gun_time);
                        run_query.Parameters.AddWithValue("@ChipTime",
                            string.IsNullOrEmpty(chip_time) ? DBNull.Value : chip_time);

                        if (overall_position.HasValue)
                            run_query.Parameters.AddWithValue("@OverallPosition", overall_position.Value);
                        else
                            run_query.Parameters.AddWithValue("@OverallPosition", DBNull.Value);

                        if (category_position.HasValue)
                            run_query.Parameters.AddWithValue("@CategoryPosition", category_position.Value);
                        else
                            run_query.Parameters.AddWithValue("@CategoryPosition", DBNull.Value);

                        if (pace.HasValue)
                            run_query.Parameters.AddWithValue("@Pace", pace.Value);
                        else
                            run_query.Parameters.AddWithValue("@Pace", DBNull.Value);

                        run_query.Parameters.AddWithValue("@Status", status);
                        run_query.Parameters.AddWithValue("@ResultId", result_id);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Result updated successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[update_result] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // DELETE RESULT
        // ============================================================
        public string delete_result(int result_id)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "DELETE FROM Results WHERE ResultId = @ResultId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@ResultId", result_id);
                        run_query.ExecuteNonQuery();
                    }

                    message = "Result deleted successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[delete_result] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // GET LEADERBOARD FOR EVENT (optional category filter, top N)
        // ============================================================
        public List<Result> get_leaderboard(int event_id, int? category_id, int limit)
        {
            List<Result> results = new List<Result>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string where_extra = "";
                    if (category_id.HasValue)
                        where_extra = " AND r.CategoryId = @CategoryId";

                    string query = $@"SELECT TOP (@Limit) 
                                            r.ResultId, r.EnrolmentId, r.ParticipantId, r.EventId, r.CategoryId,
                                            r.GunTime, r.ChipTime, r.OverallPosition, r.CategoryPosition,
                                            r.Pace, r.Status, r.CreatedAt, r.UpdatedAt,
                                            e.EventName, c.CategoryName,
                                            u.FirstName, u.LastName, u.Email
                                     FROM Results r
                                     JOIN Events e ON r.EventId = e.EventId
                                     JOIN Categories c ON r.CategoryId = c.CategoryId
                                     JOIN Users u ON r.ParticipantId = u.UserId
                                     WHERE r.EventId = @EventId {where_extra}
                                     ORDER BY r.OverallPosition ASC, r.ChipTime ASC;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);
                        run_query.Parameters.AddWithValue("@Limit", limit);

                        if (category_id.HasValue)
                            run_query.Parameters.AddWithValue("@CategoryId", category_id.Value);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(MapResult(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_leaderboard] Error: {error.Message}");
                results = new List<Result>();
            }

            return results;
        }

        // ============================================================
        // GET RESULTS BY PARTICIPANT
        // ============================================================
        public List<Result> get_results_by_participant(int participant_id)
        {
            List<Result> results = new List<Result>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT r.ResultId, r.EnrolmentId, r.ParticipantId, r.EventId, r.CategoryId,
                                            r.GunTime, r.ChipTime, r.OverallPosition, r.CategoryPosition,
                                            r.Pace, r.Status, r.CreatedAt, r.UpdatedAt,
                                            e.EventName, c.CategoryName,
                                            u.FirstName, u.LastName, u.Email
                                     FROM Results r
                                     JOIN Events e ON r.EventId = e.EventId
                                     JOIN Categories c ON r.CategoryId = c.CategoryId
                                     JOIN Users u ON r.ParticipantId = u.UserId
                                     WHERE r.ParticipantId = @ParticipantId
                                     ORDER BY e.EventDate DESC;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@ParticipantId", participant_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(MapResult(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_results_by_participant] Error: {error.Message}");
                results = new List<Result>();
            }

            return results;
        }

        // ============================================================
        // HELPER: Map a SqlDataReader row to a Result
        // ============================================================
        private Result MapResult(SqlDataReader reader)
        {
            Result res = new Result
            {
                ResultId = Convert.ToInt32(reader["ResultId"]),
                EnrolmentId = Convert.ToInt32(reader["EnrolmentId"]),
                ParticipantId = Convert.ToInt32(reader["ParticipantId"]),
                EventId = Convert.ToInt32(reader["EventId"]),
                CategoryId = Convert.ToInt32(reader["CategoryId"]),
                GunTime = reader["GunTime"] == DBNull.Value ? (TimeSpan?)null : (TimeSpan)reader["GunTime"],
                ChipTime = reader["ChipTime"] == DBNull.Value ? (TimeSpan?)null : (TimeSpan)reader["ChipTime"],
                OverallPosition = reader["OverallPosition"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["OverallPosition"]),
                CategoryPosition = reader["CategoryPosition"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CategoryPosition"]),
                Pace = reader["Pace"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["Pace"]),
                Status = reader["Status"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
            };

            res.Event = new Event
            {
                EventId = res.EventId,
                EventName = reader["EventName"].ToString()
            };

            res.Category = new Category
            {
                CategoryId = res.CategoryId,
                CategoryName = reader["CategoryName"].ToString()
            };

            res.Participant = new User
            {
                UserId = res.ParticipantId,
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                Email = reader["Email"].ToString()
            };

            return res;
        }
    }
}