using System.Data.SqlClient;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Database
{
    public class UserDatabase
    {
        private string connection_string = @"Data Source=(localdb)\api_raceday;Initial Catalog=raceday_eventDB;Integrated Security=True;TrustServerCertificate=True;";

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

                    string query = "SELECT * FROM Users WHERE UserId = @UserId;";

                    SqlCommand run_query = new SqlCommand(query, connect);
                    run_query.Parameters.AddWithValue("@UserId", user_id);

                    SqlDataReader reader = run_query.ExecuteReader();

                    if (reader.Read())
                    {
                        user = new User
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            Email = reader["Email"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            DateOfBirth = reader["DateOfBirth"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DateOfBirth"]),
                            Role = reader["Role"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                        };
                    }

                    reader.Close();
                    connect.Close();
                }
            }
            catch (Exception error)
            {
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
                                     SET FirstName = @FirstName, 
                                         LastName = @LastName, 
                                         Phone = @Phone, 
                                         DateOfBirth = @DateOfBirth,
                                         UpdatedAt = GETDATE()
                                     WHERE UserId = @UserId;";

                    SqlCommand run_query = new SqlCommand(query, connect);

                    run_query.Parameters.AddWithValue("@FirstName", first_name);
                    run_query.Parameters.AddWithValue("@LastName", last_name);
                    run_query.Parameters.AddWithValue("@Phone", phone);
                    run_query.Parameters.AddWithValue("@DateOfBirth", date_of_birth);
                    run_query.Parameters.AddWithValue("@UserId", user_id);

                    run_query.ExecuteNonQuery();

                    message = "Profile updated successfully...";
                    connect.Close();
                }
            }
            catch (Exception error)
            {
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

                    string query = "SELECT * FROM Events WHERE OrganiserId = @OrganiserId ORDER BY EventDate DESC;";

                    SqlCommand run_query = new SqlCommand(query, connect);
                    run_query.Parameters.AddWithValue("@OrganiserId", organiser_id);

                    SqlDataReader reader = run_query.ExecuteReader();

                    while (reader.Read())
                    {
                        events.Add(new Event
                        {
                            EventId = Convert.ToInt32(reader["EventId"]),
                            EventName = reader["EventName"].ToString(),
                            Description = reader["Description"].ToString(),
                            OrganiserId = Convert.ToInt32(reader["OrganiserId"]),
                            Location = reader["Location"].ToString(),
                            Venue = reader["Venue"].ToString(),
                            EventDate = Convert.ToDateTime(reader["EventDate"]),
                            RegistrationDeadline = reader["RegistrationDeadline"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["RegistrationDeadline"]),
                            EntryFee = Convert.ToDecimal(reader["EntryFee"]),
                            MaxParticipants = reader["MaxParticipants"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["MaxParticipants"]),
                            EventType = reader["EventType"].ToString(),
                            Distance = reader["Distance"].ToString(),
                            Status = reader["Status"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                        });
                    }

                    reader.Close();
                    connect.Close();
                }
            }
            catch (Exception error)
            {
                events = new List<Event>();
            }

            return events;
        }
    }
}