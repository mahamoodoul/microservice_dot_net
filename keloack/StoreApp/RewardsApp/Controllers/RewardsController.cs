using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RewardsApp.Data;
using RewardsApp.Models;
using RewardsApp.Services;
using RewardsApp.Models.DTO;
using System.Threading.Tasks;

namespace RewardsApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RewardsController : ControllerBase
    {
        private readonly RewardsContext _context;
        private readonly VaultService _vaultService;

        public RewardsController(RewardsContext context, VaultService vaultService)
        {
            _context = context;
            _vaultService = vaultService;
        }

        // GET: /api/rewards
        // Returns all rewards with the encrypted discount
        [HttpGet]
        public async Task<IActionResult> GetAllRewards()
        {
            var rewards = await _context.Rewards.ToListAsync();
            return Ok(rewards);
        }

        // GET: /api/rewards/{id}
        // Returns the reward with the encrypted discount (no decryption)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReward(int id)
        {
            var reward = await _context.Rewards.FindAsync(id);
            if (reward == null) 
                return NotFound();

            return Ok(new
            {
                reward.Id,
                reward.Name,
                reward.EncryptedDiscount
            });
        }

        // GET: /api/rewards/decrypt/{id}
        // Returns the reward with a *decrypted* discount
        [HttpGet("decrypt/{id}")]
        public async Task<IActionResult> GetRewardDecrypted(int id)
        {
            var reward = await _context.Rewards.FindAsync(id);
            if (reward == null) 
                return NotFound();

            var decryptedString = await _vaultService.DecryptDiscountAsync(reward.EncryptedDiscount);

            if (!decimal.TryParse(decryptedString, out var discountValue))
                return BadRequest("Failed to parse discount from decrypted data.");

            return Ok(new
            {
                reward.Id,
                reward.Name,
                Discount = discountValue
            });
        }

        // POST: /api/rewards
        // Creates a reward and stores the encrypted discount in SQLite
        [HttpPost]
        public async Task<IActionResult> CreateReward([FromBody] CreateRewardRequest request)
        {
            var discountString = request.Discount.ToString();
            var encryptedDiscount = await _vaultService.EncryptDiscountAsync(discountString);

            var reward = new Reward
            {
                Name = request.Name,
                EncryptedDiscount = encryptedDiscount
            };
            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                reward.Id,
                reward.Name,
                reward.EncryptedDiscount
            });
        }

        // PUT: /api/rewards/{id}
        // Updates an existing reward, re-encrypting the discount if provided
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReward(int id, [FromBody] UpdateRewardRequest request)
        {
            var reward = await _context.Rewards.FindAsync(id);
            if (reward == null)
                return NotFound();

            // Update reward properties
            reward.Name = request.Name;
            var discountString = request.Discount.ToString();
            reward.EncryptedDiscount = await _vaultService.EncryptDiscountAsync(discountString);

            _context.Rewards.Update(reward);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                reward.Id,
                reward.Name,
                reward.EncryptedDiscount
            });
        }

        // DELETE: /api/rewards/{id}
        // Deletes the specified reward
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReward(int id)
        {
            var reward = await _context.Rewards.FindAsync(id);
            if (reward == null)
            {
                return NotFound(new 
                { 
                    success = false, 
                    message = "Reward not found." 
                });
            }

            _context.Rewards.Remove(reward);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                success = true, 
                message = "Reward deleted successfully." 
            });
        }

    }
}
