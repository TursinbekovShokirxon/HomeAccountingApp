using Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ITransactionService
    {
        Task<bool> AddTransactionAsync(IndexTransactionModel model, int userId);
        Task<List<string>> GetAvailableMonthsAsync(int userId);
        Task<List<object>> GetFilteredTransactionsAsync(int userId, TransactionFilterDTO filterDTO);
        Task<bool> DeleteTransactionAsync(int transactionId, int userId);
    }
}
