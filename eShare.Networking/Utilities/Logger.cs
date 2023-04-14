// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System.Runtime.CompilerServices;

namespace eShare.Networking.Utilities;

/// <summary>
///     Defines log message levels.
/// </summary>
public enum LogLevel
{
    /// <summary>
    ///     Logs that are used for investigation during development.
    /// </summary>
    Debug,

    /// <summary>
    ///     Logs that provide general information about application flow.
    /// </summary>
    Information,

    /// <summary>
    ///     Logs that highlight abnormal or unexpected events in the application flow.
    /// </summary>
    Warning,

    /// <summary>
    ///     Logs that highlight problematic events in the application flow.
    /// </summary>
    Error
}

/// <summary>
///     Provides functionality for logging messages.
/// </summary>
public static class Logger
{
    /// <summary>
    ///     Encapsulates a method used to log messages.
    /// </summary>
    /// <param name="message"> The message to log. </param>
    public delegate void LogMethod(string message);

    /// <summary>
    ///     Log methods accessible by their <see cref="LogLevel" />.
    /// </summary>
    private static readonly Dictionary<LogLevel, LogMethod> LogMethods = new();

    /// <summary>
    ///     Initializes the <see cref="Logger" /> using the provided logging method.
    /// </summary>
    /// <param name="logMethod"> The method used to log messages. </param>
    public static void Initialize(LogMethod logMethod)
    {
        LogMethods.Clear();

        foreach (LogLevel logLevel in Enum.GetValues(typeof(LogLevel))) LogMethods.Add(logLevel, logMethod);
    }

    /// <summary>
    ///     Logs a message using the provided <see cref="LogLevel" />.
    /// </summary>
    /// <param name="logLevel"> The message log level. </param>
    /// <param name="message"> The message to log. </param>
    /// <param name="callerMemberName"> The name of the caller. </param>
    public static void Log(LogLevel logLevel, string message, [CallerMemberName] string? callerMemberName = null)
    {
        if (LogMethods.TryGetValue(logLevel, out var logMethod))
            logMethod($"({DateTime.Now}) - ({callerMemberName}) - ({Enum.GetName(logLevel)}) - {message}");
    }
}