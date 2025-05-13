namespace StoreApp.Models
{
    // Represents the reward returned by the API when the discount is decrypted
    public class RewardDecryptedDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }
    }
}
