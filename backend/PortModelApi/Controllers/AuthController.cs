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

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and Password are required");

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
                return Unauthorized(new { message = "Invalid credentials or Keycloak error", details = responseString });
            }

            // Parse token to return consistent format
            // In a real app, define a class for Keycloak response
            var json = System.Text.Json.JsonDocument.Parse(responseString);
            var accessToken = json.RootElement.GetProperty("access_token").GetString();

            return Ok(new { Token = accessToken, Username = request.Username });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Authentication service error", error = ex.Message });
        }
    }
}
