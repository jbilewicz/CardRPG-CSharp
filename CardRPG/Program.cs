using CardRPG.Services;
using CardRPG.Models;

Console.Title = "CardRPG - Login";
AuthService auth = new AuthService();
User loggedUser = null;

while (loggedUser == null)
{
    Console.Clear();
    Console.WriteLine("=== 🔐 CARDRPG LOGIN SYSTEM ===");
    Console.WriteLine("1. Login");
    Console.WriteLine("2. Register new account");
    Console.WriteLine("3. Exit");
    Console.Write("> ");
    
    string choice = Console.ReadLine();

    if (choice == "1")
    {
        Console.Write("Username: ");
        string user = Console.ReadLine();
        Console.Write("Password: ");
        string pass = Console.ReadLine();

        loggedUser = auth.Login(user, pass);
        if (loggedUser == null)
        {
            Console.WriteLine("❌ Invalid username or password!");
            Console.ReadKey();
        }
    }
    else if (choice == "2")
    {
        Console.Write("New Username: ");
        string user = Console.ReadLine();
        Console.Write("New Password: ");
        string pass = Console.ReadLine();

        var newUser = auth.Register(user, pass);
        if (newUser != null)
        {
            Console.WriteLine("✅ Account created! You can now login.");
        }
        else
        {
            Console.WriteLine("❌ Username already taken!");
        }
        Console.ReadKey();
    }
    else if (choice == "3")
    {
        return; 
    }
}

Console.WriteLine($"\n👋 Welcome back, {loggedUser.Username}!");
Thread.Sleep(1000);

GameManager game = new GameManager(loggedUser); 
game.Run();