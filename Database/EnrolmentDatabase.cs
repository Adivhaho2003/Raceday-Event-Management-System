using System.Data.SqlClient;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Database
{
    public class EnrolmentDatabase
    {
        private string connection_string = @"Data Source=(localdb)\api_raceday;Initial Catalog=raceday_eventDB;Integrated Security=True;TrustServerCertificate=True;";

        // ============================================================
        // GET ALL ENROLMENTS
        // ============================================================
        public List<Enrolment> get_all_enrolments()
        {
            List<Enrolment> enrolments = new List<Enrolment>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT en.EnrolmentId, en.EventId, en.ParticipantId, en.EventCategoryId,
                                            en.Status, en.RegistrationNumber, en.RegistrationDate,
                                            en.UpdatedAt, en.ConfirmationDate, en.PaymentStatus,
                                            e.EventName, c.CategoryName, 
                                            u.FirstName, u.LastName, u.Email
                                     FROM Enrolments en
                                     JOIN Events e ON en.EventId = e.EventId
                                     JOIN EventCategories ec ON en.EventCategoryId = ec.EventCategoryId
                                     JOIN Categories c ON ec.CategoryId = c.CategoryId
                                     JOIN Users u ON en.ParticipantId = u.UserId
                                     ORDER BY en.RegistrationDate DESC;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                enrolments.Add(MapEnrolment(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_all_enrolments] Error: {error.Message}");
                enrolments = new List<Enrolment>();
            }

            return enrolments;
        }

        // ============================================================
        // GET ENROLMENT BY ID
        // ============================================================
        public Enrolment get_enrolment_by_id(int enrolment_id)
        {
            Enrolment en = null;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT en.EnrolmentId, en.EventId, en.ParticipantId, en.EventCategoryId,
                                            en.Status, en.RegistrationNumber, en.RegistrationDate,
                                            en.UpdatedAt, en.ConfirmationDate, en.PaymentStatus,
                                            e.EventName, c.CategoryName,
                                            u.FirstName, u.LastName, u.Email
                                     FROM Enrolments en
                                     JOIN Events e ON en.EventId = e.EventId
                                     JOIN EventCategories ec ON en.EventCategoryId = ec.EventCategoryId
                                     JOIN Categories c ON ec.CategoryId = c.CategoryId
                                     JOIN Users u ON en.ParticipantId = u.UserId
                                     WHERE en.EnrolmentId = @EnrolmentId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EnrolmentId", enrolment_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                en = MapEnrolment(reader);
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
        // CHECK IF PARTICIPANT ALREADY ENROLLED IN EVENT
        // ============================================================
        public bool already_enrolled(int event_id, int participant_id)
        {
            bool exists = false;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "SELECT COUNT(*) FROM Enrolments WHERE EventId = @EventId AND ParticipantId = @ParticipantId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);
                        run_query.Parameters.AddWithValue("@ParticipantId", participant_id);

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
                Console.WriteLine($"[already_enrolled] Error: {error.Message}");
                exists = false;
            }

            return exists;
        }

        // ============================================================
        // GENERATE UNIQUE REGISTRATION NUMBER
        // ============================================================
        public string generate_registration_number()
        {
            string reg_number = "";
            bool is_unique = false;
            int attempts = 0;

            try
            {
                while (!is_unique && attempts < 10)
                {
                    reg_number = $"REG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        string query = "SELECT COUNT(*) FROM Enrolments WHERE RegistrationNumber = @RegNumber;";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@RegNumber", reg_number);

                            int count = Convert.ToInt32(run_query.ExecuteScalar());
                            if (count == 0) is_unique = true;
                        }
                    }

                    attempts++;
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[generate_registration_number] Error: {error.Message}");
            }

            return reg_number;
        }

        // ============================================================
        // CREATE ENROLMENT
        // ============================================================
        public string create_enrolment(int event_id, int participant_id, int event_category_id, string registration_number)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"INSERT INTO Enrolments (EventId, ParticipantId, EventCategoryId, Status, RegistrationNumber, PaymentStatus)
                                     VALUES (@EventId, @ParticipantId, @EventCategoryId, 'Registered', @RegistrationNumber, 'Pending');";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);
                        run_query.Parameters.AddWithValue("@ParticipantId", participant_id);
                        run_query.Parameters.AddWithValue("@EventCategoryId", event_category_id);
                        run_query.Parameters.AddWithValue("@RegistrationNumber", registration_number);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Enrolment created successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[create_enrolment] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // UPDATE ENROLMENT (Status + PaymentStatus)
        // ============================================================
        public string update_enrolment(int enrolment_id, string status, string payment_status)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"UPDATE Enrolments
                                     SET Status = @Status, 
                                         PaymentStatus = @PaymentStatus, 
                                         UpdatedAt = GETDATE()
                                     WHERE EnrolmentId = @EnrolmentId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@Status", status);
                        run_query.Parameters.AddWithValue("@PaymentStatus", payment_status);
                        run_query.Parameters.AddWithValue("@EnrolmentId", enrolment_id);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Enrolment updated successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[update_enrolment] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // DELETE ENROLMENT (Cancel)
        // ============================================================
        public string delete_enrolment(int enrolment_id)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "DELETE FROM Enrolments WHERE EnrolmentId = @EnrolmentId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EnrolmentId", enrolment_id);
                        run_query.ExecuteNonQuery();
                    }

                    message = "Enrolment cancelled successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[delete_enrolment] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // GET ALL ENROLMENTS FOR AN EVENT
        // ============================================================
        public List<Enrolment> get_enrolments_by_event(int event_id)
        {
            List<Enrolment> enrolments = new List<Enrolment>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT en.EnrolmentId, en.EventId, en.ParticipantId, en.EventCategoryId,
                                            en.Status, en.RegistrationNumber, en.RegistrationDate,
                                            en.UpdatedAt, en.ConfirmationDate, en.PaymentStatus,
                                            e.EventName, c.CategoryName,
                                            u.FirstName, u.LastName, u.Email
                                     FROM Enrolments en
                                     JOIN Events e ON en.EventId = e.EventId
                                     JOIN EventCategories ec ON en.EventCategoryId = ec.EventCategoryId
                                     JOIN Categories c ON ec.CategoryId = c.CategoryId
                                     JOIN Users u ON en.ParticipantId = u.UserId
                                     WHERE en.EventId = @EventId
                                     ORDER BY en.RegistrationDate DESC;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                enrolments.Add(MapEnrolment(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_enrolments_by_event] Error: {error.Message}");
                enrolments = new List<Enrolment>();
            }

            return enrolments;
        }

        // ============================================================
        // HELPER: Map a SqlDataReader row to an Enrolment
        // ============================================================
        private Enrolment MapEnrolment(SqlDataReader reader)
        {
            Enrolment en = new Enrolment
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

            en.Event = new Event
            {
                EventId = en.EventId,
                EventName = reader["EventName"].ToString()
            };

            en.Participant = new User
            {
                UserId = en.ParticipantId,
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                Email = reader["Email"].ToString()
            };

            en.EventCategory = new EventCategory
            {
                Category = new Category
                {
                    CategoryName = reader["CategoryName"].ToString()
                }
            };

            return en;
        }
    }
}