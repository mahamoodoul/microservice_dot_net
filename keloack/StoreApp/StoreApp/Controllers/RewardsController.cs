using Microsoft.AspNetCore.Mvc;
using StoreApp.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StoreApp.Controllers
{
    public class RewardsController : Controller
    {
        private readonly HttpClient _httpClient;

        public RewardsController(IHttpClientFactory httpClientFactory)
        {
            // Create an HttpClient with a base address
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new System.Uri("http://localhost:5268"); 
            // Adjust port/base URL if different
        }

        // GET: /Rewards
        // Displays all rewards
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("/api/rewards");
            if (!response.IsSuccessStatusCode)
            {
                // Handle error, log it, etc.
                return View(new List<Reward>());
            }

            var rewards = await response.Content.ReadFromJsonAsync<List<Reward>>();
            return View(rewards);
        }

        // GET: /Rewards/Details/5
        // Displays the details of a single reward (including decrypted discount if desired)
        public async Task<IActionResult> Details(int id)
        {
            // If you want the decrypted discount, call /api/rewards/decrypt/{id}
            var response = await _httpClient.GetAsync($"/api/rewards/decrypt/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var reward = await response.Content.ReadFromJsonAsync<Reward>();

            // The above call returns "Discount" but not "EncryptedDiscount", so 
            // you might also want to fetch the encrypted value if you need it
            // This is optional. We'll just proceed with the decrypted discount.
            return View(reward);
        }

        // GET: /Rewards/Create
        // Renders the creation form
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Rewards/Create
        // Creates a new reward via the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRewardViewModel reward)
        {
            if (!ModelState.IsValid)
            {
                return View(reward);
            }

            // The API expects { name, discount } in JSON
            var payload = new 
            {
                name = reward.Name,
                discount = reward.Discount
            };

            var response = await _httpClient.PostAsJsonAsync("/api/rewards", payload);
            if (!response.IsSuccessStatusCode)
            {
                // Handle the error or show a message to the user
                return View(reward);
            }

            // Successfully created, redirect to index
            return RedirectToAction(nameof(Index));
        }

        // GET: /Rewards/Edit/5
        // Fetches existing data and displays it for editing
        public async Task<IActionResult> Edit(int id)
        {
            // We can fetch the encrypted version or the decrypted version
            // For editing, you might want the decrypted discount
            var response = await _httpClient.GetAsync($"/api/rewards/decrypt/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var reward = await response.Content.ReadFromJsonAsync<EditRewardViewModel>();
            return View(reward);
        }

        // POST: /Rewards/Edit/5
        // Updates an existing reward via the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditRewardViewModel reward)
        {
            if (!ModelState.IsValid)
            {
                return View(reward);
            }

            // The API expects a PUT to /api/rewards/{id} with { name, discount }
            var payload = new 
            {
                name = reward.Name,
                discount = reward.Discount
            };

            var response = await _httpClient.PutAsJsonAsync($"/api/rewards/{id}", payload);
            if (!response.IsSuccessStatusCode)
            {
                // handle error
                return View(reward);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Rewards/Delete/5
        // Show a confirmation page before deletion
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"/api/rewards/decrypt/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var reward = await response.Content.ReadFromJsonAsync<Reward>();
            return View(reward);
        }

        // POST: /Rewards/Delete/5
        // Actually deletes the reward
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/rewards/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                // Attempt to read error response from API
                var errorResult = await response.Content.ReadFromJsonAsync<DeleteResponse>();
                TempData["Error"] = errorResult?.Message ?? "Failed to delete reward.";
            }
            else
            {
                var successResult = await response.Content.ReadFromJsonAsync<DeleteResponse>();
                TempData["Success"] = successResult?.Message ?? "Reward deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        
    }
}
