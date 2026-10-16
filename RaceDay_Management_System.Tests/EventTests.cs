using System.Net;
using Xunit;

namespace RaceDay_Management_System.Tests
{
    public class EventTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private const string OrganiserEmail = "testorganiser_events@raceday-tests.com";
        private const string ParticipantEmail = "testparticipant_events@raceday-tests.com";
        private const string TestPassword = "Test1234";

        public EventTests(CustomWebApplicationFactory<Program> factory)
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
        public async Task CreateEvent_AsOrganiser_Returns201Created()
        {
            await RegisterAndLogin(OrganiserEmail, "Organiser");

            var response = await _client.PostAsync(
                $"/api/events?event_name=TestEvent_Auto&description=Test&location=TestCity&venue=TestVenue" +
                $"&event_date=2027-01-15 06:00:00&registration_deadline=2027-01-10 23:59:59" +
                $"&entry_fee=100&max_participants=500&event_type=Running&distance=10 km",
                null);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_Returns403Forbidden()
        {
            await RegisterAndLogin(ParticipantEmail, "Participant");

            var response = await _client.PostAsync(
                $"/api/events?event_name=ShouldFail&description=x&location=x&venue=x" +
                $"&event_date=2027-02-15 06:00:00&registration_deadline=2027-02-10 23:59:59" +
                $"&entry_fee=100&max_participants=100&event_type=Running&distance=5 km",
                null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_Unauthenticated_Returns401Unauthorized()
        {
            var response = await _client.PostAsync(
                $"/api/events?event_name=ShouldFail&description=x&location=x&venue=x" +
                $"&event_date=2027-03-15 06:00:00&registration_deadline=2027-03-10 23:59:59" +
                $"&entry_fee=100&max_participants=100&event_type=Running&distance=5 km",
                null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAllEvents_Public_Returns200OK()
        {
            var response = await _client.GetAsync("/api/events");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_InvalidEventType_Returns400BadRequest()
        {
            await RegisterAndLogin(OrganiserEmail, "Organiser");

            var response = await _client.PostAsync(
                $"/api/events?event_name=BadType&description=x&location=x&venue=x" +
                $"&event_date=2027-04-15 06:00:00&registration_deadline=2027-04-10 23:59:59" +
                $"&entry_fee=100&max_participants=100&event_type=Swimming&distance=5 km",
                null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}