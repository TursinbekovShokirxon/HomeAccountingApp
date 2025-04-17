using Application.Interfaces.Repositories.Model;
using Domain.DTO;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeAccountingApp.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;

        public TransactionController(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
            _transactionRepository = transactionRepository;
        }
        [HttpGet]
        public IActionResult List()
        {
            return View();
        }
        //Для добавления транзакции
        [HttpPost("add-transaction")]
        public async Task<IActionResult> AddTransaction([FromBody] IndexTransactionModel model)
        {
            int userId = GetCurrentUserId();
            var category = await _categoryRepository.GetAll()
                                                    .Include(c => c.Users)
                                                    .FirstOrDefaultAsync(c => c.Id == model.CategoryId);

            if (category == null)
                return BadRequest(new { Success = false, Error = "Категория не найдена" });

            // Проверка принадлежности пользователю
            if (category.Users != null && category.Users.Any() && !category.Users.Any(u => u.Id == userId))
                return Forbid("Вы не имеете доступа к этой категории");

            var newTransaction = new Transaction()
            {
                UserId = userId,
                CategoryId = model.CategoryId,
                Amount = model.Ammount,
                Comment = model.Comment,
                Date = model.Date,
                Category = category
            };

            await _transactionRepository.Create(newTransaction);
            return Ok(new { Success = true });


        }
        //для заполнения фильтра месяцев

        [HttpGet("api/months")]
        public async Task<IActionResult> GetMonths()
        {
            int userId = GetCurrentUserId();
            var months = _transactionRepository.GetAll()
                      .Where(t => t.UserId == userId)
                     .Select(t => new { t.Date.Year, t.Date.Month })
                     .Distinct()
                     .OrderByDescending(e => e.Year).ThenByDescending(e => e.Month);

            var result =await months
                .Select(m => $"{m.Year:D4}-{m.Month:D2}") // форматируем в C# после выборки
                .ToListAsync();
            return Ok(result);
        }

        //основной метод с фильтрацией
        [HttpGet("api/transactions")]
        public async Task<IActionResult> Transactions([FromQuery] TransactionFilterDTO filterDTO)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = _transactionRepository.GetAll()
                    .Include(t => t.Category)
                    .Where(t => t.UserId == userId);

                if (filterDTO.categories != null && filterDTO.categories.Any())
                    query = query.Where(t => filterDTO.categories.Contains(t.CategoryId));
                

                if (filterDTO.month != "all" && DateOnly.TryParseExact(filterDTO.month + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                {
                    var monthEnd = monthStart.AddMonths(1);
                    query = query.Where(t => t.Date >= monthStart && t.Date < monthEnd);
                }

                if (Enum.TryParse<ExpenseIncome>(filterDTO.type, true, out var parsedType))
                    query = query.Where(t => t.Category.Type == parsedType);
                

                // Пагинация
                var transactions = query
                    .OrderByDescending(t => t.Date)
                    .Take(filterDTO.limit);

                var result = transactions.Select(t => new 
                {
                    Id = t.Id,
                    Date = t.Date,
                    CategoryName = t.Category.Name,
                    Type = t.Category.Type,
                    Amount = t.Amount,
                    Comment = t.Comment
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении транзакций: " + ex.Message);
                return StatusCode(500, "Ошибка сервера при загрузке транзакций");
            }
        }


        //удаление транзакции
        [HttpDelete("api/delete/transactions")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _transactionRepository.GetAll()
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == GetCurrentUserId());

            if (transaction == null)
                return BadRequest(new { Success = false, Error = "Транзакция не найдена" });
            
            await _transactionRepository.Delete(transaction.Id);

            return Ok(new { Success = true });
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst("UserId")?.Value);
        }
    }
}
