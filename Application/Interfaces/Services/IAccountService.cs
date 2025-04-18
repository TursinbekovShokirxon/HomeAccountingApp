using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IAccountService
    {
        public  Task<UserRegister> Login(string username, string password);
        public Task<bool> Register(string username, string password);
    }
}
