using System.Data.SqlClient;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Database
{
    public class EventDatabase
    {
        private string connection_string = @"Data Source=(localdb)\api_raceday;Initial Catalog=raceday_eventDB;Integrated Security=True;TrustServerCertificate=True;";

        // ============================================================
        // GET ALL EVENTS
        // ============================================================
        public List<Event> get_all_events()
        {
            List<Event> events = new List<Event>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT e.EventId, e.EventName, e.Description, e.OrganiserId, e.Location, e.Venue,
                                            e.EventDate, e.RegistrationDeadline, e.EntryFee, e.MaxParticipants,
                                            e.EventType, e.Distance, e.Status, e.CreatedAt, e.UpdatedAt,
                                            u.FirstName AS OrganiserFirstName, u.LastName AS OrganiserLastName
                                     FROM Events e
                                     JOIN Users u ON e.OrganiserId = u.UserId
                                     ORDER BY e.EventDate DESC;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Event ev = new Event
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
                                };

                                ev.Organiser = new User
                                {
                                    UserId = ev.OrganiserId,
                                    FirstName = reader["OrganiserFirstName"].ToString(),
                                    LastName = reader["OrganiserLastName"].ToString()
                                };

                                events.Add(ev);
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_all_events] Error: {error.Message}");
                events = new List<Event>();
            }

            return events;
        }

        // ============================================================
        // GET EVENT BY ID
        // ============================================================
        public Event get_event_by_id(int event_id)
        {
            Event ev = null;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"SELECT e.EventId, e.EventName, e.Description, e.OrganiserId, e.Location, e.Venue,
                                            e.EventDate, e.RegistrationDeadline, e.EntryFee, e.MaxParticipants,
                                            e.EventType, e.Distance, e.Status, e.CreatedAt, e.UpdatedAt,
                                            u.FirstName AS OrganiserFirstName, u.LastName AS OrganiserLastName
                                     FROM Events e
                                     JOIN Users u ON e.OrganiserId = u.UserId
                                     WHERE e.EventId = @EventId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ev = new Event
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
                                };

                                ev.Organiser = new User
                                {
                                    UserId = ev.OrganiserId,
                                    FirstName = reader["OrganiserFirstName"].ToString(),
                                    LastName = reader["OrganiserLastName"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_event_by_id] Error: {error.Message}");
                ev = null;
            }

            return ev;
        }

        // ============================================================
        // CREATE EVENT
        // ============================================================
        public string create_event(string event_name, string description, int organiser_id, string location,
                                    string venue, string event_date, string registration_deadline,
                                    decimal entry_fee, int max_participants, string event_type, string distance)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"INSERT INTO Events (EventName, Description, OrganiserId, Location, Venue,
                                                         EventDate, RegistrationDeadline, EntryFee, MaxParticipants,
                                                         EventType, Distance, Status)
                                     VALUES (@EventName, @Description, @OrganiserId, @Location, @Venue,
                                             @EventDate, @RegistrationDeadline, @EntryFee, @MaxParticipants,
                                             @EventType, @Distance, 'Open');";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventName", event_name);
                        run_query.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? DBNull.Value : description);
                        run_query.Parameters.AddWithValue("@OrganiserId", organiser_id);
                        run_query.Parameters.AddWithValue("@Location", location);
                        run_query.Parameters.AddWithValue("@Venue", string.IsNullOrEmpty(venue) ? DBNull.Value : venue);
                        run_query.Parameters.AddWithValue("@EventDate", event_date);
                        run_query.Parameters.AddWithValue("@RegistrationDeadline", string.IsNullOrEmpty(registration_deadline) ? DBNull.Value : registration_deadline);
                        run_query.Parameters.AddWithValue("@EntryFee", entry_fee);
                        run_query.Parameters.AddWithValue("@MaxParticipants", max_participants);
                        run_query.Parameters.AddWithValue("@EventType", event_type);
                        run_query.Parameters.AddWithValue("@Distance", string.IsNullOrEmpty(distance) ? DBNull.Value : distance);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Event created successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[create_event] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // UPDATE EVENT
        // ============================================================
        public string update_event(int event_id, string event_name, string description, string location,
                                    string venue, string event_date, string registration_deadline,
                                    decimal entry_fee, int max_participants, string status)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"UPDATE Events
                                     SET EventName = @EventName,
                                         Description = @Description,
                                         Location = @Location,
                                         Venue = @Venue,
                                         EventDate = @EventDate,
                                         RegistrationDeadline = @RegistrationDeadline,
                                         EntryFee = @EntryFee,
                                         MaxParticipants = @MaxParticipants,
                                         Status = @Status,
                                         UpdatedAt = GETDATE()
                                     WHERE EventId = @EventId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventName", event_name);
                        run_query.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? DBNull.Value : description);
                        run_query.Parameters.AddWithValue("@Location", location);
                        run_query.Parameters.AddWithValue("@Venue", string.IsNullOrEmpty(venue) ? DBNull.Value : venue);
                        run_query.Parameters.AddWithValue("@EventDate", event_date);
                        run_query.Parameters.AddWithValue("@RegistrationDeadline", string.IsNullOrEmpty(registration_deadline) ? DBNull.Value : registration_deadline);
                        run_query.Parameters.AddWithValue("@EntryFee", entry_fee);
                        run_query.Parameters.AddWithValue("@MaxParticipants", max_participants);
                        run_query.Parameters.AddWithValue("@Status", status);
                        run_query.Parameters.AddWithValue("@EventId", event_id);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Event updated successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[update_event] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // UPDATE EVENT STATUS
        // ============================================================
        public string update_event_status(int event_id, string status)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"UPDATE Events
                                     SET Status = @Status, UpdatedAt = GETDATE()
                                     WHERE EventId = @EventId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@Status", status);
                        run_query.Parameters.AddWithValue("@EventId", event_id);

                        run_query.ExecuteNonQuery();
                    }

                    message = "Event status updated successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[update_event_status] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // DELETE EVENT
        // ============================================================
        public string delete_event(int event_id)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "DELETE FROM Events WHERE EventId = @EventId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);
                        run_query.ExecuteNonQuery();
                    }

                    message = "Event deleted successfully...";
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[delete_event] Error: {error.Message}");
                message = error.Message;
            }

            return message;
        }

        // ============================================================
        // GET ORGANISER ID FOR AN EVENT (ownership check)
        // ============================================================
        public int get_organiser_id(int event_id)
        {
            int organiser_id = 0;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "SELECT OrganiserId FROM Events WHERE EventId = @EventId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);

                        object result = run_query.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            organiser_id = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[get_organiser_id] Error: {error.Message}");
                organiser_id = 0;
            }

            return organiser_id;
        }
    }
}