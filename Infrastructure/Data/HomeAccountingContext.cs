using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class HomeAccountingContext : DbContext
    {
        public HomeAccountingContext(DbContextOptions<HomeAccountingContext> options) : base(options)
        {

        }
        public DbSet<UserRegister> UserRegisters { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Добавление категорий
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Заработная плата", Type = ExpenseIncome.Income },
                new Category { Id = 2, Name = "Доход с аренды недвижимости", Type = ExpenseIncome.Income },
                new Category { Id = 4, Name = "Продукты питания", Type = ExpenseIncome.Expense },
                new Category { Id = 5, Name = "Транспорт", Type = ExpenseIncome.Expense },
                new Category { Id = 6, Name = "Мобильная связь", Type = ExpenseIncome.Expense },
                new Category { Id = 7, Name = "Интернет", Type = ExpenseIncome.Expense },
                new Category { Id = 8, Name = "Развлечения", Type = ExpenseIncome.Expense },
                new Category { Id = 9, Name = "Другое", Type = ExpenseIncome.Expense },
                new Category { Id = 10, Name = "Другое", Type = ExpenseIncome.Income }
            );
            // Настройка many-to-many (если ещё не сделана)
            modelBuilder.Entity<UserRegister>()
                .HasMany(u => u.Categories)
                .WithMany(c => c.Users)
                .UsingEntity(j => j.ToTable("UserCategory"));
        }
    }
}
