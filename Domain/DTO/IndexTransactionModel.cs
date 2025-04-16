namespace Domain.DTO
{
    public class IndexTransactionModel
    {
        public bool isIncome { get; set; }  // Тип операции: доход или расход
        public int CategoryId { get; set; } // Кастомная категория, если выбрано "Другое"
        public decimal Ammount { get; set; }  // Сумма
        public string Comment { get; set; }   // Комментарий
        public DateOnly Date { get; set; }
    }
}
