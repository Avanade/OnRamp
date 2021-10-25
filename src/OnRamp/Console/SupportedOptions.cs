// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;

namespace OnRamp.Console
{
    /// <summary>
    /// Provides the supported <see cref="CodeGenConsole"/> command-line console options.
    /// </summary>
    [Flags]
    public enum SupportedOptions
    {
        /// <summary>
        /// Supports overridding <see cref="ICodeGeneratorArgs.ScriptFileName"/>.
        /// </summary>
        ScriptFileName = 1,

        /// <summary>
        /// Supports overridding <see cref="ICodeGeneratorArgs.ConfigFileName"/>.
        /// </summary>
        ConfigFileName = 2,

        /// <summary>
        /// Supports overridding <see cref="ICodeGeneratorArgs.ExpectNoChanges"/>.
        /// </summary>
        ExpectNoChanges = 4,

        /// <summary>
        /// Supports overridding <see cref="ICodeGeneratorArgs.IsSimulation"/>.
        /// </summary>
        IsSimulation = 8,

        /// <summary>
        /// Supports overridding <see cref="ICodeGeneratorArgs.Assemblies"/>.
        /// </summary>
        Assemblies = 16,

        /// <summary>
        /// Supports overridding <see cref="ICodeGeneratorArgs.Parameters"/>.
        /// </summary>
        Parameters = 32,

        /// <summary>
        /// Supports overridding <see cref="ICodeGeneratorArgs.OutputDirectory"/>.
        /// </summary>
        OutputDirectory = 64,

        /// <summary>
        /// Supports overridding <see cref="ICodeGeneratorArgs.ConnectionString"/>.
        /// </summary>
        DatabaseConnectionString = 128,

        /// <summary>
        /// Supports overridding <see cref="ICodeGeneratorArgs.ConnectionStringEnvironmentVariableName"/>.
        /// </summary>
        DatabaseConnectionStringEnvironmentVariableName = 256,

        /// <summary>
        /// Supports all options except <see cref="DatabaseConnectionString"/> and <see cref="DatabaseConnectionStringEnvironmentVariableName"/>.
        /// </summary>
        AllExceptDatabase = ScriptFileName | ConfigFileName | ExpectNoChanges | IsSimulation | Assemblies | Parameters | OutputDirectory,

        /// <summary>
        /// Supports all options.
        /// </summary>
        All = AllExceptDatabase | DatabaseConnectionString | DatabaseConnectionStringEnvironmentVariableName
    }
}