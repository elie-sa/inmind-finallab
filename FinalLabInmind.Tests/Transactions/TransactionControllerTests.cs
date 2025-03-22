using FinalLabInmind.DTOs;
using FinalLabInmind.Services.TransactionLocalizationService;
using FinalLabInmind.Services.TransactionLogService;
using FinalLabInmind.Services.TransactionService;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FinalLabInmind.Tests.Transactions;

public class TransactionControllerTests
{
    private readonly Mock<ITransactionLogService> _transactionLogServiceMock;
    private readonly Mock<ITransactionService> _transactionServiceMock;
    private readonly Mock<ITransactionLocalizationService> _transactionLocalizationServiceMock;
    private readonly TransactionLogController _controller;

    public TransactionControllerTests()
    {
        _transactionLogServiceMock = new Mock<ITransactionLogService>();
        _transactionServiceMock = new Mock<ITransactionService>();
        _transactionLocalizationServiceMock = new Mock<ITransactionLocalizationService>();

        _controller = new TransactionLogController(
            _transactionLogServiceMock.Object,
            _transactionServiceMock.Object,
            _transactionLocalizationServiceMock.Object
        );
    }

    [Fact]
    public async Task LogTransaction_ShouldReturnOk_WhenValid()
    {
        // Arrange
        var dto = new TransactionLogDto
        {
            AccountId = 1,
            TransactionType = "Deposit",
            Amount = 100,
            Status = "Completed",
            Details = "Test"
        };

        _transactionLogServiceMock
            .Setup(s => s.LogTransactionAsync(dto))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.LogTransaction(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task GetTransactionLogsForAccount_ShouldReturnOk_WhenLogsExist()
    {
        // Arrange
        var accountId = 5;
        var logs = new List<TransactionLogDto>
        {
            new TransactionLogDto { AccountId = accountId, Amount = 50, TransactionType = "Deposit" }
        };

        _transactionLogServiceMock
            .Setup(s => s.GetTransactionLogsForAccountAsync(accountId))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetTransactionLogsForAccount(accountId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsAssignableFrom<IEnumerable<TransactionLogDto>>(okResult.Value);
        Assert.Single(value);
    }

    [Fact]
    public async Task GetTransactionLogsForAccount_ShouldReturnNotFound_WhenNoLogs()
    {
        // Arrange
        var accountId = 999;
        _transactionLogServiceMock
            .Setup(s => s.GetTransactionLogsForAccountAsync(accountId))
            .ReturnsAsync(new List<TransactionLogDto>()); // simulate no logs

        // Act
        var result = await _controller.GetTransactionLogsForAccount(accountId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No transaction logs found for this account.", notFoundResult.Value);
    }
}