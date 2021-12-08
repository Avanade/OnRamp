// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using OnRamp.Scripts;
using System;

namespace OnRamp
{
    /// <summary>
    /// The resulting <see cref="CodeGenOutputArgs"/> arguments.
    /// </summary>
    public class CodeGenOutputArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenOutputArgs"/> class.
        /// </summary>
        /// <param name="script">The corresponding <see cref="CodeGenScriptItem"/>.</param>
        /// <param name="directoryName">The optional generated directory name.</param>
        /// <param name="fileName">The generated file name.</param>
        /// <param name="genOncePattern">The generated gen-once file name.</param>
        /// <param name="content">The generated content.</param>
        public CodeGenOutputArgs(CodeGenScriptItem script, string? directoryName, string fileName, string? genOncePattern, string? content)
        {
            Script = script ?? throw new ArgumentNullException(nameof(script));
            DirectoryName = directoryName;
            FileName = string.IsNullOrEmpty(fileName) ? throw new ArgumentNullException(nameof(fileName)) : fileName;
            GenOncePattern = genOncePattern;
            Content = content;
        }

        /// <summary>
        /// Gets the <see cref="CodeGenScriptItem"/>.
        /// </summary>
        public CodeGenScriptItem Script { get; }

        /// <summary>
        /// Gets the optional generated directory name.
        /// </summary>
        public string? DirectoryName { get; }

        /// <summary>
        /// Gets the generated file name.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the generated content.
        /// </summary>
        public string? Content { get; }

        /// <summary>
        /// Gets the gen-once file name pattern (where specified).
        /// </summary>
        public string? GenOncePattern { get; }
    }
}