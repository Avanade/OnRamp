// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OnRamp
{
    /// <summary>
    /// Represents the base arguments for a <see cref="CodeGenerator"/>.
    /// </summary>
    public abstract class CodeGeneratorArgsBase : CodeGeneratorDbArgsBase, ICodeGeneratorArgs
    {
        /// <inheritdoc/>
        public string? ScriptFileName { get; set; }

        /// <inheritdoc/>
        public string? ConfigFileName { get; set; }

        /// <inheritdoc/>
        public DirectoryInfo? OutputDirectory { get; set; }

        /// <inheritdoc/>
        public List<Assembly> Assemblies { get; } = [];

        /// <inheritdoc/>
        public Dictionary<string, object?> Parameters { get; } = [];

        /// <inheritdoc/>
        public ILogger? Logger { get; set; }

        /// <inheritdoc/>
        public bool ExpectNoChanges { get; set; }

        /// <inheritdoc/>
        public bool IsSimulation { get; set; }

        /// <inheritdoc/>
        void ICodeGeneratorArgs.CopyFrom(ICodeGeneratorArgs args) => CopyFrom((CodeGeneratorArgsBase)args);

        /// <summary>
        /// Copy and replace from <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgsBase"/> to copy from.</param>
        public void CopyFrom(CodeGeneratorArgsBase args)
        {
            base.CopyFrom(args ?? throw new ArgumentNullException(nameof(args)));

            ScriptFileName = args.ScriptFileName;
            ConfigFileName = args.ConfigFileName;
            OutputDirectory = args.OutputDirectory == null ? null : new DirectoryInfo(args.OutputDirectory.FullName);
            Logger = args.Logger;
            ExpectNoChanges = args.ExpectNoChanges;
            IsSimulation = args.IsSimulation;

            Assemblies.Clear();
            Assemblies.AddRange(args.Assemblies);

            Parameters.Clear();
            if (args.Parameters != null)
            {
                foreach (var p in args.Parameters)
                {
                    Parameters.Add(p.Key, p.Value);
                }
            }
        }

        /// <inheritdoc/>
        public T GetParameter<T>(string key, bool throwWhereNotFound = false)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                if (value is IConvertible c)
                    return (T)Convert.ChangeType(c, typeof(T))!;
                else
                    return (T)value!;
            }

            if (throwWhereNotFound)
                throw new CodeGenException($"Parameter '{key}' does not exist.");

            return default!;
        }
    }
}