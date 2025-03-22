using FinalLabInmind.Interfaces;
using LoggingMicroservice.Models;

namespace FinalLabInmind.Services.TransactionService;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;

    public TransactionService(IUnitOfWork unitOfWork, IMessagePublisher messagePublisher)
    {
        _unitOfWork = unitOfWork;
        _messagePublisher = messagePublisher;
    }

    public async Task<string> TransferFundsAsync(long fromAccountId, long toAccountId, decimal amount)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var fromAccount = await _unitOfWork.Accounts.GetByIdAsync(fromAccountId);
            var toAccount = await _unitOfWork.Accounts.GetByIdAsync(toAccountId);

            if (fromAccount == null || toAccount == null)
                throw new Exception("One or both accounts not found.");

            if (fromAccount.Balance < amount)
                throw new Exception("Insufficient funds in the source account.");

            // Adjust account balances
            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            await _unitOfWork.Accounts.UpdateAsync(fromAccount);
            await _unitOfWork.Accounts.UpdateAsync(toAccount);

            // Log transactions
            var withdrawalLog = new TransactionLog
            {
                AccountId = fromAccountId,
                TransactionType = "Withdrawal",
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                Status = "Completed",
                Details = $"Transferred {amount} to Account {toAccountId}"
            };

            var depositLog = new TransactionLog
            {
                AccountId = toAccountId,
                TransactionType = "Deposit",
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                Status = "Completed",
                Details = $"Received {amount} from Account {fromAccountId}"
            };

            await _unitOfWork.Transactions.AddAsync(withdrawalLog);
            await _unitOfWork.Transactions.AddAsync(depositLog);

            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitTransactionAsync();

            await _messagePublisher.PublishTransactionAsync(withdrawalLog);
            await _messagePublisher.PublishTransactionAsync(depositLog);

            return "Transfer successful.";
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return $"Transfer failed: {ex.Message}";
        }
    }
}