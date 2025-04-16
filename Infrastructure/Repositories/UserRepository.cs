using Application.Interfaces.Repositories.Model;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        public HomeAccountingContext _db { get; set; }

        public UserRepository(HomeAccountingContext db)
        {
            _db = db;
        }
        public async Task<bool> Create(UserRegister obj)
        {
            await _db.UserRegisters.AddAsync(obj);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        public IQueryable<UserRegister> GetAll()
        {
            return _db.UserRegisters;
        }

        public async Task<UserRegister> GetById(int id)
        {
            return await _db.UserRegisters.FirstAsync(x => x.Id == id);
        }
    }

}
