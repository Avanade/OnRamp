﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using OnRamp.Generators;
using System;
using System.Collections.Generic;

namespace OnRamp.Config
{
    /// <summary>
    /// Provides the <b>root</b> base <see cref="ConfigBase.PrepareAsync(object, object)"/> configuration capabilities.
    /// </summary>
    /// <typeparam name="TRoot">The root <see cref="Type"/>.</typeparam>
    public abstract class ConfigRootBase<TRoot> : ConfigBase<TRoot, TRoot>, IRootConfig where TRoot : ConfigRootBase<TRoot>
    {
        /// <summary>
        /// Gets the <see cref="ICodeGeneratorArgs"/>.
        /// </summary>
        public ICodeGeneratorArgs? CodeGenArgs { get; private set; }

        /// <summary>
        /// Gets the parameter overrides.
        /// </summary>
        public Dictionary<string, object?> RuntimeParameters { get; } = [];

        /// <summary>
        /// Sets the <see cref="ICodeGeneratorArgs"/>.
        /// </summary>
        /// <param name="codeGenArgs">The <see cref="ICodeGeneratorArgs"/>.</param>
        public void SetCodeGenArgs(ICodeGeneratorArgs codeGenArgs) => CodeGenArgs = codeGenArgs ?? throw new ArgumentNullException(nameof(codeGenArgs));

        /// <summary>
        /// Merges (adds or updates) <paramref name="parameters"/> into the <see cref="RuntimeParameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters to merge.</param>
        public void MergeRuntimeParameters(IDictionary<string, object?>? parameters)
        {
            if (parameters == null)
                return;

            foreach (var p in parameters)
            {
                if (RuntimeParameters.ContainsKey(p.Key))
                    RuntimeParameters[p.Key] = p.Value;
                else
                    RuntimeParameters.Add(p.Key, p.Value);
            }
        }

        /// <summary>
        /// Resets (clears) the <see cref="RuntimeParameters"/>.
        /// </summary>
        public void ResetRuntimeParameters() => RuntimeParameters.Clear();

        /// <summary>
        /// Gets the property value from <see cref="RuntimeParameters"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value where the property is not found.</param>
        /// <returns>The value.</returns>
        public T GetRuntimeParameter<T>(string key, T defaultValue = default!)
        {
            if (RuntimeParameters != null && RuntimeParameters.TryGetValue(key, out var val))
                return (T)Convert.ChangeType(val, typeof(T))!;
            else
                return defaultValue!;
        }

        /// <summary>
        /// Trys to get the property value from <see cref="RuntimeParameters"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The corresponding value.</param>
        /// <returns><c>true</c> where the <paramref name="key"/> is found; otherwise, <c>false</c>.</returns>
        public bool TryGetRuntimeParameter<T>(string key, out T value)
        {
            if (RuntimeParameters != null && RuntimeParameters.TryGetValue(key, out var val))
            {
                value = (T)Convert.ChangeType(val, typeof(T))!;
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime.Now"/> value.
        /// </summary>
        /// <remarks>This provides a simple and consistent means to access this as a property value from a Handlebars template.</remarks>
        public DateTime DateTimeNow => DateTime.Now;

        /// <summary>
        /// Gets the <see cref="DateTime.UtcNow"/> value.
        /// </summary>
        /// <remarks>This provides a simple and consistent means to access this as a property value from a Handlebars template.</remarks>
        public DateTime DateTimeUtcNow => DateTime.UtcNow;

        /// <summary>
        /// Gets a <see cref="Guid.NewGuid"/>.
        /// </summary>
        /// <remarks>This provides a simple and consistent means to access this as a property value from a Handlebars template.</remarks>
        public Guid NewGuid => Guid.NewGuid();

        /// <summary>
        /// Gets the instance within an <see cref="IEnumerable{TRoot}"/> for a <see cref="CodeGeneratorBase{TRoot}.SelectGenConfig(TRoot)"/> result.
        /// </summary>
        public IEnumerable<TRoot> SelectGenResult => new TRoot[] { (TRoot)this };
    }
}