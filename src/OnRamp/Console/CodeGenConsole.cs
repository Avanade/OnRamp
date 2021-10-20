// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;
using System.Reflection;

namespace OnRamp.Console
{
    /// <summary>
    /// Console that facilitates the code generation by managing the standard console command-line arguments/options.
    /// </summary>
    /// <remarks>The standard console command-line arguments/options can be controlled via the constructor using the <see cref="SupportedOptions"/> flags.</remarks>
    public sealed class CodeGenConsole : CodeGenConsoleBase
    {
        /// <summary>
        /// Creates a new <see cref="CodeGenConsole"/> using <typeparamref name="T"/> to determine <see cref="Assembly"/> defaulting <see cref="CodeGenConsoleBase.Name"/> (with <see cref="AssemblyName.Name"/>),
        /// <see cref="CodeGenConsoleBase.Text"/> (with <see cref="AssemblyProductAttribute.Product"/>), <see cref="CodeGenConsoleBase.Description"/> (with <see cref="AssemblyDescriptionAttribute.Description"/>), 
        /// and <see cref="Version"/> (with <see cref="AssemblyName.Version"/>).
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>.</typeparam>
        /// <param name="args">The default <see cref="CodeGeneratorArgs"/> that will be overridden or updated by the command-line argument values.</param>
        /// <param name="name">The application/command name; defaults to <see cref="AssemblyName.Name"/>.</param>
        /// <param name="text">The application/command short text.</param>
        /// <param name="description">The application/command description; defaults to <paramref name="text"/> when not specified.</param>
        /// <param name="version">The application/command version number.</param>
        /// <param name="options">The console command-line <see cref="SupportedOptions"/>; defaults to <see cref="SupportedOptions.All"/>.</param>
        /// <returns>A new <see cref="CodeGenConsole"/>.</returns>
        public static CodeGenConsole Create<T>(CodeGeneratorArgs? args = null, string? name = null, string? text = null, string? description = null, string? version = null, SupportedOptions options = SupportedOptions.All)
            => new(typeof(T).Assembly, args, name, text, description, version, options);

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenConsole"/> class.
        /// </summary>
        /// <param name="name">The application/command name.</param>
        /// <param name="text">The application/command short text.</param>
        /// <param name="description">The application/command description; will default to <paramref name="text"/> when not specified.</param>
        /// <param name="version">The application/command version number.</param>
        /// <param name="options">The console command-line <see cref="SupportedOptions"/>; defaults to <see cref="SupportedOptions.All"/>.</param>
        /// <param name="args">The default <see cref="CodeGeneratorArgs"/> that will be overridden/updated by the command-line argument values.</param>
        public CodeGenConsole(string name, string text, string? description = null, string? version = null, SupportedOptions options = SupportedOptions.All, CodeGeneratorArgs? args = null)
            : base(name, text, description, version, options, args) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenConsole"/> class defaulting <see cref="CodeGenConsoleBase.Name"/> (with <see cref="AssemblyName.Name"/>), <see cref="CodeGenConsoleBase.Text"/> 
        /// (with <see cref="AssemblyProductAttribute.Product"/>), <see cref="CodeGenConsoleBase.Description"/> (with <see cref="AssemblyDescriptionAttribute.Description"/>), and <see cref="Version"/> 
        /// (with <see cref="AssemblyName.Version"/>) from the <paramref name="assembly"/> where not expressly provided.
        /// </summary>
        /// <param name="args">The default <see cref="CodeGeneratorArgs"/> that will be overridden/updated by the command-line argument values.</param>
        /// <param name="assembly">The <see cref="Assembly"/> to infer properties where not expressly provided.</param>
        /// <param name="name">The application/command name; defaults to <see cref="AssemblyName.Name"/>.</param>
        /// <param name="text">The application/command short text.</param>
        /// <param name="description">The application/command description; defaults to <paramref name="text"/> when not specified.</param>
        /// <param name="version">The application/command version number.</param>
        /// <param name="options">The console command-line <see cref="SupportedOptions"/>; defaults to <see cref="SupportedOptions.All"/>.</param>
        public CodeGenConsole(Assembly assembly, CodeGeneratorArgs? args = null, string? name = null, string? text = null, string? description = null, string? version = null, SupportedOptions options = SupportedOptions.All)
            : base(assembly, args, name, text, description, version, options) { }
    }
}