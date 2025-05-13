namespace StoreApp.Models
{
    // View model to display reward details in the view, including both the encrypted and decrypted discount
    public class RewardDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EncryptedDiscount { get; set; }
        public decimal? Discount { get; set; }
    }
}
