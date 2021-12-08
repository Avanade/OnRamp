// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;

namespace OnRamp
{
    /// <summary>
    /// Defines the database-specific <see cref="CodeGenerator"/> arguments.
    /// </summary>
    /// <remarks>Note that the <see cref="ConnectionString"/> treatment is managed by <see cref="OverrideConnectionString(string?)"/>.</remarks>
    public interface ICodeGeneratorDbArgs
    {
        /// <summary>
        /// Gets or sets the database connection string.
        /// </summary>
        /// <remarks>See <see cref="OverrideConnectionString"/> for how used internally.</remarks>
        string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the environment variable name to get the connection string.
        /// </summary>
        /// <remarks>See <see cref="OverrideConnectionString"/> for how used internally.</remarks>
        string? ConnectionStringEnvironmentVariableName { get; set; }

        /// <summary>
        /// Gets or sets the function to determine the <see cref="ConnectionStringEnvironmentVariableName"/> at runtime.
        /// </summary>
        /// <remarks>See <see cref="OverrideConnectionString"/> for how used internally.</remarks>
        Func<ICodeGeneratorDbArgs, string?>? CreateConnectionStringEnvironmentVariableName { get; set; }

        /// <summary>
        /// Overrides the <see cref="ConnectionString"/> based on following order of precedence: <paramref name="overrideConnectionString"/>, from the <see cref="ConnectionStringEnvironmentVariableName"/>,
        /// from the <see cref="CreateConnectionStringEnvironmentVariableName"/>, then existing <see cref="ConnectionString"/>.
        /// </summary>
        /// <param name="overrideConnectionString">The connection string override.</param>
        /// <remarks>This will only override on first invocation; subsequent invocations will have no effect.</remarks>
        void OverrideConnectionString(string? overrideConnectionString = null);

        /// <summary>
        /// Indicates whether the <see cref="OverrideConnectionString"/> has been perfomed; can only be executed once.
        /// </summary>
        bool HasOverriddenConnectionString { get; }

        /// <summary>
        /// Copy and replace from <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorDbArgs"/> to copy from.</param>
        void CopyFrom(ICodeGeneratorDbArgs args);
    }
}