using Application.Interfaces.Repositories.Base;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.Model
{
    public interface ICategoryRepository:ICreateRepository<Category>,
        IGetRepository<Category>, 
        IDeleteRepository<Category>
    {
    }
}
