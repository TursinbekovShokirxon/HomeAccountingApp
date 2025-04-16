namespace HomeAccountingApp.Models
{
    public class TransactionViewModel
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }
        public string CategoryName { get; set; }
        public bool IsIncome { get; set; }

        public string FormattedDate => Date.ToString("dd.MM.yyyy");
        public string FormattedAmount => Amount.ToString("N0") + " сум";
        public string Type => IsIncome ? "Доход" : "Расход";
        public string RowClass => IsIncome ? "table-success" : "table-danger";
    }
}
