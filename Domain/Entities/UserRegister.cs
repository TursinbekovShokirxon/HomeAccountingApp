using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class UserRegister
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}