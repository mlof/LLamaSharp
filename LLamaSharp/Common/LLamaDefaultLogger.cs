﻿using System;
using System.Diagnostics;
using System.IO;
using static LLama.Common.ILLamaLogger;

namespace LLama.Common;

/// <summary>
/// The default logger of LLamaSharp. On default it write to console. User methods of `LLamaLogger.Default` to change the behavior.
/// It's more recommended to inherit `ILLamaLogger` to cosutomize the behavior.
/// </summary>
public sealed class LLamaDefaultLogger : ILLamaLogger
{
    private static readonly Lazy<LLamaDefaultLogger> _instance = new Lazy<LLamaDefaultLogger>(() => new LLamaDefaultLogger());

    private bool _toConsole = true;
    private bool _toFile = false;

    private FileStream? _fileStream = null;
    private StreamWriter _fileWriter = null;

    public static LLamaDefaultLogger Default => _instance.Value;

    private LLamaDefaultLogger()
    {

    }

    public LLamaDefaultLogger EnableConsole()
    {
        _toConsole = true;
        return this;
    }

    public LLamaDefaultLogger DisableConsole()
    {
        _toConsole = false;
        return this;
    }

    public LLamaDefaultLogger EnableFile(string filename, FileMode mode = FileMode.Append)
    {
        _fileStream = new FileStream(filename, mode, FileAccess.Write);
        _fileWriter = new StreamWriter(_fileStream);
        _toFile = true;
        return this;
    }

    public LLamaDefaultLogger DisableFile(string filename)
    {
        if (_fileWriter is not null)
        {
            _fileWriter.Close();
            _fileWriter = null;
        }
        if (_fileStream is not null)
        {
            _fileStream.Close();
            _fileStream = null;
        }
        _toFile = false;
        return this;
    }

    public void Log(string source, string message, LogLevel level)
    {
        if (level == LogLevel.Info)
        {
            Info(message);
        }
        else if (level == LogLevel.Debug)
        {

        }
        else if (level == LogLevel.Warning)
        {
            Warn(message);
        }
        else if (level == LogLevel.Error)
        {
            Error(message);
        }
    }

    public void Info(string message)
    {
        message = MessageFormat("info", message);
        if (_toConsole)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        if (_toFile)
        {
            Debug.Assert(_fileStream is not null);
            Debug.Assert(_fileWriter is not null);
            _fileWriter.WriteLine(message);
        }
    }

    public void Warn(string message)
    {
        message = MessageFormat("warn", message);
        if (_toConsole)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        if (_toFile)
        {
            Debug.Assert(_fileStream is not null);
            Debug.Assert(_fileWriter is not null);
            _fileWriter.WriteLine(message);
        }
    }

    public void Error(string message)
    {
        message = MessageFormat("error", message);
        if (_toConsole)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        if (_toFile)
        {
            Debug.Assert(_fileStream is not null);
            Debug.Assert(_fileWriter is not null);
            _fileWriter.WriteLine(message);
        }
    }

    private string MessageFormat(string level, string message)
    {
        DateTime now = DateTime.Now;
        string formattedDate = now.ToString("yyyy.MM.dd HH:mm:ss");
        return $"[{formattedDate}][{level}]: {message}";
    }
}