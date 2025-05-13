namespace OrderApi.Models
{
// Models/OrderInfo.cs
    public class OrderInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}

