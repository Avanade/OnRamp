// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using OnRamp.Config;
using OnRamp.Generators;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnRamp.Scripts
{
    /// <summary>
    /// Represents the <see cref="HandlebarsCodeGenerator"/> script arguments used to define a <see cref="CodeGeneratorBase"/> (as specified by the <see cref="Type"/>) and other associated code-generation arguments.
    /// </summary>
    [CodeGenClass("Generate", Title = "'Generate' command.", Description = "The `Generate` command defines the execution parameters for a code-generation execution.")]
    [CodeGenCategory("Key", Title = "Provides the _Key_ configuration.")]
    public class CodeGenScriptItem : ConfigBase<CodeGenScript, CodeGenScript>
    {
        private CodeGeneratorBase? _generator;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Generate");

        /// <summary>
        /// Gets or sets the <see cref="CodeGeneratorBase"/> <see cref="System.Type"/>.
        /// </summary>
        [JsonPropertyName("type")]
        [CodeGenProperty("Key", Title = "The .NET Generator (CodeGeneratorBase) Type.", IsMandatory = true)]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the template resource name.
        /// </summary>
        [JsonPropertyName("template")]
        [CodeGenProperty("Key", Title = "The template resource name.", IsMandatory = true)]
        public string? Template { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        /// <remarks>Supports <b>Handlebars</b> syntax.</remarks>
        [JsonPropertyName("file")]
        [CodeGenProperty("Key", Title = "The file name.", IsMandatory = true, Description = "Supports _Handlebars_ syntax.")]
        public string? File { get; set; }

        /// <summary>
        /// Gets or sets the directory name.
        /// </summary>
        /// <remarks>Supports <b>Handlebars</b> syntax.</remarks>
        [JsonPropertyName("directory")]
        [CodeGenProperty("Key", Title = "The directory name.", Description = "Supports _Handlebars_ syntax.")]
        public string? Directory { get; set; }

        /// <summary>
        /// Indicates whether the file is only generated once; i.e. only created where it does not already exist.
        /// </summary>
        [JsonPropertyName("genOnce")]
        [CodeGenProperty("Key", Title = "Indicates whether the file is only generated once; i.e. only created where it does not already exist.")]
        public bool IsGenOnce { get; set; }

        /// <summary>
        /// Gets or sets the gen-once file name pattern to check (can include wildcard '<c>*</c>' characters).
        /// </summary>
        /// <remarks>Supports <b>Handlebars</b> syntax.</remarks>
        [JsonPropertyName("genOncePattern")]
        [CodeGenProperty("Key", Title = "The gen-once file name pattern to check (can include wildcard `*` characters).", Description = "Supports _Handlebars_ syntax. Defaults to `File` where not specified.")]
        public string? GenOncePattern { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        [JsonPropertyName("text")]
        [CodeGenProperty("Key", Title = "The additional text written to the log to enable additional context.")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets the runtime parameters (as specified via <see cref="ConfigBase.ExtraProperties"/>).
        /// </summary>
        public Dictionary<string, object?> RuntimeParameters { get; } = [];

        /// <summary>
        /// Gets the <see cref="CodeGeneratorBase"/> as specified by <see cref="Type"/>.
        /// </summary>
        /// <returns>The <see cref="CodeGeneratorBase"/>.</returns>
        public CodeGeneratorBase GetGenerator() => _generator ?? throw new InvalidOperationException("Prepare operation must be performed before this property can be accessed.");

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override Task PrepareAsync()
        {
            // Make sure generator type exists and is CodeGeneratorBase.
            Type type;
            try
            {
                type = System.Type.GetType(Type ?? throw new CodeGenException(this, nameof(Type), $"Type must be specified.")) ?? throw new CodeGenException(this, nameof(Type), $"Type '{Type}' does not exist.");

                if (!IsSubclassOfBaseType(typeof(CodeGeneratorBase), type) || type.GetConstructor([]) == null)
                    throw new CodeGenException(this, nameof(Type), $"Type '{Type}' does not implement CodeGeneratorBase and/or have a default parameterless constructor.");

                _generator = (CodeGeneratorBase)(Activator.CreateInstance(type) ?? throw new CodeGenException(this, nameof(Type), $"Type '{Type}' was unable to be instantiated."));
                if (_generator.RootType != Root!.GetConfigType())
                    throw new CodeGenException(this, nameof(Type), $"Type '{Type}' RootType '{_generator.RootType.Name}' must be the same as the ConfigType '{Root!.GetConfigType().Name}'.");
            }
            catch (CodeGenException) { throw; }
            catch (Exception ex) { throw new CodeGenException(this, nameof(Type), $"Type '{Type}' is invalid: {ex.Message}"); }

            // Make sure the template exists.
            var (Exists, FileName) = StreamLocator.HasTemplateStream(Template!, [.. Root!.CodeGenArgs!.Assemblies], StreamLocator.HandlebarsExtensions);
            if (!Exists)
                throw new CodeGenException(this, nameof(Template), $"Template '{Template}' does not exist.");

            Template = FileName;

            // Add special runtime parameters.
            RuntimeParameters.Add(nameof(IsGenOnce), IsGenOnce);

            // Convert any extra properties as runtime parameters.
            try
            {
                if (ExtraProperties != null)
                {
                    foreach (var json in ExtraProperties)
                    {
                        RuntimeParameters.Add(json.Key, json.Value.ToString());
                    }
                }
            }
            catch (Exception ex) { throw new CodeGenException(this, nameof(ExtraProperties), $"Error converting into RuntimeParameters: {ex.Message}"); }

            return Task.CompletedTask;
        }
    }
}