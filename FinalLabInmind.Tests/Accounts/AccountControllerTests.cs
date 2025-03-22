using FinalLabInmind.Controllers;
using FinalLabInmind.Services.AccountLocalizationService;
using FinalLabInmind.Services.AccountService;
using LoggingMicroservice.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FinalLabInmind.Tests.Accounts;

public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly Mock<IAccountLocalizationService> _localizationServiceMock;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _accountServiceMock = new Mock<IAccountService>();
            _localizationServiceMock = new Mock<IAccountLocalizationService>();
            _controller = new AccountController(_accountServiceMock.Object, _localizationServiceMock.Object);
        }

        [Fact]
        public async Task CreateAccount_ValidAccount_ReturnsOk()
        {
            // Arrange
            var account = new Account { Id = 1, CustomerId = 100, AccountName = "Elie" };
            _accountServiceMock.Setup(s => s.CreateAccountAsync(account)).ReturnsAsync(account);

            // Act
            var result = await _controller.CreateAccount(account) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var returnedAccount = Assert.IsType<Account>(result.Value);
            Assert.Equal("Elie", returnedAccount.AccountName);
        }

        [Fact]
        public async Task GetAccountById_ExistingId_ReturnsAccount()
        {
            // Arrange
            var account = new Account { Id = 1, AccountName = "Andrea" };
            _accountServiceMock.Setup(s => s.GetAccountByIdAsync(1)).ReturnsAsync(account);

            // Act
            var result = await _controller.GetAccountById(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var returned = Assert.IsType<Account>(result.Value);
            Assert.Equal("Andrea", returned.AccountName);
        }

        [Fact]
        public async Task GetAccountDetails_ReturnsLocalizedValue()
        {
            // Arrange
            var localized = "Andrea ES";
            _localizationServiceMock
                .Setup(s => s.GetLocalizedAccountDetailsAsync(1, "es"))
                .ReturnsAsync(localized);

            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Request.Headers["Accept-Language"] = "es";

            // Act
            var result = await _controller.GetAccountDetails(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Andrea ES", result.Value);
        }

        [Fact]
        public async Task GetCommonTransactions_ReturnsTransactions()
        {
            // Arrange
            var list = new List<TransactionLog> {
                new TransactionLog { AccountId = 1, Amount = 50, TransactionType = "Deposit" }
            };
            _accountServiceMock.Setup(s => s.GetCommonTransactionsAsync(It.IsAny<List<long>>())).ReturnsAsync(list);

            // Act
            var result = await _controller.GetCommonTransactions(new List<long> { 1, 2 }) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var transactions = Assert.IsType<List<TransactionLog>>(result.Value);
            Assert.Single(transactions);
        }
    }