﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using OnRamp.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnRamp.Scripts
{
    /// <summary>
    /// Represents the root that encapsulates the underlying <see cref="Generators"/>.
    /// </summary>
    [CodeGenClass("Script", Title = "'Script' object.", Description = "The `Script` object scripts the code-generation execution.")]
    [CodeGenCategory("Key", Title = "Provides the _Key_ configuration.")]
    [CodeGenCategory("Collections", Title = "Provides related child (hierarchical) configuration.")]
    public class CodeGenScript : ConfigRootBase<CodeGenScript>
    {
        private Type? _configType;
        private readonly List<Type> _editorTypes = [];

        /// <summary>
        /// Gets or sets the .NET <see cref="ConfigRootBase{TRoot}"/> Type for the underlying <see cref="Generators"/>.
        /// </summary>
        [JsonPropertyName("configType")]
        [CodeGenProperty("Key", Title = "The .NET ConfigRootBase Type for the underlying 'Generators'.", IsMandatory = true)]
        public string? ConfigType { get; set; }

        /// <summary>
        /// Gets or sets the list of additional script resource names to inherit.
        /// </summary>
        [JsonPropertyName("inherits")]
        [CodeGenPropertyCollection("Key", Title = "The list of additional script resource names to inherit.")]
        public List<string>? Inherits { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IConfigEditor"/> <see cref="Type"/>.
        /// </summary>
        [JsonPropertyName("editorType")]
        [CodeGenPropertyCollection("Key", Title = "The .NET IConfigEditor Type for performing extended custom configuration prior to generation.")]
        public string? EditorType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CodeGenScriptItem"/> collection.
        /// </summary>
        [JsonPropertyName("generators")]
        [CodeGenPropertyCollection("Collections", Title = "The corresponding `Generator` collection.")]
        public List<CodeGenScriptItem>? Generators { get; set; }

        /// <summary>
        /// Gets the <see cref="ConfigType"/> <see cref="Type"/>.
        /// </summary>
        /// <returns>The <see cref="ConfigType"/> <see cref="Type"/>.</returns>
        public Type GetConfigType() => _configType!;

        /// <summary>
        /// Merge zero or more <see cref="IConfigEditor"/> <see cref="Type"/>s.
        /// </summary>
        /// <param name="editors">Zero or more <see cref="IConfigEditor"/> <see cref="Type"/>s.</param>
        internal void MergeEditors(params Type[] editors) => _editorTypes.InsertRange(0, editors);

        /// <summary>
        /// Gets the <see cref="IConfigEditor"/> <see cref="Type"/>(s).
        /// </summary>
        /// <returns>Zero or more <see cref="IConfigEditor"/> <see cref="Type"/>s.</returns>
        internal Type[] GetEditors() => _editorTypes.Distinct().ToArray();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override async Task PrepareAsync()
        {
            // Make sure config type exists and is ConfigRootBase<>.
            try
            {
                _configType = Type.GetType(ConfigType ?? throw new CodeGenException(this, nameof(ConfigType), $"Type must be specified."));
            }
            catch (CodeGenException) { throw; }
            catch (Exception ex) { throw new CodeGenException(this, nameof(ConfigType), $"Type '{ConfigType}' is invalid: {ex.Message}"); }

            if (_configType == null)
                throw new CodeGenException(this, nameof(ConfigType), $"Type '{ConfigType}' does not exist.");

            if (!IsSubclassOfBaseType(typeof(ConfigRootBase<>), _configType))
                throw new CodeGenException(this, nameof(ConfigType), $"Type '{ConfigType}' must inherit from ConfigRootBase<TRoot>.");

            // Make sure the config editor exists and is IConfigEditor.
            if (EditorType != null)
            {
                Type configEditorType;
                try
                {
                    configEditorType = Type.GetType(EditorType) ?? throw new CodeGenException(this, nameof(EditorType), $"Type '{EditorType}' does not exist.");
                }
                catch (CodeGenException) { throw; }
                catch (Exception ex) { throw new CodeGenException(this, nameof(EditorType), $"Type '{EditorType}' is invalid: {ex.Message}"); }

                if (!typeof(IConfigEditor).IsAssignableFrom(configEditorType) || configEditorType.GetConstructor([]) == null)
                    throw new CodeGenException(this, nameof(EditorType), $"Type '{EditorType}' does not implement IConfigEditor and/or have a default parameterless constructor.");

                MergeEditors(configEditorType);
            }

            // Prepare the Generators collection.
            Generators = await PrepareCollectionAsync(Generators).ConfigureAwait(false);
        }
    }
}