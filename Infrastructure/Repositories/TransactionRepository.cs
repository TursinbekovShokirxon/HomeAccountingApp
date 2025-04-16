using Application.Interfaces.Repositories.Model;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly HomeAccountingContext _context;

        public TransactionRepository(HomeAccountingContext context)
        {
            _context = context;
        }

        public async Task<bool> Create(Transaction obj)
        {
            await _context.Transactions.AddAsync(obj);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return false;
            }

            _context.Transactions.Remove(transaction);
            return await _context.SaveChangesAsync() > 0;
        }

        public IQueryable<Transaction> GetAll()
        {
            return _context.Transactions.AsQueryable();
        }

        public async Task<IEnumerable<Transaction>> GetAllWithCategoriesAsync()
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<Transaction> GetById(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task<IEnumerable<string>> GetTransactionMonthsAsync(int userId)
        {
         return await _context.Transactions
                .Where(t => t.UserId == userId)
                .Select(t => new DateTime(t.Date.Year, t.Date.Month, 1))
                .Distinct()
                .OrderBy(d => d)
                .Select(d => $"{d.Year} {d.Month}")

                .ToListAsync();
        }
    }
}
