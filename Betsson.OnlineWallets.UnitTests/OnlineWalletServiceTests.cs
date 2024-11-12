using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Data.Repositories;
using Betsson.OnlineWallets.Services;
using Moq;

namespace Betsson.OnlineWallets.UnitTests
{
    public class OnlineWalletServiceTests
    {
        private Mock<IOnlineWalletRepository> _mockRepository;
        private OnlineWalletService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IOnlineWalletRepository>();
            _service = new OnlineWalletService(_mockRepository.Object);
        }

        /// <summary>
        /// Tests if GetBalance properly returns 0 balance when there are no transactions in the wallet.
        /// </summary>
        [Test]
        public async Task GetBalance_NoTransactions()
        {
            // First setup the mock repository to return a default/null wallet entry from the LastOnlineWalletEntryAsync function.
            // This mimicks the expected behaviour when a wallet has no transactions.
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync((OnlineWalletEntry)null);

            // Then get the result of GetBalanceAsync, and check if the result is the expected result (0).
            var balance = await _service.GetBalanceAsync();
            Assert.That(balance.Amount, Is.EqualTo(0));
        }

        [Test]
        public void GetBalance_HasTransactions()
        {
            Assert.Pass();
        }
    }
}