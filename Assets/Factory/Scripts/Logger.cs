using System.Diagnostics;
using Debug = UnityEngine.Debug;

public sealed class Logger
{
    public const string ENABLE_LOGGIN_DEF = "ENABLE_LOGGING";

    [Conditional(ENABLE_LOGGIN_DEF), Conditional("DEBUG")]
    public static void Log(string message, params object[] args)
    {
        if (args.Length > 0)
        {
            Debug.LogFormat(message, args);
        }
        else
        {
            Debug.Log(message);
        }
    }

    [Conditional(ENABLE_LOGGIN_DEF), Conditional("DEBUG")]
    public static void LogError(string message, params object[] args)
    {
        if (args.Length > 0)
        {
            Debug.LogErrorFormat(message, args);
        }
        else
        {
            Debug.LogError(message);
        }
    }

    [Conditional(ENABLE_LOGGIN_DEF), Conditional("DEBUG")]
    public static void LogWarn(string message, params object[] args)
    {
        if (args.Length > 0)
        {
            Debug.LogWarningFormat(message, args);
        }
        else
        {
            Debug.LogWarning(message);
        }
    }

    public static LoggerInstance New(string name)
    {
        return new LoggerInstance(name);
    }
}

public class LoggerInstance
{
    public string Name { get; private set; }

    public LoggerInstance(string name)
    {
        Name = name;
    }

    private string Tag(string message)
    {
        return "[" + Name + "] " + message;
    }

    [Conditional(Logger.ENABLE_LOGGIN_DEF), Conditional("DEBUG")]
    public void Log(string message, params object[] args)
    {
        Logger.Log(Tag(message), args);
    }

    [Conditional(Logger.ENABLE_LOGGIN_DEF), Conditional("DEBUG")]
    public void LogError(string message, params object[] args)
    {
        Logger.LogError(Tag(message), args);
    }

    [Conditional(Logger.ENABLE_LOGGIN_DEF), Conditional("DEBUG")]
    public void LogWarn(string message, params object[] args)
    {
        Logger.LogWarn(Tag(message), args);
    }
}