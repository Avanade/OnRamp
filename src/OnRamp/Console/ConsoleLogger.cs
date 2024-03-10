// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;

namespace OnRamp.Console
{
    /// <summary>
    /// Represents an <see cref="ILogger"/> that writes to an <see cref="IConsole"/>.
    /// </summary>
    /// <param name="console">The <see cref="IConsole"/>; where <c>null</c> will default to using <see cref="System.Console"/>.</param>
    public class ConsoleLogger(IConsole? console = null) : ILogger
    {
        private readonly IConsole? _console = console;

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Default;

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            if (exception != null)
                message += Environment.NewLine + exception;

            ConsoleColor foregroundColor;
            if (_console == null)
            {
                foregroundColor = System.Console.ForegroundColor;
                switch (logLevel)
                {
                    case LogLevel.Critical:
                    case LogLevel.Error:
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        System.Console.Error.WriteLine(message);
                        break;

                    case LogLevel.Warning:
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        System.Console.Out.WriteLine(message);
                        break;

                    default:
                        System.Console.Out.WriteLine(message);
                        break;
                }

                System.Console.ForegroundColor = foregroundColor;
            }
            else
            {
                foregroundColor = _console.ForegroundColor;
                switch (logLevel)
                {
                    case LogLevel.Critical:
                    case LogLevel.Error:
                        _console.ForegroundColor = ConsoleColor.Red;
                        _console.Error.WriteLine(message);
                        break;

                    case LogLevel.Warning:
                        _console.ForegroundColor = ConsoleColor.Yellow;
                        _console.Out.WriteLine(message);
                        break;

                    default:
                        _console.Out.WriteLine(message);
                        break;
                }

                _console.ForegroundColor = foregroundColor;
            }
        }

        /// <summary>
        /// Represents a null scope for loggers.
        /// </summary>
        private sealed class NullScope : IDisposable
        {
            /// <summary>
            /// Gets the default instance.
            /// </summary>
            public static NullScope Default { get; } = new NullScope();

            /// <summary>
            /// Initializes a new instance of the <see cref="NullScope"/> class.
            /// </summary>
            private NullScope() { }

            /// <summary>
            /// Closes and disposes the <see cref="NullScope"/>.
            /// </summary>
            public void Dispose() { }
        }
    }
}