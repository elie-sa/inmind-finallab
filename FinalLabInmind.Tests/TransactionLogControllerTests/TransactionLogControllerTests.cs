using FinalLabInmind.DTO;
using FinalLabInmind.Interfaces;
using FinalLabInmind.Resources;
using FinalLabInmind.Services.TransactionService;
using LoggingMicroservice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;

namespace FinalLabInmind.Tests.TransactionLogControllerTests;

public class TransactionLogControllerTests
    {
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly Mock<IStringLocalizer<AccountDetails>> _accountLocalizerMock;
        private readonly Mock<IStringLocalizer<TransactionDetails>> _transactionLocalizerMock;
        private readonly TestDbContext _context;

        public TransactionLogControllerTests()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TestDbContext(options);
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _transactionServiceMock = new Mock<ITransactionService>();
            _accountLocalizerMock = new Mock<IStringLocalizer<AccountDetails>>();
            _transactionLocalizerMock = new Mock<IStringLocalizer<TransactionDetails>>();
        }

        [Fact]
        public async Task LogTransaction_ShouldAddTransactionLog()
        {
            // Arrange
            var controller = new TransactionLogController(
                _context, 
                _messagePublisherMock.Object, 
                _transactionServiceMock.Object, 
                _accountLocalizerMock.Object, 
                _transactionLocalizerMock.Object
            );
            var transactionLog = new TransactionLog { AccountId = 1, TransactionType = "Deposit", Amount = 100, Status = "Completed", Details = "Hello" };

            // Act
            var result = await controller.LogTransaction(transactionLog);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTransactionLog = Assert.IsType<TransactionLog>(okResult.Value);
            Assert.Equal(transactionLog.AccountId, returnedTransactionLog.AccountId);
            Assert.Equal(transactionLog.TransactionType, returnedTransactionLog.TransactionType);
            Assert.Equal(transactionLog.Amount, returnedTransactionLog.Amount);
            Assert.Equal(transactionLog.Status, returnedTransactionLog.Status);
            Assert.Equal(transactionLog.Details, returnedTransactionLog.Details);
            Assert.NotNull(returnedTransactionLog.Timestamp);
        }

        [Fact]
        public async Task GetTransactionLogsForAccount_ShouldReturnTransactionLogs()
        {
            // Arrange
            var controller = new TransactionLogController(
                _context, 
                _messagePublisherMock.Object, 
                _transactionServiceMock.Object, 
                _accountLocalizerMock.Object, 
                _transactionLocalizerMock.Object
            );
            var accountId = 1;
            _context.TransactionLogs.Add(new TransactionLog { AccountId = accountId, TransactionType = "Deposit", Amount = 100, Status = "Completed", Details = "Deposit details" });
            _context.TransactionLogs.Add(new TransactionLog { AccountId = accountId, TransactionType = "Withdrawal", Amount = 50, Status = "Completed", Details = "Withdrawal details" });
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.GetTransactionLogsForAccount(accountId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var transactionLogs = Assert.IsType<List<TransactionLog>>(okResult.Value);
            Assert.Equal(2, transactionLogs.Count);
            Assert.Equal("Deposit details", transactionLogs[0].Details);
            Assert.Equal("Withdrawal details", transactionLogs[1].Details);
        }
        
        [Fact]
        public async Task GetTransactionLogs_ShouldReturnTransactionLogs()
        {
            // Arrange
            var controller = new TransactionLogController(
                _context, 
                _messagePublisherMock.Object, 
                _transactionServiceMock.Object, 
                _accountLocalizerMock.Object, 
                _transactionLocalizerMock.Object
            );
            _context.TransactionLogs.Add(new TransactionLog { AccountId = 1, TransactionType = "Deposit", Amount = 100, Status = "Completed", Details = "Deposit details" });
            _context.TransactionLogs.Add(new TransactionLog { AccountId = 2, TransactionType = "Withdrawal", Amount = 50, Status = "Completed", Details = "Withdrawal details" });
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.GetTransactionLogs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var transactionLogs = Assert.IsAssignableFrom<IQueryable<TransactionLog>>(okResult.Value);
            Assert.Equal(2, transactionLogs.Count());
            _messagePublisherMock.Verify(m => m.PublishTransactionAsync(It.IsAny<TransactionLog>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetCommonTransactions_ShouldReturnCommonTransactions()
        {
            // Arrange
            var controller = new TransactionLogController(
                _context, 
                _messagePublisherMock.Object, 
                _transactionServiceMock.Object, 
                _accountLocalizerMock.Object, 
                _transactionLocalizerMock.Object
            );
            var accountIds = new List<long> { 1, 2 };
            _context.TransactionLogs.Add(new TransactionLog { AccountId = 1, TransactionType = "Deposit", Amount = 100, Status = "Completed", Details = "Deposit details" });
            _context.TransactionLogs.Add(new TransactionLog { AccountId = 2, TransactionType = "Deposit", Amount = 100, Status = "Completed", Details = "Deposit details" });
            _context.TransactionLogs.Add(new TransactionLog { AccountId = 1, TransactionType = "Withdrawal", Amount = 50, Status = "Completed", Details = "Withdrawal details" });
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.GetCommonTransactions(accountIds);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var commonTransactions = Assert.IsType<List<TransactionLog>>(okResult.Value);
            Assert.Single(commonTransactions);
            Assert.Equal("Deposit", commonTransactions[0].TransactionType);
            Assert.Equal(100, commonTransactions[0].Amount);
        }

        /*[Fact]
        public async Task GetAccountBalanceSummary_ShouldReturnBalanceSummary()
        {
            // Arrange
            var controller = new TransactionLogController(
                _context, 
                _messagePublisherMock.Object, 
                _transactionServiceMock.Object, 
                _accountLocalizerMock.Object, 
                _transactionLocalizerMock.Object
            );
            var customerId = 1;
            _context.Accounts.Add(new Account { Id = 1, CustomerId = customerId, AccountName = "Account 1" });
            _context.Accounts.Add(new Account { Id = 2, CustomerId = customerId, AccountName = "Account 2" });
            _context.TransactionLogs.Add(new TransactionLog { AccountId = 1, TransactionType = "Deposit", Amount = 100, Status = "Completed", Details = "Deposit details" });
            _context.TransactionLogs.Add(new TransactionLog { AccountId = 1, TransactionType = "Withdrawal", Amount = 50, Status = "Completed", Details = "Withdrawal details" });
            _context.TransactionLogs.Add(new TransactionLog { AccountId = 2, TransactionType = "Deposit", Amount = 200, Status = "Completed", Details = "Deposit details" });
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.GetAccountBalanceSummary(customerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var balanceSummary = okResult.Value as IEnumerable<AccountBalanceSummary>;
            Assert.NotNull(balanceSummary);
            var balanceSummaryList = balanceSummary.ToList();
            Assert.Equal(2, balanceSummaryList.Count);
            Assert.Equal(100, balanceSummaryList[0].TotalDeposits);
            Assert.Equal(50, balanceSummaryList[0].TotalWithdrawals);
            Assert.Equal(50, balanceSummaryList[0].CurrentBalance);
            Assert.Equal(200, balanceSummaryList[1].TotalDeposits);
            Assert.Equal(0, balanceSummaryList[1].TotalWithdrawals);
            Assert.Equal(200, balanceSummaryList[1].CurrentBalance);
        }*/
        
        [Fact]
    public async Task CreateAccount_ShouldReturnOk_WhenAccountIsValid()
    {
        var controller = new TransactionLogController(_context, _messagePublisherMock.Object, _transactionServiceMock.Object, _accountLocalizerMock.Object, _transactionLocalizerMock.Object);
        var account = new Account { CustomerId = 1, AccountName = "Test Account" };

        var result = await controller.CreateAccount(account);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAccountById_ShouldReturnAccount_WhenAccountExists()
    {
        var account = new Account { CustomerId = 1, AccountName = "Test Account" };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var controller = new TransactionLogController(_context, _messagePublisherMock.Object, _transactionServiceMock.Object, _accountLocalizerMock.Object, _transactionLocalizerMock.Object);
        var result = await controller.GetAccountById(account.Id);

        Assert.IsType<Account>(result.Value);
    }

    [Fact]
    public async Task TransferFunds_ShouldReturnOk_WhenTransferIsSuccessful()
    {
        _transactionServiceMock.Setup(service => service.TransferFundsAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<decimal>()))
            .ReturnsAsync("Transfer successful");

        var controller = new TransactionLogController(_context, _messagePublisherMock.Object, _transactionServiceMock.Object, _accountLocalizerMock.Object, _transactionLocalizerMock.Object);
        var request = new TransferRequest { FromAccountId = 1, ToAccountId = 2, Amount = 100 };

        var result = await controller.TransferFunds(request);

        Assert.IsType<OkObjectResult>(result);
    }
    
}