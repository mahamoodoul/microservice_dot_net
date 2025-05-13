using System.ComponentModel.DataAnnotations;

namespace StoreApp.Models
{
    public class DecryptRewardViewModel
    {
        [Required]
        public int RewardId { get; set; }

        // Display field after decryption
        public decimal? DecryptedDiscount { get; set; }
    }
}
