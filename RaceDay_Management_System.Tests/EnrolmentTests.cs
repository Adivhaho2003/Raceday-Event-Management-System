using System.Net;
using Xunit;

namespace RaceDay_Management_System.Tests
{
    public class EnrolmentTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private const string OrganiserEmail = "testorganiser_enrol@raceday-tests.com";
        private const string ParticipantEmail = "testparticipant_enrol@raceday-tests.com";
        private const string TestPassword = "Test1234";

        public EnrolmentTests(CustomWebApplicationFactory<Program> factory)
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
        public async Task CreateEnrolment_AsParticipant_ProcessesRequest()
        {
            await RegisterAndLogin(ParticipantEmail, "Participant");

            var response = await _client.PostAsync(
                "/api/enrolments?event_id=1&event_category_id=1",
                null);

            Assert.True(
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Conflict,
                $"Unexpected status code: {response.StatusCode}");
        }

        [Fact]
        public async Task CreateEnrolment_AsOrganiser_Returns403Forbidden()
        {
            await RegisterAndLogin(OrganiserEmail, "Organiser");

            var response = await _client.PostAsync(
                "/api/enrolments?event_id=1&event_category_id=1",
                null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateEnrolment_Unauthenticated_Returns401Unauthorized()
        {
            var response = await _client.PostAsync(
                "/api/enrolments?event_id=1&event_category_id=1",
                null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}