namespace StoreApp.Models
{
    // View model for creating a new reward. Note that the API expects a discount (as plain text) that it will encrypt.
    public class CreateRewardViewModel
    {
        public string Name { get; set; }
        public decimal Discount { get; set; }
    }
}
