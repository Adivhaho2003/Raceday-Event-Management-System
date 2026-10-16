using System.Data.SqlClient;

namespace RaceDay_Management_System.Tests
{
    public static class TestSeedHelper
    {
        private static readonly string connection_string =
            @"Data Source=(localdb)\api_raceday;Initial Catalog=raceday_eventDB;Integrated Security=True;TrustServerCertificate=True;";

        public static void DeleteUserByEmail(string email)
        {
            try
            {
                using (var connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    int userId = 0;
                    using (var cmd = new SqlCommand("SELECT UserId FROM Users WHERE Email = @Email;", connect))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        var r = cmd.ExecuteScalar();
                        if (r == null || r == DBNull.Value) return;
                        userId = Convert.ToInt32(r);
                    }

                    string[] cleanupQueries = {
                        "DELETE FROM Results WHERE ParticipantId = @UserId;",
                        "DELETE FROM Results WHERE EventId IN (SELECT EventId FROM Events WHERE OrganiserId = @UserId);",
                        "DELETE FROM Enrolments WHERE ParticipantId = @UserId;",
                        "DELETE FROM Enrolments WHERE EventId IN (SELECT EventId FROM Events WHERE OrganiserId = @UserId);",
                        "DELETE FROM WeatherData WHERE EventId IN (SELECT EventId FROM Events WHERE OrganiserId = @UserId);",
                        "DELETE FROM EventCategories WHERE EventId IN (SELECT EventId FROM Events WHERE OrganiserId = @UserId);",
                        "DELETE FROM Events WHERE OrganiserId = @UserId;",
                        "DELETE FROM Users WHERE UserId = @UserId;"
                    };

                    foreach (var q in cleanupQueries)
                    {
                        try
                        {
                            using (var cmd = new SqlCommand(q, connect))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TestSeedHelper.DeleteUserByEmail] {ex.Message}");
            }
        }
    }
}