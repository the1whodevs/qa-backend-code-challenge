using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Data.Repositories;
using Betsson.OnlineWallets.Exceptions;
using Betsson.OnlineWallets.Models;
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
        public async Task GetBalance_WithoutTransactions_ReturnsZero()
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
        public async Task GetBalance_WithTransactions_ReturnsBalance()
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

        /// <summary>
        /// Tests if DepositFundsAsync (with a valid amount) calls 'InsertOnlineWalletEntry' exactly once, 
        /// and returns the correct balance.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DepositFunds_ValidAmount_ReturnsCorrectBalance()
        {
            // First create a fake wallet entry to mimick transactions in the wallet.
            var lastWalletEntry = new OnlineWalletEntry();
            lastWalletEntry.Amount = 50;
            lastWalletEntry.BalanceBefore = 100;

            // Then setup the mock repository to return the fake wallet entry from the LastOnlineWalletEntryAsync function.
            // This mimicks the expected behaviour when a wallet has at least 1 transaction.
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync(lastWalletEntry);

            // Then create a fake deposit.
            var deposit = new Deposit { Amount = 75 };

            // Then deposit the fake deposit and ensure the balance returned is the balance expected (225).
            var balance = await _service.DepositFundsAsync(deposit);

            Assert.That(balance.Amount, Is.EqualTo(225));

            // Finally check that InsertOnlineWalletEntryAsync is called exactly 1 time, as expected!
            _mockRepository.Verify(r => r.InsertOnlineWalletEntryAsync(It.Is<OnlineWalletEntry>(e => e.Amount == 75 && e.BalanceBefore == 150)), Times.Once);

        }

        /// <summary>
        /// Tests if WithdrawFundsAsync, when called with a withdrawal that the wallet has sufficient balance,
        /// updates the balance correctly, and returns it, as well as calls InsertOnlineWalletEntry exactly once.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task WithdrawFunds_SufficientBalance_ReturnsCorrectBalance()
        {
            // First create a fake wallet entry to mimick transactions in the wallet.
            var lastWalletEntry = new OnlineWalletEntry();
            lastWalletEntry.Amount = 50;
            lastWalletEntry.BalanceBefore = 100;

            // Then setup the mock repository to return the fake wallet entry when LastOnlineWalletEntryAsync function is called.
            // This mimicks the expected behaviour when a wallet has at least 1 transaction.
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync(lastWalletEntry);

            // Then creae a fake withdrawal.
            var withdrawal = new Withdrawal { Amount = 75 };

            // Then withdraw the fake withdrawal and ensure the balance returned is the balance expected (75).
            var balance = await _service.WithdrawFundsAsync(withdrawal);

            Assert.That(balance.Amount, Is.EqualTo(75));

            // Finally check that InsertOnlineWalletEntryAsync is called exactly 1 time, as expected!
            _mockRepository.Verify(r => r.InsertOnlineWalletEntryAsync(It.Is<OnlineWalletEntry>(e => e.Amount == -75 && e.BalanceBefore == 150)), Times.Once);
        }

        /// <summary>
        /// Tests if WithdrawFundsAsync, when called with a withdrawal that the wallet has insufficient balance,
        /// throws an InsufficientBalanceException.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task WithdrawFunds_InsufficientBalance_ThrowsException()
        {
            // First create a fake wallet entry to mimick transactions in the wallet.
            var lastWalletEntry = new OnlineWalletEntry();
            lastWalletEntry.Amount = 50;
            lastWalletEntry.BalanceBefore = 100;

            // Then setup the mock repository to return the fake wallet entry when LastOnlineWalletEntryAsync function is called.
            // This mimicks the expected behaviour when a wallet has at least 1 transaction.
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync(lastWalletEntry);

            // Then creae a fake withdrawal, with Amount greater than the lastWalletEntry.Amount + lastWalletEntry.BalanceBefore.
            var withdrawal = new Withdrawal { Amount = 250 };

            // Then check if withdrawing the fake withdrawal throws an insufficient balance exception.
            var insufficientBalanceExceptionThrown = false;

            try
            {
                _ = await _service.WithdrawFundsAsync(withdrawal);
            }
            catch (InsufficientBalanceException) { insufficientBalanceExceptionThrown = true; }

            Assert.That(insufficientBalanceExceptionThrown, Is.True);
        }
    }
}