using LoggingMicroservice.Models;

namespace FinalLabInmind.Services.TransactionService;

public class TransactionService: ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

            var transactionFrom = new TransactionLog
            {
                AccountId = fromAccountId,
                TransactionType = "Withdrawal",
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                Status = "Completed",
                Details = $"Transferred {amount} to Account {toAccountId}"
            };

            var transactionTo = new TransactionLog
            {
                AccountId = toAccountId,
                TransactionType = "Deposit",
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                Status = "Completed",
                Details = $"Received {amount} from Account {fromAccountId}"
            };

            await _unitOfWork.Transactions.AddAsync(transactionFrom);
            await _unitOfWork.Transactions.AddAsync(transactionTo);

            await _unitOfWork.Accounts.UpdateAsync(fromAccount);
            await _unitOfWork.Accounts.UpdateAsync(toAccount);
            await _unitOfWork.SaveAsync();

            await _unitOfWork.CommitTransactionAsync();

            return "Transfer successful.";
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return $"Transfer failed: {ex.Message}";
        }
    }
}
