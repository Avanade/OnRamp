// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;
using System.Collections.Generic;

namespace OnRamp.Config
{
    /// <summary>
    /// Enables the additional root configuration capabilities.
    /// </summary>
    public interface IRootConfig
    {
        /// <summary>
        /// Gets the <see cref="ICodeGeneratorArgs"/>.
        /// </summary>
        ICodeGeneratorArgs? CodeGenArgs { get; }

        /// <summary>
        /// Gets the parameter overrides.
        /// </summary>
        Dictionary<string, object?> RuntimeParameters { get; }

        /// <summary>
        /// Sets the <see cref="CodeGeneratorArgs"/>.
        /// </summary>
        /// <param name="codeGenArgs">The <see cref="ICodeGeneratorArgs"/>.</param>
        void SetCodeGenArgs(ICodeGeneratorArgs codeGenArgs);

        /// <summary>
        /// Merges (adds or updates) <paramref name="parameters"/> into the <see cref="RuntimeParameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters to merge.</param>
        void MergeRuntimeParameters(IDictionary<string, object?>? parameters);

        /// <summary>
        /// Resets (clears) the <see cref="RuntimeParameters"/>.
        /// </summary>
        void ResetRuntimeParameters();

        /// <summary>
        /// Gets the <see cref="DateTime.Now"/> value.
        /// </summary>
        /// <remarks>This provides a simple and consistent means to access this as a property value from a Handlebars template.</remarks>
        DateTime DateTimeNow { get; }

        /// <summary>
        /// Gets the <see cref="DateTime.UtcNow"/> value.
        /// </summary>
        /// <remarks>This provides a simple and consistent means to access this as a property value from a Handlebars template.</remarks>
        DateTime DateTimeUtcNow { get; }

        /// <summary>
        /// Gets a <see cref="Guid.NewGuid"/>.
        /// </summary>
        /// <remarks>This provides a simple and consistent means to access this as a property value from a Handlebars template.</remarks>
        Guid NewGuid { get; }
    }
}