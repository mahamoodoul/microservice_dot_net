using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace StoreApp.Helper
{
    public class KeycloakRoleHelper
    {
        public static async Task<bool> HasRealmAdminRole(HttpContext httpContext)
        {
            // Get the access token from the authentication session
            var accessToken = await httpContext.GetTokenAsync("access_token");

            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("[DEBUG] No access token found.");
                return false;
            }

            Console.WriteLine($"[DEBUG] Access Token Retrieved");

            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(accessToken))
            {
                Console.WriteLine("[DEBUG] Invalid JWT: Cannot read token.");
                return false;
            }

            var jwt = handler.ReadJwtToken(accessToken);

            // Extract "resource_access" claim
            var resourceAccessClaim = jwt.Claims.FirstOrDefault(c => c.Type == "resource_access");

            if (resourceAccessClaim == null)
            {
                Console.WriteLine("[DEBUG] No 'resource_access' claim found.");
                return false;
            }

            Console.WriteLine($"[DEBUG] resource_access JSON: {resourceAccessClaim.Value}");

            using var doc = JsonDocument.Parse(resourceAccessClaim.Value);

            // Check if "realm-management" exists
            if (doc.RootElement.TryGetProperty("realm-management", out var realmMgmtElement))
            {
                // Check if "roles" exist under "realm-management"
                if (realmMgmtElement.TryGetProperty("roles", out var rolesElement) &&
                    rolesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in rolesElement.EnumerateArray())
                    {
                        var roleName = role.GetString();
                        if (roleName == "realm-admin")
                        {
                            Console.WriteLine($"[DEBUG] User has realm-admin role");
                            return true; // Found realm-admin role
                        }
                    }
                }
            }

            Console.WriteLine("[DEBUG] User does not have realm-admin role");
            return false;
        }
    }
}