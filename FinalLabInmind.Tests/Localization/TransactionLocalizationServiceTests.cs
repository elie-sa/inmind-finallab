using FinalLabInmind.DbContext;
using FinalLabInmind.Resources;
using FinalLabInmind.Services.TransactionLocalizationService;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;

namespace FinalLabInmind.Tests.Localization;

public class TransactionLocalizationServiceTests
{
    private readonly Mock<IAppDbContext> _contextMock;
    private readonly Mock<IStringLocalizer<TransactionDetails>> _localizerMock;
    private readonly TransactionLocalizationService _service;

    public TransactionLocalizationServiceTests()
    {
        _contextMock = new Mock<IAppDbContext>();
        _localizerMock = new Mock<IStringLocalizer<TransactionDetails>>();
        _service = new TransactionLocalizationService(_contextMock.Object, _localizerMock.Object);
    }

    private static DbSet<TransactionLog> CreateMockDbSet(params TransactionLog[] logs)
    {
        var queryable = logs.AsQueryable();
        var dbSetMock = new Mock<DbSet<TransactionLog>>();

        dbSetMock.As<IQueryable<TransactionLog>>().Setup(m => m.Provider).Returns(queryable.Provider);
        dbSetMock.As<IQueryable<TransactionLog>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSetMock.As<IQueryable<TransactionLog>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSetMock.As<IQueryable<TransactionLog>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        dbSetMock.Setup(d => d.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((object[] ids) => logs.FirstOrDefault(t => t.Id == (long)ids[0]));

        return dbSetMock.Object;
    }

    [Fact]
    public async Task GetLocalizedTransactionNotificationAsync_ReturnsLocalizedValue()
    {
        // Arrange
        var transaction = new TransactionLog { Id = 5, TransactionType = "Deposit" };
        var mockDbSet = CreateMockDbSet(transaction);

        _contextMock.Setup(c => c.TransactionLogs).Returns(mockDbSet);
        _localizerMock.Setup(l => l["Deposit"]).Returns(new LocalizedString("Deposit", "Depósito Realizado"));

        // Act
        var result = await _service.GetLocalizedTransactionNotificationAsync(5, "es");

        // Assert
        Assert.Equal("Depósito Realizado", result);
    }

    [Fact]
    public async Task GetLocalizedTransactionNotificationAsync_ReturnsDefault_WhenKeyNotFound()
    {
        // Arrange
        var transaction = new TransactionLog { Id = 7, TransactionType = "UnknownType" };
        var mockDbSet = CreateMockDbSet(transaction);

        _contextMock.Setup(c => c.TransactionLogs).Returns(mockDbSet);

        _localizerMock.Setup(l => l["UnknownType"]).Returns(new LocalizedString("UnknownType", "", true));
        _localizerMock.Setup(l => l["Transaction_Default_Details"]).Returns(new LocalizedString("Transaction_Default_Details", "Default Notification"));

        // Act
        var result = await _service.GetLocalizedTransactionNotificationAsync(7, "de");

        // Assert
        Assert.Equal("Default Notification", result);
    }

    [Fact]
    public async Task GetLocalizedTransactionNotificationAsync_Throws_WhenTransactionNotFound()
    {
        // Arrange
        var mockDbSet = CreateMockDbSet(); // Empty
        _contextMock.Setup(c => c.TransactionLogs).Returns(mockDbSet);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetLocalizedTransactionNotificationAsync(999, "en"));

        Assert.Equal("Transaction with ID 999 not found.", ex.Message);
    }
}