// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;

namespace OnRamp.Config
{
    /// <summary>
    /// Represents the <i>code-generation</i> class category configuration.
    /// </summary>
    /// <param name="category">The grouping category name.</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class CodeGenCategoryAttribute(string category) : Attribute
    {
        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string Category { get; } = category;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; }
    }
}