using Application.Interfaces.Repositories.Model;
using Application.Interfaces.Services;
using Domain.DTO;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;

        public TransactionService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<bool> AddTransactionAsync(IndexTransactionModel model, int userId)
        {
            var category = await _categoryRepository.GetAll()
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == model.CategoryId);

            if (category == null || (category.Users?.Any() == true && !category.Users.Any(u => u.Id == userId)))
                return false;

            var newTransaction = new Transaction
            {
                UserId = userId,
                CategoryId = model.CategoryId,
                Amount = model.Ammount,
                Comment = model.Comment,
                Date = model.Date,
                Category = category
            };

            await _transactionRepository.Create(newTransaction);
            return true;
        }

        public async Task<List<string>> GetAvailableMonthsAsync(int userId)
        {
            var months = await _transactionRepository.GetAll()
                .Where(t => t.UserId == userId)
                .Select(t => new { t.Date.Year, t.Date.Month })
                .Distinct()
                .OrderByDescending(e => e.Year).ThenByDescending(e => e.Month)
                .Select(m => $"{m.Year:D4}-{m.Month:D2}")
                .ToListAsync();

            return months;
        }

        public async Task<List<object>> GetFilteredTransactionsAsync(int userId, TransactionFilterDTO filterDTO)
        {
            var query = _transactionRepository.GetAll()
                .Include(t => t.Category)
                .Where(t => t.UserId == userId);

            if (filterDTO.categories.Any())
            {
                var hasOther = filterDTO.categories.Contains(-1);
                query = query.Where(t =>
                    (hasOther && (t.CategoryId == 9 || t.CategoryId == 10)) ||
                    filterDTO.categories.Contains(t.CategoryId));
            }

            if (filterDTO.month != "all" &&
                DateOnly.TryParseExact(filterDTO.month + "-01", "yyyy-MM-dd", null,
                System.Globalization.DateTimeStyles.None, out var monthStart))
            {
                var monthEnd = monthStart.AddMonths(1);
                query = query.Where(t => t.Date >= monthStart && t.Date < monthEnd);
            }

            if (Enum.TryParse<ExpenseIncome>(filterDTO.type, true, out var parsedType))
            {
                query = query.Where(t => t.Category.Type == parsedType);
            }

            return await query
                .OrderByDescending(t => t.Date)
                .Take(filterDTO.limit)
                .Select(t => new
                {
                    t.Id,
                    t.Date,
                    CategoryName = t.Category.Name,
                    Type = t.Category.Type,
                    t.Amount,
                    t.Comment
                })
                .ToListAsync<object>();
        }

        public async Task<bool> DeleteTransactionAsync(int transactionId, int userId)
        {
            var transaction = await _transactionRepository.GetAll()
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

            if (transaction == null)
                return false;

            await _transactionRepository.Delete(transaction.Id);
            return true;
        }
    }

}
