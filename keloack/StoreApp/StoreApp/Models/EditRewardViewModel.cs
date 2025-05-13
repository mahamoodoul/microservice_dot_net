namespace StoreApp.Models
{
    // View model for creating a new reward. Note that the API expects a discount (as plain text) that it will encrypt.
    public class EditRewardViewModel
    {
       public int Id { get; set; }
        public string Name { get; set; }

        public string EncryptedDiscount { get; set; }

        // The API returns an EncryptedDiscount, 
        // but you might also want to store a plain Discount
        public decimal Discount { get; set; }
    }
}
