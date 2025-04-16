using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Domain.Enums;

namespace Domain.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        public ExpenseIncome Type { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserRegister> Users { get; set; }
        [JsonIgnore]

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}