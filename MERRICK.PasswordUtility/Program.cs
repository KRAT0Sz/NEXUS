namespace MERRICK.PasswordUtility;

using Microsoft.AspNetCore.Identity;
using MERRICK.DatabaseContext.Constants;
using MERRICK.DatabaseContext.Entities.Core;
using MERRICK.DatabaseContext.Entities.Utility;
using MERRICK.DatabaseContext.Handlers;

public class Program
{
    public static void Main(string[] arguments)
    {
        if (arguments.Length is 0)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  PBKDF2: dotnet run -- pbkdf2 <password>");
            Console.WriteLine("  SRP:    dotnet run -- srp <password> <salt>");
            return;
        }

        string command = arguments[0];

        if (command.Equals("pbkdf2", StringComparison.OrdinalIgnoreCase))
        {
            if (arguments.Length < 2)
            {
                Console.WriteLine("Usage: dotnet run -- pbkdf2 <password>");
                return;
            }

            string password = arguments[1];

            Role role = new Role { Name = UserRoles.User };

            User temporaryUser = new User
            {
                EmailAddress = "temp@example.com",
                Role = role,
                SRPPasswordSalt = string.Empty,
                SRPPasswordHash = string.Empty
            };

            string pbkdf2Hash = new PasswordHasher<User>().HashPassword(temporaryUser, password);

            Console.WriteLine($@"PBKDF2 Hash For ""{password}"":");
            Console.WriteLine(pbkdf2Hash);
        }

        else if (command.Equals("srp", StringComparison.OrdinalIgnoreCase))
        {
            if (arguments.Length < 3)
            {
                Console.WriteLine("Usage: dotnet run -- srp <password> <salt>");
                return;
            }

            string password = arguments[1];
            string salt = arguments[2];

            string srpHash = ZORGATH.WebPortal.API.Handlers.SRPRegistrationHandlers.ComputeSRPPasswordHash(password, salt, passwordIsHashed: false);

            Console.WriteLine($@"SRP Hash For ""{password}"" With Salt ""{salt}"".");
            Console.WriteLine(srpHash);
        }

        else
        {
            Console.WriteLine($@"Unknown Command ""{command}""");
        }
    }
}
