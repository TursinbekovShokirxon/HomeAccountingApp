using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Infrastructure.Services
{
    public static class HasherService
    {
        public static string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);
       
        public static bool Verify(string password, string passwordHash) =>
            BCrypt.Net.BCrypt.Verify(password, passwordHash); 
    }
}
