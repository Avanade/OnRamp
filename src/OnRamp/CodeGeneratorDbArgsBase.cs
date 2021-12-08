// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;

namespace OnRamp
{
    /// <summary>
    /// Represents the base database-specific arguments for a <see cref="CodeGenerator"/>.
    /// </summary>
    /// <remarks>Note that the <see cref="ConnectionString"/> treatment is managed by <see cref="OverrideConnectionString(string?)"/>.</remarks>
    public abstract class CodeGeneratorDbArgsBase : ICodeGeneratorDbArgs
    {
        /// <inheritdoc/>
        public string? ConnectionString { get; set; }

        /// <inheritdoc/>
        public string? ConnectionStringEnvironmentVariableName { get; set; }

        /// <inheritdoc/>
        public Func<ICodeGeneratorDbArgs, string?>? CreateConnectionStringEnvironmentVariableName { get; set; }

        /// <inheritdoc/>
        public void OverrideConnectionString(string? overrideConnectionString = null)
        {
            if (HasOverriddenConnectionString)
                return;

            if (!string.IsNullOrEmpty(overrideConnectionString))
            {
                ConnectionString = overrideConnectionString;
                HasOverriddenConnectionString = true;
                return;
            }

            string? cs = null;
            if (!string.IsNullOrEmpty(ConnectionStringEnvironmentVariableName))
            {
                cs = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariableName);
                if (!string.IsNullOrEmpty(cs))
                {
                    ConnectionString = cs;
                    HasOverriddenConnectionString = true;
                    return;
                }
            }

            if (CreateConnectionStringEnvironmentVariableName != null)
            {
                var evn = CreateConnectionStringEnvironmentVariableName(this);
                if (!string.IsNullOrEmpty(evn))
                {
                    cs = Environment.GetEnvironmentVariable(evn);
                    if (!string.IsNullOrEmpty(cs))
                    {
                        ConnectionString = cs;
                        HasOverriddenConnectionString = true;
                        return;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public bool HasOverriddenConnectionString { get; private set; }

        /// <inheritdoc/>
        void ICodeGeneratorDbArgs.CopyFrom(ICodeGeneratorDbArgs args) => CopyFrom((CodeGeneratorDbArgsBase)args);

        /// <summary>
        /// Copy and replace from <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorDbArgsBase"/> to copy from.</param>
        public void CopyFrom(CodeGeneratorDbArgsBase args)
        {
            HasOverriddenConnectionString = args.HasOverriddenConnectionString;
            ConnectionString = args.ConnectionString;
            ConnectionStringEnvironmentVariableName = args.ConnectionStringEnvironmentVariableName;
            CreateConnectionStringEnvironmentVariableName = args.CreateConnectionStringEnvironmentVariableName;
        }
    }
}