// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using Microsoft.Extensions.Logging;
using OnRamp.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OnRamp
{
    /// <summary>
    /// Defines the <see cref="CodeGenerator"/> arguments.
    /// </summary>
    public interface ICodeGeneratorArgs : ICodeGeneratorDbArgs
    {
        /// <summary>
        /// Gets or sets the <b>Script</b> file name to load the content from the <c>Scripts</c> folder within the file system (primary) or <see cref="Assemblies"/> (secondary, recursive until found).
        /// </summary>
        string? ScriptFileName { get; set; }

        /// <summary>
        /// Gets or sets the <b>Configuration</b> file name.
        /// </summary>
        string? ConfigFileName { get; set; }

        /// <summary>
        /// Gets or sets the output <see cref="DirectoryInfo"/> where the generated artefacts are to be written.
        /// </summary>
        DirectoryInfo? OutputDirectory { get; set; }

        /// <summary>
        /// Gets the <see cref="Assembly"/> list to use to probe for assembly resource (in defined sequence); will check this assembly also (no need to explicitly specify).
        /// </summary>
        List<Assembly> Assemblies { get; } 

        /// <summary>
        /// Gets the dictionary of <see cref="IRootConfig.RuntimeParameters"/> name/value pairs.
        /// </summary>
        Dictionary<string, object?> Parameters { get; }

        /// <summary>
        /// Gets or sets the <see cref="ILogger"/> to optionally log the underlying code-generation.
        /// </summary>
        ILogger? Logger { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="CodeGenerator.GenerateAsync(string)"/> is expecting to generate <i>no</i> changes; e.g. within in a build pipeline.
        /// </summary>
        /// <remarks>Where changes are found then a <see cref="CodeGenChangesFoundException"/> will be thrown.</remarks>
        bool ExpectNoChanges { get; set; }

        /// <summary>
        /// Indicates whether the code-generation is a simulation; i.e. does not update the artefacts.
        /// </summary>
        bool IsSimulation { get; set; }

        /// <summary>
        /// Adds (inserts) one or more <paramref name="assemblies"/> to <see cref="Assemblies"/> (before any existing values).
        /// </summary>
        /// <param name="assemblies">The assemblies to add.</param>
        /// <remarks>The order in which they are specified is the order in which they will be probed for embedded resources.</remarks>
        void AddAssembly(params Assembly[] assemblies)
        {
            foreach (var a in assemblies.Reverse())
            {
                if (!Assemblies.Contains(a))
                    Assemblies.Insert(0, a);
            }
        }

        /// <summary>
        /// Adds (updates) the parameter to the <see cref="Parameters"/>.
        /// </summary>
        /// <param name="key">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        void AddParameter(string key, object? value)
        {
            if (!Parameters.TryAdd(key, value))
                Parameters[key] = value;
        }

        /// <summary>
        /// Adds (merges) the <paramref name="parameters"/> to the <see cref="Parameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        void AddParameters(IDictionary<string, object?> parameters)
        {
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    AddParameter(p.Key, p.Value);
                }
            }
        }

        /// <summary>
        /// Gets the specified parameter from the <see cref="Parameters"/> collection.
        /// </summary>
        /// <typeparam name="T">The parameter value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="throwWhereNotFound">Indicates to throw a <see cref="CodeGenException"/> when the specified key is not found.</param>
        /// <returns>The parameter value where found; otherwise, <c>null</c>.</returns>
        /// <exception cref="CodeGenException">The <see cref="CodeGenException"/>.</exception>
        T? GetParameter<T>(string key, bool throwWhereNotFound = false);

        /// <summary>
        /// Copy and replace from <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/> to copy from.</param>
        void CopyFrom(ICodeGeneratorArgs args);
    }
}