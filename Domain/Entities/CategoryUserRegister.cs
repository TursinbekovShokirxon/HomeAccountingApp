using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CategoryUserRegister
    {
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int UserId { get; set; }
        public UserRegister User { get; set; }
    }
}
