﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System.Threading.Tasks;

namespace OnRamp.Console
{
    /// <summary>
    /// Provides the direct console capabilities.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The main entry point.
        /// </summary>
        /// <param name="args">The console arguments.</param>
        /// <returns><b>Zero</b> indicates success; otherwise, unsuccessful.</returns>
        internal static async Task<int> Main(string[] args) => await new CodeGenConsole().RunAsync(args).ConfigureAwait(false);
    }
}