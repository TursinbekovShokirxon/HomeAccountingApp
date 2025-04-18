using Application.Interfaces.Repositories.Model;
using Application.Interfaces.Services;
using Domain.DTO;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;

        public CategoryService(ICategoryRepository categoryRepository, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task<IEnumerable<CategoryDto>> GetByType(int userId, string type = "all")
        {
            // Загружаем пользователя с категориями
            var user = await _userRepository
               .GetAll()
               .Include(u => u.Categories)
               .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Enumerable.Empty<CategoryDto>();

            var categories = user.Categories
                .Where(cat => cat.Name != "Другое");

            // Фильтрация по типу, если задан
            if (!string.Equals(type, "all", StringComparison.OrdinalIgnoreCase) &&
                Enum.TryParse<ExpenseIncome>(type, true, out var parsedType))
            {
                categories = categories.Where(cat => cat.Type == parsedType);
            }

            // Преобразуем в DTO
            var result = categories
                .Select(cat => new CategoryDto
                {
                    Id = cat.Id,
                    Name = cat.Name,
                    Type = cat.Type
                });

            return result;
        }


        public async Task AddCategoryAsync(string name, string type, int userId)
        {
            if (!Enum.TryParse(type, true, out ExpenseIncome expenseIncomeType))
                throw new ArgumentException("Invalid category type");

            var currentUser = await _userRepository.GetAll()
                .Include(x=>x.Categories)
                .FirstOrDefaultAsync(x=>x.Id==userId);

            var category = _categoryRepository.GetAll().FirstOrDefault(c =>
                    c.Name.ToLower() == name.ToLower() &&
                    c.Type == expenseIncomeType);

            if (category != null)
            {
                // Если категория уже существует, просто добавляем ее к пользователю
                if (currentUser.Categories.Any(c => c.Id == category.Id))
                    throw new Exception("Категория уже добавлена пользователю");
                currentUser.Categories.Add(category);
                await _userRepository.SaveChangesAsync();
                return;
            }
            var newCategory = new Category
            {
                Name = name,
                Type = expenseIncomeType,
                Users = new List<UserRegister> { currentUser }
            };

            currentUser.Categories.Add(newCategory);
          await  _userRepository.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int userId, int categoryId)
        {
            var user = await _userRepository
                .GetAll()
                .Include(u => u.Categories)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("Пользователь не найден");
            foreach (var c in user.Categories)
            {
                Console.WriteLine($"- {c.Id}: {c.Name}");
            }
            var categoryToRemove = user.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (categoryToRemove == null)
                throw new Exception("Категория не найдена у пользователя");

            user.Categories.Remove(categoryToRemove);
            await _userRepository.SaveChangesAsync(); // Убедись, что SaveChangesAsync реально вызывает context.SaveChangesAsync()
        }
    }
}
