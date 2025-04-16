using Application.Interfaces.Repositories.Model;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeAccountingApp.Controllers
{
    [Authorize]
    public class StatisticController : Controller 
    {
        private readonly IUserRepository _userRepository;
        private readonly ITransactionRepository _trasactionRepository;
        public StatisticController(IUserRepository userRepository, ITransactionRepository trasactionRepository)
        {
            _trasactionRepository = trasactionRepository;
            _userRepository =userRepository;
        }
        [HttpGet("api/statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] string? month="all")
        {
            var username = User.Identity?.Name; // Fix the User context and add a semicolon

            var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Username == username);

            var query = _trasactionRepository.GetAll().Include(x => x.Category).AsQueryable();
            if (month == "all")
            {
                query = query
              .Where(x => x.UserId == user.Id && x.Date.Month == DateTime.Now.Month && x.Date.Year == DateTime.Now.Year);


            }
            else
            {
                var dateParts = month.Split('-');
                if (dateParts.Length == 2 && int.TryParse(dateParts[0], out int year) && int.TryParse(dateParts[1], out int monthNumber))
                {
                    query = query
                        .Where(x => x.UserId == user.Id && x.Date.Month == monthNumber && x.Date.Year == year);
                }
            }
            var transactions = await query.ToListAsync();

            var totalIncome = transactions.Where(x => x.Category.Type == ExpenseIncome.Income).Sum(x => x.Amount);
            var totalExpense = transactions.Where(x => x.Category.Type == ExpenseIncome.Expense).Sum(x => x.Amount);
            var balance = totalIncome - totalExpense;

            return Ok(new { totalIncome, totalExpense, balance });
        }
    }
}
