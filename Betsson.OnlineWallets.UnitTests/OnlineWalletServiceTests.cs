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
            _ = _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync((OnlineWalletEntry)null);

            // Then get the balance using GetBalanceAsync, and check if the result is the expected result (0).
            var balance = await _service.GetBalanceAsync();

            Assert.That(balance.Amount, Is.EqualTo(0));
        }

        /// <summary>
        /// Tests if GetBalance properly returns the balance based on the last wallet entry, if there are transactions in the wallet.
        /// </summary>
        [Test]
        public async Task GetBalance_HasTransactions()
        {
            // First create a fake last wallet entry to mimick transactions in the wallet.
            var lastWalletEntry = new OnlineWalletEntry();
            lastWalletEntry.Amount = 50;
            lastWalletEntry.BalanceBefore = 100;

            // Then setup the mock repository to return the fake wallet entry from the LastOnlineWalletEntryAsync function.
            // This mimicks the expected behaviour when a wallet has at least 1 transaction.
            _ = _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync(lastWalletEntry);

            // Then get the balance using GetBalanceAsync, and check if the result is the expected result (150).
            var balance = await _service.GetBalanceAsync();

            Assert.That(balance.Amount, Is.EqualTo(150));
        }
    }
}