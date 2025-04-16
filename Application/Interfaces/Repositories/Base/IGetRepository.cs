namespace Application.Interfaces.Repositories.Base
{
    public interface IGetRepository<T>
    {
        public IQueryable<T> GetAll();
        public Task<T> GetById(int id);
    }
}
