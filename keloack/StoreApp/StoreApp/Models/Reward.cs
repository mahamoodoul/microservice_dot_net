namespace StoreApp.Models
{
    public class Reward
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // The API returns an EncryptedDiscount, 
        // but you might also want to store a plain Discount
        public string EncryptedDiscount { get; set; }
        
        // For the decrypted discount or the user input
        public decimal Discount { get; set; }
    }
}
