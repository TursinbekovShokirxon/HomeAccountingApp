using Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ICategoryService
    {
        public Task AddCategoryAsync(string name, string type, int userId);
        public Task<IEnumerable<CategoryDto>> GetByType(int userId, string type = "all");
        public Task DeleteCategoryAsync(int userId,int categoryId);
    }
}
