namespace UsageTracker.Interfaces;

public interface IUser
{
    int GetId();
    string GetName();
    string GetLastName();
    string GetEmail();
    /// <summary>Returns the stored password hash (never plain text).</summary>
    string GetPassword();
}
