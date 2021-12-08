// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OnRamp.Utility
{
    /// <summary>
    /// <see cref="Stream"/> locator/manager.
    /// </summary>
    public static class StreamLocator
    {
        /// <summary>
        /// Gets the list of standard YAML and JSON extensions. 
        /// </summary>
        public static readonly string[] YamlJsonExtensions = new string[] { "yaml", "yml", "json", "jsn" };

        /// <summary>
        /// Gets the list of standard <i>Handlebars</i> extensions.
        /// </summary>
        public static readonly string[] HandlebarsExtensions = new string[] { "hb", "hbs" };

        /// <summary>
        /// Gets the <b>Resource</b> content from the file system and then <c>Resources</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence).</param>
        /// <param name="extensions">The file extensions to also probe for.</param>
        /// <returns>The resource <see cref="StreamReader"/> where found and filename; otherwise, <c>null</c>.</returns>
        public static (StreamReader? StreamReader, string FileName) GetResourcesStreamReader(string fileName, Assembly[]? assemblies = null, string[]? extensions = null) => GetStreamReader(fileName, "Resources", assemblies, extensions);

        /// <summary>
        /// Gets the <b>Script</b> content from the file system and then <c>Scripts</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence).</param>
        /// <param name="extensions">The file extensions to also probe for.</param>
        /// <returns>The resource <see cref="StreamReader"/> where found and filename; otherwise, <c>null</c>.</returns>
        public static (StreamReader? StreamReader, string FileName) GetScriptStreamReader(string fileName, Assembly[]? assemblies = null, string[]? extensions = null) => GetStreamReader(fileName, "Scripts", assemblies, extensions);

        /// <summary>
        /// Gets the <b>Template</b> content from the file system and then <c>Templates</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence).</param>
        /// <param name="extensions">The file extensions to also probe for.</param>
        /// <returns>The resource <see cref="StreamReader"/> where found and filename; otherwise, <c>null</c>.</returns>
        public static (StreamReader? StreamReader, string FileName) GetTemplateStreamReader(string fileName, Assembly[]? assemblies = null, string[]? extensions = null) => GetStreamReader(fileName, "Templates", assemblies, extensions);

        /// <summary>
        /// Gets the specified content from the file system and then <paramref name="contentType"/> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="contentType">The optional content type name.</param>
        /// <param name="assemblies">The assemblies to use to probe for the assembly resource (in defined sequence).</param>
        /// <param name="extensions">The file extensions to also probe for.</param>
        /// <returns>The resource <see cref="StreamReader"/> where found and file name; otherwise, <c>null</c>.</returns>
        public static (StreamReader? StreamReader, string FileName) GetStreamReader(string fileName, string? contentType = null, Assembly[]? assemblies = null, string[]? extensions = null)
        {
            var sr = GetStreamReaderInternal(fileName, contentType, assemblies);
            if (sr != null || extensions == null)
                return (sr, fileName);

            foreach (var ext in extensions)
            {
                var fn = $"{fileName}.{ext}";
                sr = GetStreamReaderInternal(fn, contentType, assemblies);
                if (sr != null)
                    return (sr, fn);
            }

            return (null, fileName);
        }

        /// <summary>
        /// Gets the specified content from the file system and then <paramref name="contentType"/> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        private static StreamReader? GetStreamReaderInternal(string fileName, string? contentType = null, Assembly[]? assemblies = null)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var fi = new FileInfo(fileName);
            if (fi.Exists)
                return new StreamReader(fi.FullName);

            if (!string.IsNullOrEmpty(contentType))
            {
                fi = new FileInfo(Path.Combine(fi.DirectoryName, contentType, fi.Name));
                if (fi.Exists)
                    return new StreamReader(fi.FullName);

                if (assemblies != null)
                {
                    var frn = ConvertFileNameToResourceName(fileName);
                    var ew = string.IsNullOrEmpty(contentType) ? ".{frn}" : $".{contentType}.{frn}";
                    foreach (var ass in new List<Assembly>(assemblies) { typeof(StreamLocator).Assembly })
                    {
                        var rn = ass.GetManifestResourceNames().Where(x => x.EndsWith(ew, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (rn != null)
                        {
                            var ri = ass.GetManifestResourceInfo(rn);
                            if (ri != null)
                                return new StreamReader(ass.GetManifestResourceStream(rn)!);
                        }
                    }
                }
            }

            return null!;
        }

        /// <summary>
        /// Indicates whether the specified <b>Resource</b> content exists within the file system and then <c>Resources</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence).</param>
        /// <param name="extensions">The file extensions to also probe for.</param>
        /// <returns><c>true</c> indicates that the <see cref="Stream"/> exists; otherwise, <c>false</c>. Also returns the file name.</returns>
        public static (bool Exists, string FileName) HasResourceStream(string fileName, Assembly[]? assemblies = null, string[]? extensions = null) => HasStream(fileName, "Resources", assemblies, extensions);

        /// <summary>
        /// Indicates whether the specified <b>Script</b> content exists within the file system and then <c>Scripts</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence).</param>
        /// <param name="extensions">The file extensions to also probe for.</param>
        /// <returns><c>true</c> indicates that the <see cref="Stream"/> exists; otherwise, <c>false</c>. Also returns the file name.</returns>
        public static (bool Exists, string FileName) HasScriptStream(string fileName, Assembly[]? assemblies = null, string[]? extensions = null) => HasStream(fileName, "Scripts", assemblies, extensions);

        /// <summary>
        /// Indicates whether the specified <b>Template</b> content exists within the file system and then <c>Templates</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence).</param>
        /// <param name="extensions">The file extensions to also probe for.</param>
        /// <returns><c>true</c> indicates that the <see cref="Stream"/> exists; otherwise, <c>false</c>. Also returns the file name.</returns>
        public static (bool Exists, string FileName) HasTemplateStream(string fileName, Assembly[]? assemblies = null, string[]? extensions = null) => HasStream(fileName, "Templates", assemblies, extensions);

        /// <summary>
        /// Indicates whether the specified resource content exists within the file system or the <paramref name="contentType"/> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="contentType">The optional content type name.</param>
        /// <param name="assemblies">The assemblies to use to probe for the assembly resource (in defined sequence).</param>
        /// <param name="extensions">The file extensions to also probe for.</param>
        /// <returns><c>true</c> indicates that the <see cref="Stream"/> exists; otherwise, <c>false</c>. Also returns the file name.</returns>
        public static (bool Exists, string FileName) HasStream(string fileName, string? contentType = null, Assembly[]? assemblies = null, string[]? extensions = null)
        {
            var exists = HasStreamInternal(fileName, contentType, assemblies);
            if (exists || extensions == null)
                return (exists, fileName);

            foreach (var ext in extensions)
            {
                var fn = $"{fileName}.{ext}";
                exists = HasStreamInternal(fn, contentType, assemblies);
                if (exists)
                    return (exists, fn);
            }

            return (exists, fileName);
        }

        /// <summary>
        /// Indicates whether the specified resource content exists within the file system or the <paramref name="contentType"/> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        private static bool HasStreamInternal(string fileName, string? contentType, Assembly[]? assemblies)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var fi = new FileInfo(fileName);
            if (fi.Exists)
                return true;

            if (!string.IsNullOrEmpty(contentType))
            {
                fi = new FileInfo(Path.Combine(fi.DirectoryName, contentType, fileName));
                if (fi.Exists)
                    return true;

                if (assemblies != null)
                {
                    var frn = ConvertFileNameToResourceName(fileName);
                    var ew = $".{contentType}.{frn}";
                    foreach (var ass in new List<Assembly>(assemblies) { typeof(StreamLocator).Assembly })
                    {
                        var rn = ass.GetManifestResourceNames().Where(x => x.EndsWith(ew, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (rn != null)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Replaces path characters to dot as per resource file notation.
        /// </summary>
        private static string ConvertFileNameToResourceName(string filename) => filename.Replace('/', '.').Replace('\\', '.');

        /// <summary>
        /// Gets (determines) the <see cref="StreamContentType"/> from the <paramref name="fileName"/> extension.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The corresponding <see cref="StreamContentType"/>.</returns>
        public static StreamContentType GetContentType(string fileName) => new FileInfo(fileName).Extension.ToLowerInvariant() switch
        {
            ".yaml" => StreamContentType.Yaml,
            ".yml" => StreamContentType.Yaml,
            ".json" => StreamContentType.Json,
            ".jsn" => StreamContentType.Json,
            _ => StreamContentType.Unknown
        };
    }
}