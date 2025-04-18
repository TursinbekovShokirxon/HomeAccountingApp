using Application.Interfaces.Repositories.Base;
using Domain.Entities;

namespace Application.Interfaces.Repositories.Model
{
    public interface IUserRepository : ICreateRepository<UserRegister>,
        IGetRepository<UserRegister>
    {
        public Task SaveChangesAsync();
    }
}
