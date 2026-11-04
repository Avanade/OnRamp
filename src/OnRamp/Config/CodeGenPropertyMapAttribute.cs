// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;

namespace OnRamp.Config
{
    /// <summary>
    /// Represents the <i>code-generation</i> property map configuration.
    /// </summary>
    /// <remarks>The property must be a <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> with string keys and values derived from <see cref="ConfigBase"/>.</remarks>
    /// <param name="category">The grouping category.</param>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CodeGenPropertyMapAttribute(string category) : Attribute
    {
        /// <summary>
        /// Gets the category.
        /// </summary>
        public string Category { get; } = category ?? throw new ArgumentNullException(nameof(category));

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the Markdown.
        /// </summary>
        public string? Markdown { get; set; }

        /// <summary>
        /// Indicates whether the property is mandatory.
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Indicates whether the property is considered important.
        /// </summary>
        public bool IsImportant { get; set; }
    }
}
