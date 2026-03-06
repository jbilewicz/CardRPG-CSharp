using System.Security.Cryptography;
using System.Text;
using CardRPG.Core.Data;
using CardRPG.Core.Models;

namespace CardRPG.Core.Services;

public class AuthService
{
    private GameDBContext _context;

    public AuthService()
    {
        _context = new GameDBContext();
        _context.Database.EnsureCreated();
    }

    public User Register(string username, string password)
    {
        if (_context.Users.Any(u => u.Username == username))
            return null;

        var newUser = new User
        {
            Username = username,
            PasswordHash = HashPassword(password)
        };

        _context.Users.Add(newUser);
        _context.SaveChanges();
        return newUser;
    }

    public User Login(string username, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null) return null;

        string inputHash = HashPassword(password);
        return user.PasswordHash == inputHash ? user : null;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}