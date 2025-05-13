using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RewardsApp.Services
{
    public class VaultService
    {
        private readonly HttpClient _httpClient;
        private readonly string _vaultAddress;
        private readonly string _vaultToken;
        private const string TransitKeyName = "discount-key"; // Must match your Vault key name

        public VaultService(IConfiguration configuration)
        {
            _vaultAddress = configuration["Vault:Address"];
            _vaultToken = configuration["Vault:Token"];

            _httpClient = new HttpClient();
            // Set required headers for Vault API
            _httpClient.DefaultRequestHeaders.Add("X-Vault-Token", _vaultToken);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // Encrypt a discount string using Vault's Transit API
        public async Task<string> EncryptDiscountAsync(string discount)
        {
            try
            {
                // Convert the discount to Base64 (Vault expects the plaintext in Base64)
                var plainTextBytes = Encoding.UTF8.GetBytes(discount);
                var plainTextBase64 = Convert.ToBase64String(plainTextBytes);

                // Prepare the JSON payload with a "plaintext" property
                var payload = new { plaintext = plainTextBase64 };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Build the Vault transit encrypt endpoint URL:
                // e.g. http://127.0.0.1:8200/v1/transit/encrypt/discount-key
                var url = $"{_vaultAddress}/v1/transit/encrypt/{TransitKeyName}";

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(responseContent);
                var cipherText = doc.RootElement.GetProperty("data").GetProperty("ciphertext").GetString();
                return cipherText;
            }
            catch (Exception ex)
            {
                // Include the raw response content if available to help with debugging
                throw new Exception("Error in EncryptDiscountAsync: " + ex.Message, ex);
            }
        }

        // Decrypt a discount string using Vault's Transit API
        public async Task<string> DecryptDiscountAsync(string cipherText)
        {
            try
            {
                // Prepare the JSON payload with a "ciphertext" property
                var payload = new { ciphertext = cipherText };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Build the Vault transit decrypt endpoint URL:
                var url = $"{_vaultAddress}/v1/transit/decrypt/{TransitKeyName}";

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(responseContent);
                var plainTextBase64 = doc.RootElement.GetProperty("data").GetProperty("plaintext").GetString();

                var decryptedBytes = Convert.FromBase64String(plainTextBase64);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in DecryptDiscountAsync: " + ex.Message, ex);
            }
        }
    }
}
