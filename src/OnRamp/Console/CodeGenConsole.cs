// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnRamp.Console
{
    /// <summary>
    /// Console that facilitates the code generation by managing the standard console command-line arguments/options.
    /// </summary>
    /// <remarks>The standard console command-line arguments/options can be controlled via the constructor using the <see cref="SupportedOptions"/> flags. Additional capabilities can be added by inherting and overridding the
    /// <see cref="OnBeforeExecute(CommandLineApplication)"/>, <see cref="OnValidation(ValidationContext)"/> and <see cref="OnCodeGenerationAsync"/>. Changes to the console output can be achieved by overridding
    /// <see cref="OnWriteMasthead"/>, <see cref="OnWriteHeader"/>, <see cref="OnWriteArgs(ICodeGeneratorArgs)"/> and <see cref="OnWriteFooter(CodeGenStatistics)"/>.
    /// <para>The underlying command line parsing is provided by <see href="https://natemcmaster.github.io/CommandLineUtils/"/>.</para></remarks>
    /// <param name="args">The default <see cref="CodeGeneratorArgs"/> that will be overridden/updated by the command-line argument values.</param>
    /// <param name="options">The console command-line <see cref="SupportedOptions"/>; defaults to <see cref="SupportedOptions.All"/>.</param>
    public class CodeGenConsole(CodeGeneratorArgs? args = null, SupportedOptions options = SupportedOptions.All)
    {
        private readonly SupportedOptions _supportedOptions = options;
        private readonly Dictionary<SupportedOptions, CommandOption?> _options = [];

        /// <summary>
        /// Split an <paramref name="args"/> <see cref="string"/> into an <see cref="Array"/> of arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The <see cref="Array"/> of arguments.</returns>
        public static string[] SplitArgumentsIntoArray(string? args)
        {
            if (string.IsNullOrEmpty(args))
                return [];

            // See for inspiration: https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp/298990#298990
            var regex = Regex.Matches(args, @"\G(""((""""|[^""])+)""|(\S+)) *");
            return regex.Cast<Match>()
                        .Select(m => Regex.Replace(
                            m.Groups[2].Success
                                ? m.Groups[2].Value
                                : m.Groups[4].Value, @"""""", @"""")).ToArray();
        }

        /// <summary>
        /// Gets the base executable directory path.
        /// </summary>
        /// <returns>The base executable directory path.</returns>
        /// <remarks>Uses <see cref="Environment.CurrentDirectory"/> and removes the last instance of <c>bin/debug</c> or <c>bin/release</c> where found in the path to find the base executable directory for code generation.</remarks>
        public static string GetBaseExeDirectory()
        {
            var exeDir = Environment.CurrentDirectory;
            var i = exeDir.LastIndexOf(Path.Combine("bin", "debug"), StringComparison.InvariantCultureIgnoreCase);
            if (i > 0)
                return exeDir[0..i];

            i = exeDir.LastIndexOf(Path.Combine("bin", "release"), StringComparison.InvariantCultureIgnoreCase);
            if (i > 0)
                return exeDir[0..i];

            return exeDir;
        }

        /// <summary>
        /// Gets the <see cref="CodeGeneratorArgs"/>.
        /// </summary>
        public CodeGeneratorArgs Args { get; } = args ?? new CodeGeneratorArgs();

        /// <summary>
        /// Gets the application/command name.
        /// </summary>
        public virtual string AppName => (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName()?.Name ?? "UNKNOWN";

        /// <summary>
        /// Gets the application/command title. 
        /// </summary>
        public virtual string AppTitle => $"{AppName} Code Generation Tool.";

        /// <summary>
        /// Gets the <see cref="Args"/> <see cref="ICodeGeneratorArgs.Logger"/>.
        /// </summary>
        protected ILogger? Logger => Args.Logger;

        /// <summary>
        /// Indicates whether to bypass standard execution of <see cref="OnWriteMasthead"/>, <see cref="OnWriteHeader"/>, <see cref="OnWriteArgs(ICodeGeneratorArgs)"/> and <see cref="OnWriteFooter(CodeGenStatistics)"/>.
        /// </summary>
        protected bool BypassOnWrites { get; set; }

        /// <summary>
        /// Gets or sets the masthead text used by <see cref="OnWriteMasthead"/>.
        /// </summary>
        /// <remarks>Defaults to 'OnRamp Code-Gen Tool' formatted using <see href="https://www.patorjk.com/software/taag/#p=display&amp;f=Calvin%20S&amp;t=OnRamp%20Code-Gen%20Tool"/>.</remarks>
        public string? MastheadText { get; protected set; } = @"
╔═╗┌┐┌╦═╗┌─┐┌┬┐┌─┐  ╔═╗┌─┐┌┬┐┌─┐  ╔═╗┌─┐┌┐┌  ╔╦╗┌─┐┌─┐┬  
║ ║│││╠╦╝├─┤│││├─┘  ║  │ │ ││├┤───║ ╦├┤ │││   ║ │ ││ ││  
╚═╝┘└┘╩╚═┴ ┴┴ ┴┴    ╚═╝└─┘─┴┘└─┘  ╚═╝└─┘┘└┘   ╩ └─┘└─┘┴─┘
";

        /// <summary>
        /// Runs the code generation using the passed <paramref name="args"/> string.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsuccessful.</returns>
        public async Task<int> RunAsync(string? args = null) => await RunAsync(SplitArgumentsIntoArray(args)).ConfigureAwait(false);

        /// <summary>
        /// Runs the code generation using the passed <paramref name="args"/> array.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsuccessful.</returns>
        public async Task<int> RunAsync(string[] args)
        {
            Args.Logger ??= new ConsoleLogger(PhysicalConsole.Singleton);
            Utility.HandlebarsHelpers.Logger ??= Args.Logger;

            // Set up the app.
            using var app = new CommandLineApplication(PhysicalConsole.Singleton) { Name = AppName, Description = AppTitle };
            app.HelpOption();

            _options.Add(SupportedOptions.ScriptFileName, _supportedOptions.HasFlag(SupportedOptions.ScriptFileName) ? app.Option("-s|--script", "Script orchestration file/resource name.", CommandOptionType.SingleValue) : null);
            _options.Add(SupportedOptions.ConfigFileName, _supportedOptions.HasFlag(SupportedOptions.ConfigFileName) ? app.Option("-c|--config", "Configuration data file name.", CommandOptionType.SingleValue) : null);
            _options.Add(SupportedOptions.OutputDirectory, _supportedOptions.HasFlag(SupportedOptions.OutputDirectory) ? app.Option("-o|--output", "Output directory path.", CommandOptionType.MultipleValue).Accepts(v => v.ExistingDirectory("Output directory path does not exist.")) : null);
            _options.Add(SupportedOptions.Assemblies, _supportedOptions.HasFlag(SupportedOptions.Assemblies) ? app.Option("-a|--assembly", "Assembly containing embedded resources (multiple can be specified in probing order).", CommandOptionType.MultipleValue) : null);
            _options.Add(SupportedOptions.Parameters, _supportedOptions.HasFlag(SupportedOptions.Parameters) ? app.Option("-p|--param", "Parameter expressed as a 'Name=Value' pair (multiple can be specified).", CommandOptionType.MultipleValue) : null);
            _options.Add(SupportedOptions.DatabaseConnectionString, _supportedOptions.HasFlag(SupportedOptions.DatabaseConnectionString) ? app.Option("-cs|--connection-string", "Database connection string.", CommandOptionType.SingleValue) : null);
            _options.Add(SupportedOptions.DatabaseConnectionStringEnvironmentVariableName, _supportedOptions.HasFlag(SupportedOptions.DatabaseConnectionStringEnvironmentVariableName) ? app.Option("-cv|--connection-varname", "Database connection string environment variable name.", CommandOptionType.SingleValue) : null);
            _options.Add(SupportedOptions.ExpectNoChanges, _supportedOptions.HasFlag(SupportedOptions.ExpectNoChanges) ? app.Option("-enc|--expect-no-changes", "Indicates to expect _no_ changes in the artefact output (e.g. error within build pipeline).", CommandOptionType.NoValue) : null);
            _options.Add(SupportedOptions.IsSimulation, _supportedOptions.HasFlag(SupportedOptions.IsSimulation) ? app.Option("-sim|--simulation", "Indicates whether the code-generation is a simulation (i.e. does not create/update any artefacts).", CommandOptionType.NoValue) : null);

            OnBeforeExecute(app);

            // Set up the code generation validation.
            app.OnValidate(ctx =>
            {
                // Update the options from command line.
                UpdateStringOption(SupportedOptions.ScriptFileName, v => Args.ScriptFileName = v);
                UpdateStringOption(SupportedOptions.ConfigFileName, v => Args.ConfigFileName = v);
                UpdateBooleanOption(SupportedOptions.ExpectNoChanges, () => Args.ExpectNoChanges = true);
                UpdateBooleanOption(SupportedOptions.IsSimulation, () => Args.IsSimulation = true);
                UpdateStringOption(SupportedOptions.OutputDirectory, v => Args.OutputDirectory = new DirectoryInfo(v));

                var vr = ValidateMultipleValue(SupportedOptions.Assemblies, ctx, (ctx, co) => new AssemblyValidator(Args).GetValidationResult(co, ctx));
                if (vr != ValidationResult.Success)
                    return vr;

                vr = ValidateMultipleValue(SupportedOptions.Parameters, ctx, (ctx, co) => new ParametersValidator(Args).GetValidationResult(co, ctx));
                if (vr != ValidationResult.Success)
                    return vr;

                // Handle the connection string, in order of precedence: command-line argument, environment variable, what was passed as initial argument.
                var cs = GetCommandOption(SupportedOptions.DatabaseConnectionString);
                if (cs != null)
                {
                    var evn = GetCommandOption(SupportedOptions.DatabaseConnectionStringEnvironmentVariableName)?.Value();
                    if (!string.IsNullOrEmpty(evn))
                        Args.ConnectionStringEnvironmentVariableName = evn;
                }

                Args.OverrideConnectionString(cs?.Value());

                // Invoke any additional.
                return OnValidation(ctx)!;
            });

            // Set up the code generation execution.
            app.OnExecuteAsync(async _ => await RunRunawayAsync().ConfigureAwait(false));

            // Execute the command-line app.
            try
            {
                return await app.ExecuteAsync(args).ConfigureAwait(false);
            }
            catch (CommandParsingException cpex)
            {
                Args.Logger?.LogError("{Content}", cpex.Message);
                Args.Logger?.LogError("{Content}", string.Empty);
                return 1;
            }
            catch (CodeGenException cgex)
            {
                Args.Logger?.LogError("{Content}", cgex.Message);
                Args.Logger?.LogError("{Content}", string.Empty);
                return 2;
            }
        }

        /// <summary>
        /// Gets the selected <see cref="CommandOption"/> for the specfied <paramref name="option"/> selection.
        /// </summary>
        /// <param name="option">The <see cref="SupportedOptions"/> option.</param>
        /// <returns>The corresponding <see cref="CommandOption"/> where found; otherwise, <c>null</c>.</returns>
        protected CommandOption? GetCommandOption(SupportedOptions option) => _options.GetValueOrDefault(option);

        /// <summary>
        /// Updates the command option from a string option.
        /// </summary>
        private void UpdateStringOption(SupportedOptions option, Action<string?> action)
        {
            var co = GetCommandOption(option);
            if (co != null && co.HasValue())
            {
                var val = co.Value();
                if (!string.IsNullOrEmpty(val))
                    action.Invoke(val);
            }
        }

        /// <summary>
        /// Updates the command option from a boolean option.
        /// </summary>
        private void UpdateBooleanOption(SupportedOptions option, Action action)
        {
            var co = GetCommandOption(option);
            if (co != null && co.HasValue())
                action.Invoke();
        }

        /// <summary>
        /// Validate multiple options.
        /// </summary>
        private ValidationResult ValidateMultipleValue(SupportedOptions option, ValidationContext ctx, Func<ValidationContext, CommandOption, ValidationResult> func)
        {
            var co = GetCommandOption(option);
            if (co == null)
                return ValidationResult.Success!;
            else
                return func(ctx, co);
        }

        /// <summary>
        /// Invoked before the underlying console execution occurs.
        /// </summary>
        /// <param name="app">The underlying <see cref="CommandLineApplication"/>.</param>
        /// <remarks>This enables additional configuration to the <paramref name="app"/> prior to execution. For example, adding additional command line arguments.</remarks>
        protected virtual void OnBeforeExecute(CommandLineApplication app) { }

        /// <summary>
        /// Invoked after command parsing is complete and before the underlying code-generation.
        /// </summary>
        /// <param name="context">The <see cref="ValidationContext"/>.</param>
        /// <returns>The <see cref="ValidationResult"/>.</returns>
        protected virtual ValidationResult? OnValidation(ValidationContext context) => ValidationResult.Success;

        /// <summary>
        /// Performs the actual code-generation.
        /// </summary>
        private async Task<int> RunRunawayAsync() /* Method name inspired by: Slade - Run Runaway - https://www.youtube.com/watch?v=gMxcGaAwy-Q */
        {
            try
            {
                // Write header, etc.
                if (!BypassOnWrites)
                {
                    OnWriteMasthead();
                    OnWriteHeader();
                    OnWriteArgs(Args);
                }

                // Run the code generator.
                var stats = (await OnCodeGenerationAsync().ConfigureAwait(false)) ?? throw new InvalidOperationException($"A {nameof(CodeGenStatistics)} instance must be returned from {nameof(OnCodeGenerationAsync)}.");

                // Write footer and exit successfully.
                if (!BypassOnWrites)
                    OnWriteFooter(stats);

                return 0;
            }
            catch (CodeGenException cgex)
            {
                if (cgex.Message != null)
                {
                    Args.Logger?.LogError("{Content}", cgex.Message);
                    Args.Logger?.LogError("{Content}", string.Empty);
                }

                return 2;
            }
            catch (CodeGenChangesFoundException cgcfex)
            {
                Args.Logger?.LogError("{Content}", cgcfex.Message);
                Args.Logger?.LogError("{Content}", string.Empty);
                return 3;
            }
        }

        /// <summary>
        /// Invoked to write the <see cref="MastheadText"/> to the <see cref="Logger"/>.
        /// </summary>
        protected virtual void OnWriteMasthead()
        {
            if (MastheadText != null)
                Logger?.LogInformation("{Content}", MastheadText);
        }

        /// <summary>
        /// Invoked to instantiate and run a <see cref="CodeGenerator"/> using the <see cref="Args"/> returning the corresponding <see cref="CodeGenStatistics"/>.
        /// </summary>
        /// <remarks>The code invoked internally is: <c>return new CodeGenerator(Args).Generate();</c></remarks>
        /// <returns>The <see cref="CodeGenStatistics"/> where successful.</returns>
        protected virtual async Task<CodeGenStatistics> OnCodeGenerationAsync()
        {
            var cg = await CodeGenerator.CreateAsync(Args).ConfigureAwait(false);
            return await cg.GenerateAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Invoked to write the header information to the <see cref="Logger"/>.
        /// </summary>
        /// <remarks>Writes the <see cref="AppTitle"/>.</remarks>
        protected virtual void OnWriteHeader()
        {
            Logger?.LogInformation("{Content}", AppTitle);
            Logger?.LogInformation("{Content}", string.Empty);
        }

        /// <summary>
        /// Invoked to write the <see cref="Args"/> to the <see cref="Logger"/>.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/> to write.</param>
        protected virtual void OnWriteArgs(ICodeGeneratorArgs args) => WriteStandardizedArgs(args);

        /// <summary>
        /// Write the <see cref="Args"/> to the <see cref="Logger"/> in a standardized (reusable) manner.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/> to write.</param>
        public static void WriteStandardizedArgs(ICodeGeneratorArgs args)
        {
            if (args == null || args.Logger == null)
                return;

            args.Logger.LogInformation("{Content}", $"Config = {args.ConfigFileName}");
            args.Logger.LogInformation("{Content}", $"Script = {args.ScriptFileName}");
            args.Logger.LogInformation("{Content}", $"OutDir = {args.OutputDirectory?.FullName}");
            args.Logger.LogInformation("{Content}", $"ExpectNoChanges = {args.ExpectNoChanges}");
            args.Logger.LogInformation("{Content}", $"IsSimulation = {args.IsSimulation}");

            args.Logger.LogInformation("{Content}", $"Parameters{(args.Parameters.Count == 0 ? " = none" : ":")}");
            foreach (var p in args.Parameters)
            {
                args.Logger.LogInformation("{Content}", $"  {p.Key} = {p.Value}");
            }

            args.Logger.LogInformation("{Content}", $"Assemblies{(args.Assemblies.Count == 0 ? " = none" : ":")}");
            foreach (var a in args.Assemblies)
            {
                args.Logger.LogInformation("{Content}", $"  {a.FullName}");
            }
        }

        /// <summary>
        /// Invoked to write the footer (<see cref="CodeGenStatistics.ToSummaryString"/>) information to the <see cref="Logger"/>.
        /// </summary>
        /// <param name="stats">The <see cref="CodeGenStatistics"/> information.</param>
        protected virtual void OnWriteFooter(CodeGenStatistics stats)
        {
            Logger?.LogInformation("{Content}", string.Empty);
            Logger?.LogInformation("{Content}", $"{AppName} Complete. {stats.ToSummaryString()}");
            Logger?.LogInformation("{Content}", string.Empty);
        }
    }
}