using System.Net;
using Xunit;

namespace RaceDay_Management_System.Tests
{
    public class AuthTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private const string TestEmail = "testuser_auth@raceday-tests.com";
        private const string TestPassword = "Test1234";

        public AuthTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            TestSeedHelper.DeleteUserByEmail(TestEmail);
        }

        [Fact]
        public async Task Register_NewUser_Returns201Created()
        {
            var response = await _client.PostAsync(
                $"/api/auth/register?email={TestEmail}&password={TestPassword}&confirm_password={TestPassword}" +
                $"&first_name=Test&last_name=User&phone=0821234567&date_of_birth=2000-01-01&role=Participant",
                null);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409Conflict()
        {
            await _client.PostAsync(
                $"/api/auth/register?email={TestEmail}&password={TestPassword}&confirm_password={TestPassword}" +
                $"&first_name=Test&last_name=User&phone=0821234567&date_of_birth=2000-01-01&role=Participant",
                null);

            var response = await _client.PostAsync(
                $"/api/auth/register?email={TestEmail}&password={TestPassword}&confirm_password={TestPassword}" +
                $"&first_name=Test&last_name=User&phone=0821234567&date_of_birth=2000-01-01&role=Participant",
                null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Register_PasswordMismatch_Returns400BadRequest()
        {
            var response = await _client.PostAsync(
                $"/api/auth/register?email=mismatch@raceday-tests.com&password=Pass1234&confirm_password=Different123" +
                $"&first_name=Test&last_name=User&phone=0821234567&date_of_birth=2000-01-01&role=Participant",
                null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_Returns200OK()
        {
            await _client.PostAsync(
                $"/api/auth/register?email={TestEmail}&password={TestPassword}&confirm_password={TestPassword}" +
                $"&first_name=Test&last_name=User&phone=0821234567&date_of_birth=2000-01-01&role=Participant",
                null);

            var response = await _client.PostAsync(
                $"/api/auth/login?email={TestEmail}&password={TestPassword}",
                null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_InvalidPassword_Returns401Unauthorized()
        {
            await _client.PostAsync(
                $"/api/auth/register?email={TestEmail}&password={TestPassword}&confirm_password={TestPassword}" +
                $"&first_name=Test&last_name=User&phone=0821234567&date_of_birth=2000-01-01&role=Participant",
                null);

            var response = await _client.PostAsync(
                $"/api/auth/login?email={TestEmail}&password=WrongPassword999",
                null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_UnregisteredEmail_Returns401Unauthorized()
        {
            var response = await _client.PostAsync(
                $"/api/auth/login?email=neverregistered@raceday-tests.com&password=Test1234",
                null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}