// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;

namespace OnRamp.Config
{
    /// <summary>
    /// Represents the <i>code-generation</i> property collection configuration.
    /// </summary>
    /// <remarks>The property should be a collection of strings or <see cref="ConfigBase"/> objects, or have explicitly declared scalar <see cref="ItemJsonTypes"/> for reference generation.</remarks>
    /// <param name="category">The grouping category.</param>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CodeGenPropertyCollectionAttribute(string category) : Attribute
    {
        /// <summary>
        /// Gets or sets the category.
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

        /// <summary>
        /// Gets or sets the markdown.
        /// </summary>
        public string? Markdown { get; set; }

        /// <summary>
        /// Indicates whether the property is mandatory.
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Inidicates whether the property is considered important.
        /// </summary>
        public bool IsImportant { get; set; }

        /// <summary>
        /// Gets or sets the permitted JSON scalar types for collection items, for reference generation only.
        /// </summary>
        /// <remarks>Defaults to <c>null</c> to infer the existing item mapping. Explicit declarations must be nonempty and compatible with the item CLR type; <c>JsonElement</c> items require an explicit declaration. Item types never implicitly include null. Types are deduplicated and ordered as declared by <see cref="CodeGenJsonType"/>.</remarks>
        public CodeGenJsonType[]? ItemJsonTypes { get; set; }
    }
}