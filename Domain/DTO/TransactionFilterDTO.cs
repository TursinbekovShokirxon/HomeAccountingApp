using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class TransactionFilterDTO
    {
        public List<int> categories { get; set; } = null;
        public string month { get; set; } = "all";
        public string type { get; set; } = "all";
        public int limit { get; set; } = 25;

         
      
    }
}
