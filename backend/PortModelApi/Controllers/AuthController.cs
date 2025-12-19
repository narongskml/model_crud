using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PortModelApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PortModelApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login failed: Username and Password are required");
            return BadRequest("Username and Password are required");
        }

        try
        {
            using var client = new HttpClient();
            var keycloakUrl = $"{_configuration["Keycloak:Authority"]}/protocol/openid-connect/token";
            
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _configuration["Keycloak:ClientId"]!),
                new KeyValuePair<string, string>("client_secret", _configuration["Keycloak:ClientSecret"]!),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", request.Username),
                new KeyValuePair<string, string>("password", request.Password)
            });

            var response = await client.PostAsync(keycloakUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Login failed for user {Username}. Details: {Details}", request.Username, responseString);
                return Unauthorized(new { message = "Invalid credentials or Keycloak error", details = responseString });
            }

            var tokenJson = System.Text.Json.JsonDocument.Parse(responseString);
            var accessToken = tokenJson.RootElement.GetProperty("access_token").GetString();

            // Parse token to return consistent format
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            
            // Extract roles from both realm and client
            var roles = new List<string>();
            
            // Extract realm roles
            var realmAccess = jwtToken.Claims.FirstOrDefault(c => c.Type == "realm_access")?.Value;
            if (!string.IsNullOrEmpty(realmAccess))
            {
                var realmJson = System.Text.Json.JsonDocument.Parse(realmAccess);
                if (realmJson.RootElement.TryGetProperty("roles", out var realmRolesElement))
                {
                    roles.AddRange(realmRolesElement.EnumerateArray().Select(r => r.GetString()!));
                }
            }
            
            // Extract client roles from resource_access
            var resourceAccess = jwtToken.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;
            if (!string.IsNullOrEmpty(resourceAccess))
            {
                var resourceJson = System.Text.Json.JsonDocument.Parse(resourceAccess);
                
                // Iterate through all clients in resource_access
                foreach (var clientEntry in resourceJson.RootElement.EnumerateObject())
                {
                    if (clientEntry.Value.TryGetProperty("roles", out var clientRolesElement))
                    {
                        roles.AddRange(clientRolesElement.EnumerateArray().Select(r => r.GetString()!));
                    }
                }
            }

            _logger.LogInformation("User {Username} logged in successfully with roles: {Roles}", request.Username, string.Join(", ", roles));
            return Ok(new { Token = accessToken, Username = request.Username, Roles = roles });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication service error for user {Username}", request.Username);
            return StatusCode(500, new { message = "Authentication service error", error = ex.Message });
        }
    }
}
