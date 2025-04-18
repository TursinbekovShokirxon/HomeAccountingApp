using Application.Interfaces.Repositories.Model;
using Application.Interfaces.Services;
using Application.Utilities;
using Domain.DTO;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeAccountingApp.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public IActionResult List() => View();

        [HttpPost("add-transaction")]
        public async Task<IActionResult> AddTransaction([FromBody] IndexTransactionModel model)
        {
            var userId = User.GetCurrentUserId();
            var result = await _transactionService.AddTransactionAsync(model, userId);

            return result ? Ok(new { Success = true }) :
                            BadRequest(new { Success = false, Error = "Ошибка добавления транзакции" });
        }

        [HttpGet("api/months")]
        public async Task<IActionResult> GetMonths()
        {
            var userId = User.GetCurrentUserId();
            var months = await _transactionService.GetAvailableMonthsAsync(userId);
            return Ok(months);
        }

        [HttpGet("api/transactions")]
        public async Task<IActionResult> Transactions([FromQuery] TransactionFilterDTO filterDTO)
        {
            var userId = User.GetCurrentUserId();
            var result = await _transactionService.GetFilteredTransactionsAsync(userId, filterDTO);
            return Ok(result);
        }

        [HttpDelete("api/delete/transaction")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var userId = User.GetCurrentUserId();
            var result = await _transactionService.DeleteTransactionAsync(id, userId);

            return result ? Ok(new { Success = true }) :
                            BadRequest(new { Success = false, Error = "Транзакция не найдена" });
        }
    }

}
