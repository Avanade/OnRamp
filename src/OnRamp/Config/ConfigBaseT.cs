// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OnRamp.Config
{
    /// <summary>
    /// Provides the base <see cref="PrepareAsync(TRoot, TParent)"/> configuration capabilities.
    /// </summary>
    /// <typeparam name="TRoot">The root <see cref="Type"/>.</typeparam>
    /// <typeparam name="TParent">The parent <see cref="Type"/>.</typeparam>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class ConfigBase<TRoot, TParent> : ConfigBase where TRoot : ConfigBase where TParent : ConfigBase
    {
        /// <summary>
        /// Gets the <b>Root</b> (set via <see cref="PrepareAsync(TRoot, TParent)"/> execution).
        /// </summary>
        public TRoot? Root { get => (TRoot?)RootConfig; internal set => RootConfig = value; }

        /// <summary>
        /// Gets the <b>Parent</b> (set via <see cref="PrepareAsync(TRoot, TParent)"/> execution).
        /// </summary>
        public TParent? Parent { get => (TParent?)ParentConfig; internal set => ParentConfig = value; }

        /// <summary>
        /// Gets the <see cref="Root"/> <see cref="Type"/>.
        /// </summary>
        internal override Type RootType => typeof(TRoot);

        /// <summary>
        /// Overrides the <see cref="Root"/> and <see cref="Parent"/> values.
        /// </summary>
        /// <param name="root">The <see cref="Root"/>.</param>
        /// <param name="parent">The <see cref="Parent"/>.</param>
        /// <remarks>This method may result in unintended circumstances and is intended for advanced usage only.</remarks>
        public void OverrideRootAndParent(TRoot root, TParent parent)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        /// <summary>
        /// Prepares the configuration properties in advance of the code-generation execution (Internal use!).
        /// </summary>
        /// <param name="root">The root <see cref="ConfigBase"/>.</param>
        /// <param name="parent">The parent <see cref="ConfigBase"/>.</param>
        internal override Task PrepareAsync(object root, object parent) => PrepareAsync((TRoot)root, (TParent)parent);

        /// <summary>
        /// Prepares the configuration properties in advance of the code-generation execution.
        /// </summary>
        /// <param name="root">The root <see cref="ConfigBase"/>.</param>
        /// <param name="parent">The parent <see cref="ConfigBase"/>.</param>
        /// <remarks>Prior to <see cref="PrepareAsync(TRoot, TParent)"/> invocation all properties marked up with <see cref="CodeGenPropertyAttribute.IsMandatory"/> and/or <see cref="CodeGenPropertyAttribute.Options"/> will be validated as such.</remarks>
        public async Task PrepareAsync(TRoot root, TParent parent)
        {
            Root = root;
            Parent = parent;
            CheckConfiguration();
            await PrepareAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Check the configuration options.
        /// </summary>
        private void CheckConfiguration()
        {
            foreach (var pi in GetType().GetProperties())
            {
                foreach (var psa in pi.GetCustomAttributes(typeof(CodeGenPropertyAttribute), true).OfType<CodeGenPropertyAttribute>())
                {
                    if (psa.IsMandatory && pi.GetValue(this) == null)
                        throw new CodeGenException(this, pi.Name, "Value is mandatory.");

                    if (psa.Options != null && psa.Options.Length > 0 && pi.GetValue(this) is string val && !psa.Options.Contains(val))
                        throw new CodeGenException(this, pi.Name, $"Value '{val}' is invalid; valid values are: {string.Join(", ", psa.Options.Select(x => $"'{x}'"))}.");
                }

                foreach (var pcsa in pi.GetCustomAttributes(typeof(CodeGenPropertyCollectionAttribute), true).OfType<CodeGenPropertyCollectionAttribute>())
                {
                    if (pcsa.IsMandatory && pi.GetValue(this) == null)
                        throw new CodeGenException(this, pi.Name, "Collection is mandatory.");
                }
            }
        }

        /// <summary>
        /// Prepares the sub-collection.
        /// </summary>
        /// <typeparam name="T">The item <see cref="Type"/>.</typeparam>
        /// <param name="coll">The sub-collection to be <see cref="ConfigBase.PrepareAsync(object, object)"/>.</param>
        /// <returns>A new collection where <paramref name="coll"/> is <c>null</c>; otherwise, the <paramref name="coll"/> instance.</returns>
        /// <remarks>This will also perform the <see cref="CodeGenPropertyAttribute.IsUnique"/> checks.</remarks>
        protected async Task<List<T>> PrepareCollectionAsync<T>(List<T>? coll) where T : ConfigBase
        {
            if (coll == null)
                return new List<T>();

            var dict = new Dictionary<PropertyInfo, HashSet<object?>>();
            foreach (var pi in typeof(T).GetProperties())
            {
                foreach (var psa in pi.GetCustomAttributes(typeof(CodeGenPropertyAttribute), true).OfType<CodeGenPropertyAttribute>())
                {
                    if (psa.IsUnique)
                        dict.Add(pi, new HashSet<object?>());
                }
            }

            foreach (var entity in coll)
            {
                foreach (var kvp in dict)
                {
                    var val = kvp.Key.GetValue(entity);
                    if (kvp.Value.TryGetValue(val, out _))
                        throw new CodeGenException(entity, kvp.Key.Name, $"Value '{val}' is not unique.");

                    kvp.Value.Add(val);
                }

                await entity.PrepareAsync(Root!, this).ConfigureAwait(false);
            }

            return coll;
        }

        /// <summary>
        /// Performs the actual prepare logic.
        /// </summary>
        /// <remarks>This should not be invoked directly as this is invoked internally by <see cref="PrepareAsync(TRoot, TParent)"/>.</remarks>
        protected abstract Task PrepareAsync();
    }
}