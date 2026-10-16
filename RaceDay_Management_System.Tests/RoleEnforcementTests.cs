using System.Net;
using Xunit;

namespace RaceDay_Management_System.Tests
{
    public class RoleEnforcementTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private const string OrganiserEmail = "testorganiser_roles@raceday-tests.com";
        private const string ParticipantEmail = "testparticipant_roles@raceday-tests.com";
        private const string TestPassword = "Test1234";

        public RoleEnforcementTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            TestSeedHelper.DeleteUserByEmail(OrganiserEmail);
            TestSeedHelper.DeleteUserByEmail(ParticipantEmail);
        }

        private async Task RegisterAndLogin(string email, string role)
        {
            await _client.PostAsync(
                $"/api/auth/register?email={email}&password={TestPassword}&confirm_password={TestPassword}" +
                $"&first_name=Test&last_name=User&phone=0821234567&date_of_birth=1990-01-01&role={role}",
                null);

            await _client.PostAsync(
                $"/api/auth/login?email={email}&password={TestPassword}",
                null);
        }

        [Fact]
        public async Task GetAllUsers_AsParticipant_Returns403Forbidden()
        {
            await RegisterAndLogin(ParticipantEmail, "Participant");
            var response = await _client.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetAllUsers_AsOrganiser_Returns200OK()
        {
            await RegisterAndLogin(OrganiserEmail, "Organiser");
            var response = await _client.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAllUsers_Unauthenticated_Returns401Unauthorized()
        {
            var response = await _client.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_AsParticipant_Returns403Forbidden()
        {
            await RegisterAndLogin(ParticipantEmail, "Participant");
            var response = await _client.DeleteAsync("/api/categories/1");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetCurrentUser_Unauthenticated_Returns401Unauthorized()
        {
            var response = await _client.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetUserEnrolments_AsParticipantForOtherUser_Returns403Forbidden()
        {
            await RegisterAndLogin(ParticipantEmail, "Participant");
            var response = await _client.GetAsync("/api/users/1/enrolments");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetAllEvents_Unauthenticated_Returns200OK()
        {
            var response = await _client.GetAsync("/api/events");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}