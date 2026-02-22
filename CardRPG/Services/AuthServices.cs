using System.Security.Cryptography;
using System.Text;
using CardRPG.Data;
using CardRPG.Models;

namespace CardRPG.Services;

public class AuthService
{
    private GameDBContext _context;

    public AuthService()
    {
        _context = new GameDBContext();
        _context.Database.EnsureCreated(); //this commend create db file, if not exists
    }

    public User Register(string username, string password)
    {
        if(_context.Users.Any(u=>u.Username == username))
        {
            return null; //error: user exists
        }

        //Create new user
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
        //search user by name
        var user = _context.Users.FirstOrDefault(u=>u.Username==username);

        if(user == null) return null; //no user with such a name exists

        //checking the password
        string inputHash = HashPassword(password);

        if (user.PasswordHash == inputHash)
        {
            return user;
        }

        return null; //wrong password
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}