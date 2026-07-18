namespace IndustrialApp.Application.Ai;

public sealed class AiServiceUnavailableException : Exception
{
    public AiServiceUnavailableException()
    {
    }

    public AiServiceUnavailableException(string message)
        : base(message)
    {
    }

    public AiServiceUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
