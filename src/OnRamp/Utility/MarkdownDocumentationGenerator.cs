// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using OnRamp.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace OnRamp.Utility
{
    /// <summary>
    /// Represents a <c>Markdown</c> documentation file <see cref="Generate">generator</see>.
    /// </summary>
    /// <remarks>Uses the <i>code-generation</i> attributes (<see cref="CodeGenClassAttribute"/>, <see cref="CodeGenCategoryAttribute"/>, <see cref="CodeGenPropertyAttribute"/> and <see cref="CodeGenPropertyCollectionAttribute"/>) and <see cref="System.ComponentModel.DefaultValueAttribute"/> as the documentation content source. Follows configuration objects and collections once per type, including cycles, without constructing or preparing configurations.</remarks>
    public static class MarkdownDocumentationGenerator
    {
        /// <summary>
        /// Generates <c>Markdown</c> for each <see cref="Type"/> (recursively) from the specified root <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The root <see cref="Type"/> to derive the schema from.</typeparam>
        /// <param name="createFileName">The function to create the file name; defaults to '<c>Xxx.md</c>' where 'Xxx' is the <see cref="CodeGenClassAttribute.Name"/>.</param>
        /// <param name="directory">The directory in which to create the markdown files; defaults to <see cref="Environment.CurrentDirectory"/>.</param>
        /// <param name="includeExample">Indicates whether to include the <see cref="CodeGenClassAttribute.Markdown"/> in the file.</param>
        /// <param name="addBreaksBetweenSections">Indicates whether to include additional breaks (<c>&lt;br/&gt;</c>) between sections.</param>
        /// <param name="tableTitleTextSplitHtml">The HTML that will be used to split the title and text content within the table output.</param>
        /// <param name="propertyData">The action invoked for every documented property on each generated page to optionally manipulate the <see cref="MarkdownDocumentationGeneratorPropertyData"/> before writing.</param>
        /// <param name="fileCreation">The action invoked once per generated page to optionally perform other work, such as logging, on file create.</param>
        public static void Generate<T>(Func<Type, CodeGenClassAttribute, string>? createFileName = null, string? directory = null, bool includeExample = true, bool addBreaksBetweenSections = false, string? tableTitleTextSplitHtml = "<br/>&dagger; ", Action<MarkdownDocumentationGeneratorPropertyData>? propertyData = null, Action<string>? fileCreation = null) where T : ConfigBase, IRootConfig
        {
            ReferenceGeneration.FindTypes(typeof(T));
            var fileNames = new Dictionary<Type, string>();
            WriteObject(typeof(T), GetFileName, directory ?? Environment.CurrentDirectory, includeExample, addBreaksBetweenSections, tableTitleTextSplitHtml, propertyData, fileCreation, new HashSet<Type>());

            string GetFileName(Type type, CodeGenClassAttribute csa)
            {
                if (!fileNames.TryGetValue(type, out var name))
                {
                    name = createFileName == null ? $"{csa.Name}.md" : createFileName(type, csa);
                    if (name == null)
                        throw new InvalidOperationException("The createFileName function must not return a null.");

                    fileNames.Add(type, name);
                }

                return name;
            }
        }

        /// <summary>
        /// Writes the markdown for the object.
        /// </summary>
        private static void WriteObject(Type type, Func<Type, CodeGenClassAttribute, string> createFileName, string directory, bool includeExample, bool addBreaksBetweenSections, string? tableTitleTextSplitHtml, Action<MarkdownDocumentationGeneratorPropertyData>? propertyData, Action<string>? fileCreation, HashSet<Type> visited)
        {
            if (!visited.Add(type))
                return;

            var csa = ReferenceGeneration.GetClass(type);
            // Get file name and create.
            var fn = createFileName(type, csa) ?? throw new InvalidOperationException("The createFileName function must not return a null.");
            fileCreation?.Invoke(fn);
            using var tw = File.CreateText(new FileInfo(Path.Combine(directory, fn)).FullName);

            // Get all the properties prior to write.
            var pdlist = new List<MarkdownDocumentationGeneratorPropertyData>();
            var properties = new Dictionary<MarkdownDocumentationGeneratorPropertyData, ReferenceGeneration.Property>();
            foreach (var pi in type.GetProperties())
            {
                var jpa = pi.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (jpa == null)
                    continue;

                var pd = new MarkdownDocumentationGeneratorPropertyData
                {
                    Type = type,
                    Class = csa,
                    Name = jpa.Name ?? StringConverter.ToCamelCase(pi.Name),
                    Property = pi,
                    Psa = pi.GetCustomAttribute<CodeGenPropertyAttribute>()
                };

                if (pd.Psa == null)
                {
                    pd.Pcsa = pi.GetCustomAttribute<CodeGenPropertyCollectionAttribute>();
                    if (pd.Pcsa == null)
                        throw new InvalidOperationException($"Type '{type.Name}' Property '{pi.Name}' does not have a required PropertySchemaAttribute or PropertyCollectionSchemaAttribute.");

                    pd.Category = pd.Pcsa.Category;
                }
                else
                    pd.Category = pd.Psa.Category;

                propertyData?.Invoke(pd);

                pdlist.Add(pd);
                properties.Add(pd, ReferenceGeneration.GetProperty(pd.Property!, pd.Psa, pd.Pcsa));
            }

            var title = csa.Title;
            if (title != null && title.EndsWith('.'))
                title = title[..^1];

            tw.WriteLine($"# {title}");
            tw.WriteLine();
            tw.WriteLine(csa.Description);
            if (!string.IsNullOrEmpty(csa.Markdown))
            {
                tw.WriteLine();
                tw.WriteLine(csa.Markdown);
            }

            tw.WriteLine();
            if (addBreaksBetweenSections)
            {
                tw.WriteLine("<br/>");
                tw.WriteLine();
            }

            if (includeExample && !string.IsNullOrEmpty(csa.ExampleMarkdown))
            {
                tw.WriteLine("## Example");
                tw.WriteLine();
                tw.WriteLine(csa.ExampleMarkdown);
                tw.WriteLine();
                if (addBreaksBetweenSections)
                {
                    tw.WriteLine("<br/>");
                    tw.WriteLine();
                }
            }

            var cats = type.GetCustomAttributes<CodeGenCategoryAttribute>();

            if (cats.Count() > 1)
            {
                tw.WriteLine("## Property categories");
                tw.WriteLine($"The `{csa.Name}` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.");
                tw.WriteLine();
                tw.WriteLine("Category | Description");
                tw.WriteLine("-|-");

                foreach (var cat in cats)
                {
                    tw.WriteLine($"[`{cat.Category}`](#{cat.Category.Replace(".", "")}) | {cat.Title}");
                }

                tw.WriteLine();
                tw.WriteLine("The properties with a bold name are those that are more typically used (considered more important).");
                tw.WriteLine();
                if (addBreaksBetweenSections)
                {
                    tw.WriteLine("<br/>");
                    tw.WriteLine();
                }
            }
            else
            {
                tw.WriteLine("## Properties");
                tw.WriteLine($"The `{csa.Name}` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).");
                tw.WriteLine();
            }

            foreach (var cat in cats)
            {
                if (cats.Count() > 1)
                {
                    tw.WriteLine($"## {cat.Category}");
                    tw.Write(cat.Title);
                    if (cat.Description != null)
                        tw.Write($" {cat.Description}");

                    tw.WriteLine();
                    tw.WriteLine();
                }

                tw.WriteLine("Property | Description");
                tw.WriteLine("-|-");

                foreach (var p in pdlist.Where(x => x.Category == cat.Category))
                {
                    var reference = properties[p];
                    string? childText = null;
                    if (reference.ChildType != null)
                    {
                        var childClass = ReferenceGeneration.GetClass(reference.ChildType);
                        var childFile = createFileName(reference.ChildType, childClass);
                        childText = $"The corresponding [`{childClass.Name}`]({childFile}) {(p.Psa == null ? "collection" : "object")}.";
                    }

                    var declared = p.Psa?.JsonTypes ?? p.Pcsa?.ItemJsonTypes;
                    var scalarText = declared == null ? null : $"{(p.Psa == null ? "Item JSON types" : "JSON types")}: {string.Join(", ", reference.JsonTypes!.Select(x => $"`{x}`"))}.";
                    if (p.Psa != null)
                    {
                        var propertyTitle = p.Psa.Title;
                        if (childText != null)
                            propertyTitle = propertyTitle == null ? childText : $"{propertyTitle} {childText}";

                        WriteTableItem(tw, p.Name, propertyTitle, p.Psa.Description, null, p.Psa.IsMandatory, p.Psa.IsImportant, tableTitleTextSplitHtml, p.Psa.Options, scalarText, reference.DefaultJson);
                    }
                    else
                        WriteTableItem(tw, p.Name, childText ?? p.Pcsa!.Title, p.Pcsa!.Description, p.Pcsa.Markdown, p.Pcsa.IsMandatory, p.Pcsa.IsImportant, tableTitleTextSplitHtml, scalarTypes: scalarText, defaultJson: reference.DefaultJson);
                }

                tw.WriteLine();

                if (addBreaksBetweenSections && cats.Last() != cat)
                {
                    tw.WriteLine("<br/>");
                    tw.WriteLine();
                }
            }

            tw.Close();

            foreach (var p in pdlist)
            {
                var childType = properties[p].ChildType;
                if (childType != null)
                    WriteObject(childType, createFileName, directory, includeExample, addBreaksBetweenSections, tableTitleTextSplitHtml, propertyData, fileCreation, visited);
            }
        }

        /// <summary>
        /// Write the table line.
        /// </summary>
        private static void WriteTableItem(TextWriter tw, string? name, string? title, string? description = null, string? markdown = null, bool isMandatory = false, bool isImportant = false, string? tableTitleTextSplitHtml = null, string[]? options = null, string? scalarTypes = null, string? defaultJson = null)
        {
            if (isMandatory || isImportant)
                tw.Write("**");

            tw.Write($"`{name}`");

            if (isMandatory || isImportant)
                tw.Write("**");

            tw.Write($" | {title}");

            if (options != null)
            {
                tw.Write(" Valid options are: ");
                var isFirst = true;
                foreach (var o in options)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        tw.Write(", ");

                    tw.Write($"`{o}`");
                }

                tw.Write(".");
            }

            if (isMandatory)
                tw.Write(" [Mandatory]");

            if (scalarTypes != null)
                tw.Write($" {scalarTypes}");

            if (defaultJson != null)
                tw.Write($" Default: `{defaultJson.Replace("|", "&#124;").Replace("`", "&#96;")}`.");
            
            if (description != null)
                tw.Write($"{tableTitleTextSplitHtml ?? " "}{description}");

            if (markdown != null)
                tw.Write($"<br/><br/>{markdown}");

            tw.WriteLine();
        }
    }

    /// <summary>
    /// Provides the property data used for the markdown schema generation.
    /// </summary>
    public class MarkdownDocumentationGeneratorPropertyData
    {
        /// <summary>
        /// Gets or sets the parent <see cref="Type"/>
        /// </summary>
        public Type? Type { get; set; }

        /// <summary>
        /// Gets or sets the parent <see cref="CodeGenClassAttribute"/>.
        /// </summary>
        public CodeGenClassAttribute? Class { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo? Property { get; set; }

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CodeGenPropertyAttribute"/>.
        /// </summary>
        public CodeGenPropertyAttribute? Psa { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CodeGenPropertyCollectionAttribute"/>.
        /// </summary>
        public CodeGenPropertyCollectionAttribute? Pcsa { get; set; }
    }
}