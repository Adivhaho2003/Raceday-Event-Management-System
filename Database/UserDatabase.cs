using System.Data.SqlClient;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Database
{
    public class UserDatabase
    {
        private string connection_string = @"Data Source=(localdb)\api_raceday;Initial Catalog=raceday_eventDB;Integrated Security=True;TrustServerCertificate=True;";

        // ============================================================
        // GET ALL USERS (paginated)
        // Used by: UsersController.GetAllUsers
        // ============================================================
        public List<User> get_all_users(int page, int size, string? role, string? search)
        {
            List<User> users = new List<User>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string where_clause = "WHERE 1 = 1";
                    if (!string.IsNullOrEmpty(role))
                        where_clause += " AND Role = @Role";
                    if (!string.IsNullOrEmpty(search))
                        where_clause += " AND (FirstName LIKE @Search OR LastName LIKE @Search OR Email LIKE @Search)";

                    string query = $@"SELECT UserId, Email, PasswordHash, FirstName, LastName, 
                                            Phone, DateOfBirth, Role, ProfilePictureUrl, 
                                            CreatedAt, UpdatedAt
                                     FROM Users 
                                     {where_clause}
                                     ORDER BY CreatedAt DESC
                                     OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        if (!string.IsNullOrEmpty(role))
                            run_query.Parameters.AddWithValue("@Role", role);
                        if (!string.IsNullOrEmpty(search))
                            run_query.Parameters.AddWithValue("@Search", "%" + search + "%");

                        run_query.Parameters.AddWithValue("@Offset", (page - 1) * size);
                        run_query.Parameters.AddWithValue("@Size", size);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Email = reader["Email"].ToString(),
                                    PasswordHash = reader["PasswordHash"].ToString(),
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    Phone = reader["Phone"] == DBNull.Value ? null : reader["Phone"].ToString(),
                                    DateOfBirth = reader["DateOfBirth"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DateOfBirth"]),
                                    Role = reader["Role"].ToString(),
                                    ProfilePictureUrl = reader["ProfilePictureUrl"] == DBNull.Value ? null : reader["ProfilePictureUrl"].ToString(),
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
                Console.WriteLine($"[get_all_users] Error: {error.Message}");
                users = new List<User>();
            }

            return users;
        }

        // ============================================================
        // GET USER BY ID
        // ============================================================
        public User get_user_by_id(int user_id)
        {
            User user = null;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT UserId, Email, PasswordHash, FirstName, LastName, 
                                            Phone, DateOfBirth, Role, ProfilePictureUrl, 
                                            CreatedAt, UpdatedAt
                                     FROM Users 
                                     WHERE UserId = @UserId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@UserId", user_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User
                                {
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Email = reader["Email"].ToString(),
                                    PasswordHash = reader["PasswordHash"].ToString(),
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    Phone = reader["Phone"] == DBNull.Value ? null : reader["Phone"].ToString(),
                                    DateOfBirth = reader["DateOfBirth"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DateOfBirth"]),
                                    Role = reader["Role"].ToString(),
                                    ProfilePictureUrl = reader["ProfilePictureUrl"] == DBNull.Value ? null : reader["ProfilePictureUrl"].ToString(),
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
                Console.WriteLine($"[get_user_by_id] Error: {error.Message}");
                user = null;
            }

            return user;
        }

        // ============================================================
        // UPDATE USER PROFILE
        // ============================================================
        public string update_user_profile(int user_id, string first_name, string last_name, string phone, string date_of_birth)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"UPDATE Users 
                                     SET FirstName = @FirstName, LastName = @LastName, 
                                         Phone = @Phone, DateOfBirth = @DateOfBirth,
                                         UpdatedAt = GETDATE()
                                     WHERE UserId = @UserId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@FirstName", first_name);
                        run_query.Parameters.AddWithValue("@LastName", last_name);
                        run_query.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? DBNull.Value : phone);
                        run_query.Parameters.AddWithValue("@DateOfBirth", string.IsNullOrEmpty(date_of_birth) ? DBNull.Value : date_of_birth);
                        run_query.Parameters.AddWithValue("@UserId", user_id);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Profile updated successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[update_user_profile] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // UPDATE PROFILE PICTURE URL
        // ============================================================
        public string update_profile_picture(int user_id, string? profile_picture_url)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"UPDATE Users 
                                     SET ProfilePictureUrl = @ProfilePictureUrl, UpdatedAt = GETDATE()
                                     WHERE UserId = @UserId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        if (string.IsNullOrEmpty(profile_picture_url))
                            run_query.Parameters.AddWithValue("@ProfilePictureUrl", DBNull.Value);
                        else
                            run_query.Parameters.AddWithValue("@ProfilePictureUrl", profile_picture_url);

                        run_query.Parameters.AddWithValue("@UserId", user_id);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Profile picture updated successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[update_profile_picture] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // GET EVENTS BY ORGANISER
        // ============================================================
        public List<Event> get_events_by_organiser(int organiser_id)
        {
            List<Event> events = new List<Event>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT EventId, EventName, Description, OrganiserId, Location, Venue,
                                            EventDate, RegistrationDeadline, EntryFee, MaxParticipants,
                                            EventType, Distance, Status, CreatedAt, UpdatedAt
                                     FROM Events 
                                     WHERE OrganiserId = @OrganiserId 
                                     ORDER BY EventDate DESC;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@OrganiserId", organiser_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                events.Add(new Event
                                {
                                    EventId = Convert.ToInt32(reader["EventId"]),
                                    EventName = reader["EventName"].ToString(),
                                    Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                                    OrganiserId = Convert.ToInt32(reader["OrganiserId"]),
                                    Location = reader["Location"].ToString(),
                                    Venue = reader["Venue"] == DBNull.Value ? null : reader["Venue"].ToString(),
                                    EventDate = Convert.ToDateTime(reader["EventDate"]),
                                    RegistrationDeadline = reader["RegistrationDeadline"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["RegistrationDeadline"]),
                                    EntryFee = Convert.ToDecimal(reader["EntryFee"]),
                                    MaxParticipants = reader["MaxParticipants"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["MaxParticipants"]),
                                    EventType = reader["EventType"].ToString(),
                                    Distance = reader["Distance"] == DBNull.Value ? null : reader["Distance"].ToString(),
                                    Status = reader["Status"].ToString(),
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
                Console.WriteLine($"[get_events_by_organiser] Error: {error.Message}");
                events = new List<Event>();
            }

            return events;
        }

        // ============================================================
        // GET ENROLMENTS BY PARTICIPANT
        // ============================================================
        public List<Enrolment> get_enrolments_by_participant(int participant_id)
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
                                            e.EventName, c.CategoryName
                                     FROM Enrolments en 
                                     JOIN Events e ON en.EventId = e.EventId 
                                     JOIN EventCategories ec ON en.EventCategoryId = ec.EventCategoryId 
                                     JOIN Categories c ON ec.CategoryId = c.CategoryId 
                                     WHERE en.ParticipantId = @ParticipantId 
                                     ORDER BY en.RegistrationDate DESC;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@ParticipantId", participant_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
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

                                en.EventCategory = new EventCategory
                                {
                                    Category = new Category
                                    {
                                        CategoryName = reader["CategoryName"].ToString()
                                    }
                                };

                                enrolments.Add(en);
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_enrolments_by_participant] Error: {error.Message}");
                enrolments = new List<Enrolment>();
            }

            return enrolments;
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
                                            e.EventName, c.CategoryName
                                     FROM Results r 
                                     JOIN Events e ON r.EventId = e.EventId 
                                     JOIN Categories c ON r.CategoryId = c.CategoryId 
                                     WHERE r.ParticipantId = @ParticipantId 
                                     ORDER BY e.EventDate DESC;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@ParticipantId", participant_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
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

                                results.Add(res);
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
    }
}