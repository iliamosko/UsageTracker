using UsageTracker.Interfaces;

namespace UsageTracker.Entities;

public sealed class User : IUser
{
    public int Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string PasswordHash { get; }

    public User(int id, string firstName, string lastName, string email, string passwordHash)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
    }

    public int GetId() => Id;
    public string GetName() => FirstName;
    public string GetLastName() => LastName;
    public string GetEmail() => Email;
    public string GetPassword() => PasswordHash;
}
