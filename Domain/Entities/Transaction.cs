using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Domain.Enums;

namespace Domain.Entities
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        [JsonIgnore]
        public virtual Category Category { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [StringLength(200)]
        public string Comment { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        [JsonIgnore]
        public virtual UserRegister User { get; set; }
    }
}