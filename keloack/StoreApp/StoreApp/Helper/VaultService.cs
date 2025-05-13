// using Microsoft.Extensions.Configuration;
// using System;
// using System.Text;
// using System.Threading.Tasks;
// using VaultSharp;
// using VaultSharp.V1.AuthMethods.Token;

// namespace StoreApp.Helper
// {
//     public class VaultService
//     {
//         private readonly IVaultClient _vaultClient;
//         private const string TransitKeyName = "discount-key";  // The key name you created in Vault

//         public VaultService(IConfiguration configuration)
//         {
//             // Read Vault settings from appsettings.json
//             var vaultAddress = configuration["Vault:Address"];
//             var vaultToken = configuration["Vault:Token"];

//             // Set up VaultSharp client
//             var authMethod = new TokenAuthMethodInfo(vaultToken);
//             var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod);
//             _vaultClient = new VaultClient(vaultClientSettings);
//         }

//         // Encrypt a discount string using Vault's Transit engine
//         public async Task<string> EncryptDiscountAsync(string discount)
//         {
//             var encryptResponse = await _vaultClient.V1.Secrets.Transit.EncryptAsync(TransitKeyName, discount);
//             return encryptResponse.Data.CipherText;
//         }

//         // Decrypt a discount string using Vault's Transit engine
//         public async Task<string> DecryptDiscountAsync(string cipherText)
//         {
//             var decryptResponse = await _vaultClient.V1.Secrets.Transit.DecryptAsync(TransitKeyName, cipherText);
//             var decryptedBase64 = decryptResponse.Data.PlainText;
//             var decryptedBytes = Convert.FromBase64String(decryptedBase64);
//             return Encoding.UTF8.GetString(decryptedBytes);
//         }
//     }
// }
