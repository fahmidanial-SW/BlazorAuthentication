using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly List<User> _users = new List<User>(); // Replace this with a database in a real application

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public IActionResult Register(UserDto userDto)
    {
        if (_users.Any(u => u.Username == userDto.Username))
        {
            return BadRequest("Username already exists");
        }

        var user = new User
        {
            Id = _users.Count + 1,
            Username = userDto.Username,
            Email = userDto.Email,
            PasswordHash = HashPassword(userDto.Password),
            Role = userDto.Role ?? "User"
        };

        _users.Add(user);

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public IActionResult Login(UserDto userDto)
    {
        var user = _users.FirstOrDefault(u => u.Username == userDto.Username);

        if (user == null || !VerifyPassword(userDto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid username or password");
        }

        var token = _jwtService.GenerateJwtToken(user);

        return Ok(new { token });
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        return HashPassword(password) == storedHash;
    }
}
