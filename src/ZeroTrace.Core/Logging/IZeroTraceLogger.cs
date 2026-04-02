// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

namespace ZeroTrace.Core.Logging;

public interface IZeroTraceLogger
{
    void Debug(string message);
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
}

public enum LogLevel
{
    Debug   = 0,
    Info    = 1,
    Warning = 2,
    Error   = 3
}
