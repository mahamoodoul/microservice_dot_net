namespace RewardsApp.Models
{
    public class Reward
    {
        public int Id { get; set; }
        public string Name { get; set; }            // e.g. "Holiday Special"
        public string EncryptedDiscount { get; set; } // Vault-encrypted discount
    }
}
