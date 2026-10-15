using System.Data.SqlClient;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Database
{
    public class AuthDatabase
    {
        private string connection_string = @"Data Source=(localdb)\api_raceday;Initial Catalog=raceday_eventDB;Integrated Security=True;TrustServerCertificate=True;";

        // ============================================================
        // REGISTER USER
        // ============================================================
        public string register_user(string email, string password_hash, string first_name,
                                     string last_name, string phone, string date_of_birth, string role)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, DateOfBirth, Role) 
                                     VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Phone, @DateOfBirth, @Role);";

                    SqlCommand run_query = new SqlCommand(query, connect);

                    run_query.Parameters.AddWithValue("@Email", email);
                    run_query.Parameters.AddWithValue("@PasswordHash", password_hash);
                    run_query.Parameters.AddWithValue("@FirstName", first_name);
                    run_query.Parameters.AddWithValue("@LastName", last_name);
                    run_query.Parameters.AddWithValue("@Phone", phone);
                    run_query.Parameters.AddWithValue("@DateOfBirth", date_of_birth);
                    run_query.Parameters.AddWithValue("@Role", role);

                    run_query.ExecuteNonQuery();

                    message = "User registered successfully...";
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
        // CHECK IF EMAIL EXISTS
        // ============================================================
        public bool email_exists(string email)
        {
            bool exists = false;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email;";

                    SqlCommand run_query = new SqlCommand(query, connect);
                    run_query.Parameters.AddWithValue("@Email", email);

                    int count = Convert.ToInt32(run_query.ExecuteScalar());

                    if (count > 0)
                    {
                        exists = true;
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                exists = false;
            }

            return exists;
        }

        // ============================================================
        // LOGIN USER (Get user by email)
        // ============================================================
        public User login_user(string email)
        {
            User user = null;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "SELECT * FROM Users WHERE Email = @Email;";

                    SqlCommand run_query = new SqlCommand(query, connect);
                    run_query.Parameters.AddWithValue("@Email", email);

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
        // UPDATE PASSWORD
        // ============================================================
        public string update_password(int user_id, string password_hash)
        {
            string message = "";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = @"UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = GETDATE() WHERE UserId = @UserId;";

                    SqlCommand run_query = new SqlCommand(query, connect);
                    run_query.Parameters.AddWithValue("@PasswordHash", password_hash);
                    run_query.Parameters.AddWithValue("@UserId", user_id);

                    run_query.ExecuteNonQuery();

                    message = "Password updated successfully...";
                    connect.Close();
                }
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;
        }
    }
}