namespace CasualDate.Services;

public class PlayerDataUnavailableException : Exception
{
    public PlayerDataUnavailableException(string message)
        : base(message)
    {
    }
}
