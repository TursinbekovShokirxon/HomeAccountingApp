namespace Domain.DTO
{
    public class TransactionFilterDTO
    {
        public List<int> categories { get; set; } = new(); // важно!;
        public string month { get; set; } = "all";
        public string type { get; set; } = "all";
        public int limit { get; set; } = 25;



    }
}
