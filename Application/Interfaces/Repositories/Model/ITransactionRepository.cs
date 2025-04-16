using Application.Interfaces.Repositories.Base;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.Model
{
    public interface ITransactionRepository : ICreateRepository<Transaction>,
        IGetRepository<Transaction>,
        IDeleteRepository<Transaction>
    {
        public Task<IEnumerable<Transaction>> GetAllWithCategoriesAsync();
        public Task<IEnumerable<string>> GetTransactionMonthsAsync(int userId);
    }
}
