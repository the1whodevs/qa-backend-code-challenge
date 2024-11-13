using NUnit.Framework;
using RestSharp;
using System.Threading.Tasks;
using System.Text.Json;
using Betsson.OnlineWallets.Web.Models;

namespace Betsson.OnlineWallets.E2ETests
{
    public class E2EEndpointTests
    {
        private RestClient _client;

        [SetUp]
        public void Setup()
        {
            // Create the RestClient and initialize with the base URL of the microservice.
            _client = new RestClient("http://localhost:5047");
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose of the RestClient after each test to avoid resource leaks.
            _client.Dispose();
        }

        /// <summary>
        /// Sends a GET request to read the balance of the wallet. Then ensures the response is successful,
        /// and balance is greater than or equal to 0.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetBalance_ReturnsExpectedBalance()
        {
            // First create the request, execute it and wait for the response.
            var request = new RestRequest("/onlinewallet/balance", Method.Get);
            var response = await _client.ExecuteAsync(request);

            // Then ensure the response is successful, with non-empty contents.
            Assert.That(response.IsSuccessful, Is.True, "API call should be successful");
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Empty, "Response should not have empty contents");

            // Then ensure the balance in the response is greater than or equal to 0, as expected.
            var balanceResponse = JsonSerializer.Deserialize<BalanceResponse>(response.Content);
            Assert.That(balanceResponse?.Amount, Is.GreaterThanOrEqualTo(0), "Balance should be zero or positive.");
        }

        /// <summary>
        /// Sends a POST request to deposit funds and thus increase the balance of the wallet. Then ensures the response is successful,
        /// non-empty and that the balance is greater than or equal to the deposit amount.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DepositFunds_IncreasesBalance()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Required because response key is 'amount' and class variable name is 'Amount'.
            };

            // First create a request, execute and wait for the response of GetBalance.
            var getBalanceRequest = new RestRequest("/onlinewallet/balance", Method.Get);
            var getBalanceResponse = await _client.ExecuteAsync(getBalanceRequest);

            // Deserialize with case-insensitive options
            var getBalance = JsonSerializer.Deserialize<BalanceResponse>(getBalanceResponse.Content, options);
            Console.WriteLine($"GetBalance Response JSON: {getBalanceResponse.Content}");

            var startingBalance = getBalance?.Amount;

            // Then create the request, execute it and wait for the response.
            var request = new RestRequest("/onlinewallet/deposit", Method.Post);
            var depositAmount = 100.0M;
            request.AddJsonBody(new { amount =  depositAmount });

            var response = await _client.ExecuteAsync(request);

            // Then ensure the response is successful, with non-empty contents.
            Assert.That(response.IsSuccessful, Is.True, "Deposit should be successful");
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Empty, "Response should not have empty contents");

            Console.WriteLine($"Deposit Response JSON: {response.Content}");

            // Then ensure the new balance is greater than or equal to the deposit amount.
            // Deserialize with case-insensitive options
            var depositBalanceResponse = JsonSerializer.Deserialize<BalanceResponse>(response.Content, options);
            Assert.That(depositBalanceResponse?.Amount, Is.EqualTo(depositAmount + startingBalance), $"Balance within deposit response should be equal to the deposited amount ({depositAmount}) + startingBalance ({startingBalance}).");

            // Then ensure GetBalance properly returns the expected balance.
            getBalanceResponse = await _client.ExecuteAsync(getBalanceRequest);
            // Deserialize with case-insensitive options
            getBalance = JsonSerializer.Deserialize<BalanceResponse>(getBalanceResponse.Content, options);
            Assert.That(getBalance?.Amount, Is.EqualTo(depositAmount + startingBalance), $"Balance from GetBalance after the deposit should be equal to the deposited amount ({depositAmount}) + starting balance ({startingBalance}).");
        }
    }
}