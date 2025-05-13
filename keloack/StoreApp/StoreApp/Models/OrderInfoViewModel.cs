namespace StoreApp.Models;
public class OrderInfoViewModel
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
