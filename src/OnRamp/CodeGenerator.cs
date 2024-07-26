// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using Microsoft.Extensions.Logging;
using OnRamp.Config;
using OnRamp.Scripts;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OnRamp
{
    /// <summary>
    /// Primary code-generation orchestrator.
    /// </summary>
    public class CodeGenerator
    {
        /// <summary>
        /// Represents the file name when loaded from a stream; i.e. actual file name is unknown.
        /// </summary>
        public const string StreamFileName = "<stream>";

        /// <summary>
        /// Create a new instance of the <see cref="CodeGenerator"/> class.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
        /// <returns>The <see cref="CodeGenerator"/>.</returns>
        public static async Task<CodeGenerator> CreateAsync(ICodeGeneratorArgs args)
            => new CodeGenerator(args, await LoadScriptsAsync(args).ConfigureAwait(false));

        /// <summary>
        /// Create a new instance of the <typeparamref name="TCodeGenerator"/> class.
        /// </summary>
        /// <typeparam name="TCodeGenerator">The <see cref="CodeGenerator"/> <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
        /// <returns>The <typeparamref name="TCodeGenerator"/> instance.</returns>
        /// <remarks>The constructor must be the same as the <see cref="CodeGenerator"/>.</remarks>
        public static async Task<TCodeGenerator> CreateAsync<TCodeGenerator>(ICodeGeneratorArgs args) where TCodeGenerator : CodeGenerator
            => (TCodeGenerator)Activator.CreateInstance(typeof(TCodeGenerator), [args, await LoadScriptsAsync(args).ConfigureAwait(false)]);

        /// <summary>
        /// Load the Scripts.
        /// </summary>
        private static async Task<CodeGenScript> LoadScriptsAsync(ICodeGeneratorArgs args)
        {
            var r = StreamLocator.GetScriptStreamReader(args.ScriptFileName ?? throw new CodeGenException("Script file name must be specified."), [.. args.Assemblies], StreamLocator.YamlJsonExtensions);
            using var s = r.StreamReader ?? throw new CodeGenException($"Script '{args.ScriptFileName}' does not exist.");
            args.ScriptFileName = r.FileName;
            return await LoadScriptStreamAsync(args, null, args.ScriptFileName, s).ConfigureAwait(false);
        }

        /// <summary>
        /// Load/parse the script configuration from the stream.
        /// </summary>
        private static async Task<CodeGenScript> LoadScriptStreamAsync(ICodeGeneratorArgs args, CodeGenScript? rootScript, string scriptFileName, TextReader scriptReader)
        {
            try
            {
                // Load file and deserialize.
                CodeGenScript scripts;
                try
                {
                    scripts = StreamLocator.GetContentType(scriptFileName) switch
                    {
                        StreamContentType.Yaml => scriptReader.DeserializeYaml<CodeGenScript>(),
                        StreamContentType.Json => scriptReader.DeserializeJson<CodeGenScript>(),
                        _ => throw new CodeGenException($"Stream content type is not supported.")
                    } ?? throw new CodeGenException($"Stream is empty.");
                }
                catch (CodeGenException) { throw; }
                catch (Exception ex) { throw new CodeGenException(ex.Message); }

                // Merge in the parameters and prepare/validate.
                scripts.SetCodeGenArgs(args);
                scripts.MergeRuntimeParameters(args.Parameters);
                await scripts.PrepareAsync(scripts, scripts).ConfigureAwait(false);
                rootScript ??= scripts;

                // Recursively inherit (include/merge) additional scripts files.
                var inherited = new List<CodeGenScriptItem>();
                if (rootScript.GetConfigType() != scripts.GetConfigType())
                    throw new CodeGenException(scripts, nameof(CodeGenScript.ConfigType), $"Inherited ConfigType '{scripts.ConfigType}' must be the same as root ConfigType '{rootScript.ConfigType}'.");

                if (scripts.Inherits != null)
                {
                    foreach (var ifn in scripts.Inherits)
                    {
                        using var s = StreamLocator.GetScriptStreamReader(ifn, [.. args.Assemblies], StreamLocator.YamlJsonExtensions).StreamReader ?? throw new CodeGenException($"Script '{ifn}' does not exist.");
                        var inherit = await LoadScriptStreamAsync(args, rootScript, ifn, s).ConfigureAwait(false);
                        foreach (var iscript in inherit.Generators!)
                        {
                            iscript.Root = rootScript.Root;
                            iscript.Parent = rootScript.Parent;
                            inherited.Add(iscript);
                        }
                    }
                }

                // Merge in the generators and editors.
                scripts.Generators!.InsertRange(0, inherited);
                rootScript.MergeEditors(scripts.GetEditors());

                return scripts;
            }
            catch (CodeGenException cgex)
            {
                throw new CodeGenException($"Script '{scriptFileName}' is invalid: {cgex.Message}");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenerator"/> class.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
        /// <param name="scripts">The <see cref="CodeGenScript"/>.</param>
        /// <remarks>The class should only be instantiated by <see cref="CreateAsync(ICodeGeneratorArgs)"/> or <see cref="CreateAsync{TCodeGenerator}(ICodeGeneratorArgs)"/> to ensure conrrectly set up for execution.</remarks>
        protected CodeGenerator(ICodeGeneratorArgs args, CodeGenScript scripts)
        {
            CodeGenArgs = args ?? throw new ArgumentNullException(nameof(args));
            CodeGenArgs.OutputDirectory ??= new DirectoryInfo(Environment.CurrentDirectory);
            Scripts = scripts;
        }

        /// <summary>
        /// Gets the <see cref="CodeGenScript"/>.
        /// </summary>
        public CodeGenScript Scripts { get; }

        /// <summary>
        /// Gets the <see cref="ICodeGeneratorArgs"/>.
        /// </summary>
        public ICodeGeneratorArgs CodeGenArgs { get; }

        /// <summary>
        /// Loads the <see cref="IRootConfig"/> <see cref="ConfigBase"/> from the specified <paramref name="configFileName"/>.
        /// </summary>
        /// <param name="configFileName">The configuration file name.</param>
        /// <returns>The <see cref="IRootConfig"/> <see cref="ConfigBase"/>.</returns>
        public async Task<ConfigBase> LoadConfigAsync(string? configFileName = null)
        {
            var fn = configFileName ?? CodeGenArgs.ConfigFileName ?? throw new CodeGenException("Config file must be specified.");
            var r = StreamLocator.GetStreamReader(fn, null, [.. CodeGenArgs.Assemblies]);
            using var sr = r.StreamReader ?? throw new CodeGenException($"Config '{fn}' does not exist.");
            return await LoadConfigAsync(sr, StreamLocator.GetContentType(r.FileName), r.FileName).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the <see cref="IRootConfig"/> <see cref="ConfigBase"/> from the specified <paramref name="configReader"/>.
        /// </summary>
        /// <param name="configReader">The <see cref="TextReader"/> containing the configuration.</param>
        /// <param name="contentType">The corresponding <see cref="StreamContentType"/>.</param>
        /// <param name="configFileName">The optional configuration file name used specifically in error messages.</param>
        /// <returns>The resultant <see cref="CodeGenStatistics"/>.</returns>
        /// <exception cref="CodeGenException">Thrown when an error is encountered during the code-generation.</exception>
        /// <exception cref="CodeGenChangesFoundException">Thrown where the code-generation would result in changes to an underlying artefact. This is managed by setting <see cref="ICodeGeneratorArgs.ExpectNoChanges"/> to <c>true</c>.</exception>
        public async Task<ConfigBase> LoadConfigAsync(TextReader configReader, StreamContentType contentType, string configFileName = StreamFileName)
        {
            ConfigBase? config;
            IRootConfig rootConfig;

            // Load, validate and prepare.
            try
            {
                JsonNode? jsonNode;

                // Read the YAML/JSON into a JsonNode. 
                try
                {
                    jsonNode = contentType switch
                    {
                        StreamContentType.Yaml => configReader.YamlToJsonNode(),
                        StreamContentType.Json => configReader.JsonToJsonNode(),
                        _ => throw new CodeGenException($"Stream content type is not supported.")
                    } ?? throw new CodeGenException($"Stream is empty.");

                    // Verify and mutate the configuration before deserialization.
                    OnConfigurationLoad(Scripts, configFileName, jsonNode);
                }
                catch (CodeGenException) { throw; }
                catch (Exception ex) { throw new CodeGenException(ex.Message); }

                // Deserialize the JsonNode in to the configured (script) type.
                try
                {
                    config = (ConfigBase?)jsonNode.Deserialize(Scripts.GetConfigType(), OnRamp.Utility.JsonSerializer.Options);
                }
                catch (CodeGenException) { throw; }
                catch (Exception ex) { throw new CodeGenException(ex.Message); }

                rootConfig = config as IRootConfig ?? throw new InvalidOperationException("Configuration must implement IRootConfig.");
                rootConfig.SetCodeGenArgs(CodeGenArgs);
                rootConfig.MergeRuntimeParameters(CodeGenArgs.Parameters);

                // Instantiate and execute any 'before' custom editors.
                var editors = new List<IConfigEditor>();
                foreach (var cet in Scripts.GetEditors().Distinct())
                {
                    var ce = (IConfigEditor)(Activator.CreateInstance(cet) ?? throw new CodeGenException($"Config Editor {cet.FullName} could not be instantiated."));
                    editors.Add(ce);
                    await ce.BeforePrepareAsync(rootConfig).ConfigureAwait(false);
                }

                await config!.PrepareAsync(config!, config!).ConfigureAwait(false);

                // Execute any 'after' custom editors (in reverse order).
                foreach (var ce in ((IEnumerable<IConfigEditor>)editors).Reverse())
                {
                    await ce.AfterPrepareAsync(rootConfig).ConfigureAwait(false);
                }

                return config;
            }
            catch (CodeGenException cgex)
            {
                throw new CodeGenException($"Config '{configFileName}' is invalid: {cgex.Message}");
            }
        }

        /// <summary>
        /// Execute the code-generation; loads the configuration file and executes each of the scripted templates.
        /// </summary>
        /// <param name="configFileName">The filename (defaults to <see cref="CodeGenArgs"/>) to load the content from the file system (primary) or <see cref="ICodeGeneratorArgs.Assemblies"/> (secondary, recursive until found).</param>
        /// <returns>The resultant <see cref="CodeGenStatistics"/>.</returns>
        /// <exception cref="CodeGenException">Thrown when an error is encountered during the code-generation.</exception>
        /// <exception cref="CodeGenChangesFoundException">Thrown where the code-generation would result in changes to an underlying artefact. This is managed by setting <see cref="ICodeGeneratorArgs.ExpectNoChanges"/> to <c>true</c>.</exception>
        public async Task<CodeGenStatistics> GenerateAsync(string? configFileName = null)
            => await GenerateAsync(await LoadConfigAsync(configFileName).ConfigureAwait(false)).ConfigureAwait(false);

        /// <summary>
        /// Execute the code-generation; loads the configuration from the <paramref name="configReader"/> and executes each of the scripted templates.
        /// </summary>
        /// <param name="configReader">The <see cref="TextReader"/> containing the configuration.</param>
        /// <param name="contentType">The corresponding <see cref="StreamContentType"/>.</param>
        /// <param name="configFileName">The optional configuration file name used specifically in error messages.</param>
        /// <returns>The resultant <see cref="CodeGenStatistics"/>.</returns>
        /// <exception cref="CodeGenException">Thrown when an error is encountered during the code-generation.</exception>
        /// <exception cref="CodeGenChangesFoundException">Thrown where the code-generation would result in changes to an underlying artefact. This is managed by setting <see cref="ICodeGeneratorArgs.ExpectNoChanges"/> to <c>true</c>.</exception>
        public async Task<CodeGenStatistics> GenerateAsync(TextReader configReader, StreamContentType contentType, string configFileName = "<stream>") 
            => await GenerateAsync(await LoadConfigAsync(configReader, contentType, configFileName).ConfigureAwait(false)).ConfigureAwait(false);

        /// <summary>
        /// Executes the code-generation for the specific <paramref name="config"/>.
        /// </summary>
        /// <param name="config">The <see cref="IRootConfig"/> <see cref="ConfigBase"/>.</param>
        public Task<CodeGenStatistics> GenerateAsync(ConfigBase config)
        {
            if (config is not IRootConfig rootConfig)
                throw new ArgumentException("Configuration must implement IRootConfig.", nameof(config));

            CodeGenArgs.Logger?.LogInformation("{Content}", string.Empty);
            CodeGenArgs.Logger?.LogInformation("{Content}", "Scripts:");

            // Generate the scripted artefacts.
            var overallStopwatch = Stopwatch.StartNew();
            var overallStats = new CodeGenStatistics();
            Stopwatch scriptStopwatch;

            foreach (var script in Scripts.Generators!)
            {
                scriptStopwatch = Stopwatch.StartNew();

                // Reset/merge the runtime parameters.
                rootConfig.ResetRuntimeParameters();
                rootConfig.MergeRuntimeParameters(script.RuntimeParameters);
                rootConfig.MergeRuntimeParameters(Scripts.RuntimeParameters);

                var scriptStats = new CodeGenStatistics { RootConfig = rootConfig };
                OnBeforeScript(script, scriptStats);
                script.GetGenerator().Generate(script, config, (oa) => OnCodeGenerated(oa, scriptStats));

                scriptStopwatch.Stop();
                scriptStats.ElapsedMilliseconds = scriptStopwatch.ElapsedMilliseconds;
                OnAfterScript(script, scriptStats);

                overallStats.Add(scriptStats);
            }

            overallStopwatch.Stop();
            overallStats.ElapsedMilliseconds = overallStopwatch.ElapsedMilliseconds;
            return Task.FromResult(overallStats);
        }

        /// <summary>
        /// Provides an opportunity to verify and manipulate the mutable <i>configuration</i> <see cref="JsonNode"/> as it is loaded; directly before it is deserialized into the configured <see cref="ConfigBase"/> as defined by the <see cref="CodeGenScript.ConfigType"/>.
        /// </summary>
        /// <param name="script">The <see cref="CodeGenScript"/>.</param>
        /// <param name="fileName">The configuration file name.</param>
        /// <param name="json">The <see cref="JsonNode"/>.</param>
        protected virtual void OnConfigurationLoad(CodeGenScript script, string fileName, JsonNode json) { }

        /// <summary>
        /// Handles the processing before the <paramref name="script"/> is executed.
        /// </summary>
        /// <param name="script">The <see cref="CodeGenScriptItem"/> to be executed.</param>
        /// <param name="statistics">The corresponding <see cref="CodeGenStatistics"/> for the <paramref name="script"/> execution.</param>
        /// <remarks>Default implementation will <see cref="ILogger">log</see> template details where appropriate.</remarks>
        protected virtual void OnBeforeScript(CodeGenScriptItem script, CodeGenStatistics statistics) => script.Root?.CodeGenArgs?.Logger?.LogInformation(" Template: {template} {text}", script.Template, script.Text == null ? string.Empty : $"({script.Text})");

        /// <summary>
        /// Handles the code generated content after it has been generated.
        /// </summary>
        /// <param name="outputArgs">The <see cref="CodeGenOutputArgs"/>.</param>
        /// <param name="statistics">The <see cref="CodeGenStatistics"/> for the generated artefact.</param>
        /// <remarks>Default implementation will write files (on create or update), update the <paramref name="statistics"/> accordingly, and <see cref="ILogger">log</see> where appropriate.</remarks>
        protected virtual void OnCodeGenerated(CodeGenOutputArgs outputArgs, CodeGenStatistics statistics)
        {
            var di = string.IsNullOrEmpty(outputArgs.DirectoryName) ? outputArgs.Script.Root!.CodeGenArgs!.OutputDirectory! : new DirectoryInfo(Path.Combine(outputArgs.Script.Root!.CodeGenArgs!.OutputDirectory!.FullName, outputArgs.DirectoryName));
            if (!Scripts!.CodeGenArgs!.IsSimulation && !di.Exists)
                di.Create();

            var fi = new FileInfo(Path.Combine(di.FullName, outputArgs.FileName));

            // Check if exists and can be overridden.
            if (outputArgs.Script.IsGenOnce)
            {
                if (string.IsNullOrEmpty(outputArgs.Script.GenOncePattern))
                {
                    if (fi.Exists)
                        return;
                }
                else
                {
                    // Perform a wildcard search and stop code-gen where any matches.
                    if (di.GetFiles(outputArgs.GenOncePattern).Length != 0)
                        return;
                }
            }

            // Convert content into lines.
            var lines = ConvertContentIntoLines(outputArgs.Content);
            statistics.LinesOfCodeCount += lines.Length;

            // Create or override.
            if (fi.Exists)
            {
                var diff = CompareLines(File.ReadAllLines(fi.FullName), lines);
                if (diff is null)
                    statistics.NotChangedCount++;
                else
                {
                    if (Scripts!.CodeGenArgs!.ExpectNoChanges)
                        throw new CodeGenChangesFoundException($"File '{fi.FullName}' would be updated as a result of the code generation:{Environment.NewLine}{diff}");

                    if (!Scripts!.CodeGenArgs!.IsSimulation)
                        File.WriteAllText(fi.FullName, outputArgs.Content);

                    statistics.UpdatedCount++;
                    outputArgs.Script.Root?.CodeGenArgs?.Logger?.LogWarning("    Updated -> {fileName}", fi.FullName);
                }
            }
            else
            {
                if (Scripts!.CodeGenArgs!.ExpectNoChanges)
                    throw new CodeGenChangesFoundException($"File '{fi.FullName}' would be created as a result of the code generation.");

                if (!Scripts!.CodeGenArgs!.IsSimulation)
                    File.WriteAllText(fi.FullName, outputArgs.Content);

                statistics.CreatedCount++;
                outputArgs.Script.Root?.CodeGenArgs?.Logger?.LogWarning("    Created -> {fileName}", fi.FullName);
            }
        }

        /// <summary>
        /// Converts the content into lines.
        /// </summary>
        private static string[] ConvertContentIntoLines(string? content)
        {
            if (content is null)
                return [];

            string line;
            var lines = new List<string>();
            using var sr = new StringReader(content);
            while ((line = sr.ReadLine()) is not null)
            {
                lines.Add(line);
            }

            return [.. lines];
        }

        /// <summary>
        /// Compare the existing lines with the new lines and return initial differences.
        /// </summary>
        private static string? CompareLines(string[] previousLines, string[] newLines)
        {
            var sb = new StringBuilder();
            if (previousLines.Length != newLines.Length)
                sb.AppendLine($"> Line count difference; previous '{previousLines.Length}' versus generated '{newLines.Length}'.");

            for (int i = 0; i < Math.Min(previousLines.Length, newLines.Length); i ++)
            {
                if (string.Compare(previousLines[i], newLines[i], StringComparison.InvariantCulture) != 0)
                {
                    sb.AppendLine($"> Line '{i + 1}' content difference (no further lines compared);");
                    sb.AppendLine($">  previous--> {previousLines[i]}");
                    sb.AppendLine($">  generated-> {newLines[i]}");
                }
            }

            return sb.Length > 0 ? sb.ToString() : null;
        }

        /// <summary>
        /// Handles the processing after the <paramref name="script"/> is executed.
        /// </summary>
        /// <param name="script">The <see cref="CodeGenScriptItem"/> to be executed.</param>
        /// <param name="statistics">The corresponding <see cref="CodeGenStatistics"/> for the <paramref name="script"/> execution.</param>
        /// <remarks>Default implementation will <see cref="ILogger">log</see> <paramref name="statistics"/> where appropriate.</remarks>
        protected virtual void OnAfterScript(CodeGenScriptItem script, CodeGenStatistics statistics) => script.Root?.CodeGenArgs?.Logger?.LogInformation("  {stats}", statistics);
    }
}