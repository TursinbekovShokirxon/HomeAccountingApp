using Application.Interfaces.Repositories.Model;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        public AccountService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<UserRegister> Login(string username, string password)
        {
            var users = _userRepository.GetAll();
            var user = users.FirstOrDefault(x => x.Username == username);
            
            if (user != null&&HasherService.Verify(password, user.Password))               
                return user;
            return null;
        }
        public async Task<bool> Register(string username, string password)
        {
            var result = await _userRepository.Create(new UserRegister
            {
                Username = username,
                Password = HasherService.HashPassword(password)
            });
            return result;
        }

    }
}
