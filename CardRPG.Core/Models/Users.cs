using System.ComponentModel.DataAnnotations;

namespace CardRPG.Core.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? SavedGameData { get; set; }
}