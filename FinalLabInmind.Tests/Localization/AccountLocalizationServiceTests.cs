using FinalLabInmind.DbContext;
using FinalLabInmind.Resources;
using FinalLabInmind.Services.AccountLocalizationService;
using LoggingMicroservice.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;

namespace FinalLabInmind.Tests.Localization;

public class AccountLocalizationServiceTests
{
    private readonly Mock<IAppDbContext> _contextMock;
    private readonly Mock<IStringLocalizer<AccountDetails>> _localizerMock;
    private readonly AccountLocalizationService _service;

    public AccountLocalizationServiceTests()
    {
        _contextMock = new Mock<IAppDbContext>();
        _localizerMock = new Mock<IStringLocalizer<AccountDetails>>();
        var mockAccountSet = new Mock<DbSet<Account>>();
        var account = new Account { Id = 1, AccountName = "Elie" };
        mockAccountSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(account);

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountSet.Object);

        _service = new AccountLocalizationService(_contextMock.Object, _localizerMock.Object);
    }

    [Fact]
    public async Task GetLocalizedAccountDetailsAsync_ReturnsLocalizedValue()
    {
        // Arrange
        _localizerMock
            .Setup(l => l["Elie"])
            .Returns(new LocalizedString("Elie", "Elie ES"));

        // Act
        var result = await _service.GetLocalizedAccountDetailsAsync(1, "es");

        // Assert
        Assert.Equal("Elie ES", result);
    }

    [Fact]
    public async Task GetLocalizedAccountDetailsAsync_ReturnsDefault_WhenKeyNotFound()
    {
        // Arrange
        var accountId = 1L;

        var account = new Account
        {
            Id = accountId,
            AccountName = "NonExistingKey"
        };
        
        var data = new List<Account> { account }.AsQueryable();

        var dbSetMock = new Mock<DbSet<Account>>();
        dbSetMock.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(data.Provider);
        dbSetMock.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(data.Expression);
        dbSetMock.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(data.ElementType);
        dbSetMock.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        dbSetMock.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(account);

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock.Setup(c => c.Accounts).Returns(dbSetMock.Object);

        var localizerMock = new Mock<IStringLocalizer<AccountDetails>>();
        localizerMock.Setup(l => l[account.AccountName]).Returns(new LocalizedString(account.AccountName, ""));
        localizerMock.Setup(l => l["Account_Default_Details"]).Returns(new LocalizedString("Account_Default_Details", "Default Details"));

        var service = new AccountLocalizationService(dbContextMock.Object, localizerMock.Object);

        // Act
        var result = await service.GetLocalizedAccountDetailsAsync(accountId, "en");

        // Assert
        Assert.Equal("Default Details", result);
    }
}