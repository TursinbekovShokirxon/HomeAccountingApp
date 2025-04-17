using Application.Interfaces.Repositories.Model;
using Domain.DTO;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HomeAccountingApp.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        public CategoryController(ICategoryRepository categoryRepository, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
        }
        [HttpPost]
        public async Task<IActionResult> AddCategory(string name, string type)
        {
            if (!Enum.TryParse(type, true, out ExpenseIncome expenseIncomeType))
                return BadRequest("Invalid category type");

            var currentUserId = GetCurrentUserId();
            var currentUser = await _userRepository.GetById(currentUserId);

            // Ищем категорию с таким же именем и типом (чтобы "Зарплата (доход)" и "Зарплата (расход)" не путались)
            var existingCategory = await _categoryRepository.GetAll()
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c =>
                    c.Name.ToLower() == name.ToLower() &&
                    c.Type == expenseIncomeType);
            
            if (existingCategory != null)
            {
                // Проверим, есть ли уже пользователь в категории
                if (!existingCategory.Users.Any(u => u.Id == currentUserId))
                {
                    existingCategory.Users.Add(currentUser);
                    await _categoryRepository.Create(existingCategory); // Не забудь про SaveChanges, если у тебя UnitOfWork
                    TempData["SuccessMessage"] = $"Пользователь успешно добавлен в категорию '{name}'!";
                }
                else
                    TempData["InfoMessage"] = $"Категория '{name}' уже привязана к пользователю.";

                return RedirectToAction("Index", "Home");
            }

            // Если категории нет — создаём новую
            var newCategory = new Category
            {
                Name = name,
                Type = expenseIncomeType,
                Users = new List<UserRegister> { currentUser }
            };

            await _categoryRepository.Create(newCategory);
            TempData["SuccessMessage"] = $"Категория '{name}' успешно добавлена!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }


        [HttpGet("by-type")]
        public async Task<IActionResult> GetByType([FromQuery] string? type="all")
            {
                var userId = GetCurrentUserId();

                var query = _categoryRepository
                    .GetAll()
                    .Include(c => c.Users)
                    .Where(c => c.Users.Any(u => u.Id == userId) || !c.Users.Any()) // категории пользователя ИЛИ общие
                    .Where(c => c.Name != "Другое");

            if (type == "all") {

                var responseAllCategory =await query.ToListAsync();
                return Ok(responseAllCategory);
            } 
                if (Enum.TryParse<ExpenseIncome>(type, true, out var parsedType))
                {
                    query = query.Where(c => c.Type == parsedType);
                }

                var result = await query
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Type = c.Type
                    })
                    .ToListAsync();

                return Ok(result);
            }


        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst("UserId")?.Value);
        }
    }
}
