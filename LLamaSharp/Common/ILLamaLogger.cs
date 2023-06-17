namespace LLama.Common;

public interface ILLamaLogger
{
    public enum LogLevel
    {
        Info,
        Debug,
        Warning,
        Error
    }
    /// <summary>
    /// Write the log in cosutomized way
    /// </summary>
    /// <param name="source">The source of the log. It may be a method name or class name.</param>
    /// <param name="message">The message.</param>
    /// <param name="level">The log level.</param>
    void Log(string source, string message, LogLevel level);
}