using Application.Interfaces.Services;
using Application.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeAccountingApp.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpPost]
        public async Task<IActionResult> AddCategory(string name, string type)
        {
            try
            {
                var currentUserId = User.GetCurrentUserId();
                await _categoryService.AddCategoryAsync(name, type, currentUserId);
                TempData["SuccessMessage"] = $"Категория '{name}' успешно добавлена!";
            }
            catch (ArgumentException)
            {
                return BadRequest("Неверный тип категории");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = $"Категория уже существует.";
            }

            return RedirectToAction("Index", "Category");
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? type = "all")
        {
            var categories = await _categoryService.GetByType(User.GetCurrentUserId(), type);
            return View(categories);
        }


        [HttpGet("by-type")]
        public async Task<IActionResult> GetByType([FromQuery] string? type = "all")
        {
            var categories = await _categoryService.GetByType(User.GetCurrentUserId(), type);
            return Ok(categories);
        }

        [HttpDelete("api/delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = User.GetCurrentUserId();
            await _categoryService.DeleteCategoryAsync(userId, id);
            return Ok();
        }

    }
}
