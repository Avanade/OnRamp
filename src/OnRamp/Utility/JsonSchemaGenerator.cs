// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using OnRamp.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnRamp.Utility
{
    /// <summary>
    /// Represents a <c>JsonSchema</c> <see cref="Generate{T}(Stream, string?)">generator</see>.
    /// </summary>
    /// <remarks>See <see href="https://json-schema.org/"/> and <see href="https://www.schemastore.org/json/"/>. Traverses configuration objects and collections once per type, including cycles. Child references use draft-04-compatible <c>allOf</c> wrappers. Explicit scalar declarations and <see cref="System.ComponentModel.DefaultValueAttribute"/> affect references only; no configuration instances are constructed or prepared.</remarks>
    public static class JsonSchemaGenerator
    {
        /// <summary>
        /// Generates a <c>JsonSchema</c> from the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The root <see cref="Type"/> to derive the schema from.</typeparam>
        /// <param name="path">The file to be opened for writing.</param>
        /// <param name="title">The title override.</param>
        public static void Generate<T>(string path, string? title = null) where T : ConfigBase, IRootConfig
        {
            using var fs = File.Create(path ?? throw new ArgumentNullException(nameof(path)));
            Generate<T>(fs, title);
        }

        /// <summary>
        /// Generates a <c>JsonSchema</c> from the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The root <see cref="Type"/> to derive the schema from.</typeparam>
        /// <param name="stream">The <see cref="Stream"/>.</param>
        /// <param name="title">The title override.</param>
        public static void Generate<T>(Stream stream, string? title = null) where T : ConfigBase, IRootConfig
        {
            var type = typeof(T);
            var types = ReferenceGeneration.FindTypes(type);

            using var jtw = new Utf8JsonWriter(stream ?? throw new ArgumentNullException(nameof(stream)), new JsonWriterOptions { Indented = true });

            jtw.WriteStartObject();
            jtw.WritePropertyName("title");
            jtw.WriteStringValue(title ?? StringConverter.ToSentenceCase(type.Name));
            jtw.WritePropertyName("$schema");
            jtw.WriteStringValue("https://json-schema.org/draft-04/schema#");
            jtw.WritePropertyName("definitions");

            jtw.WriteStartObject();
            types.ForEach(t => WriteDefinition(t, jtw));
            jtw.WriteEndObject();

            jtw.WritePropertyName("allOf");
            jtw.WriteStartArray();
            jtw.WriteStartObject();
            jtw.WritePropertyName("$ref");
            jtw.WriteStringValue(ReferenceGeneration.GetReference(type));
            jtw.WriteEndObject();
            jtw.WriteEndArray();

            jtw.WriteEndObject();
        }

        /// <summary>
        /// Writes the object definition.
        /// </summary>
        private static void WriteDefinition(Type type, Utf8JsonWriter jtw)
        {
            var csa = ReferenceGeneration.GetClass(type);
            jtw.WritePropertyName(csa.Name);
            jtw.WriteStartObject();
            jtw.WritePropertyName("type");
            jtw.WriteStringValue("object");
            jtw.WritePropertyName("title");
            jtw.WriteStringValue(CleanString(csa.Title) ?? StringConverter.ToSentenceCase(csa.Name)!);
            if (csa.Description != null)
            {
                jtw.WritePropertyName("description");
                jtw.WriteStringValue(CleanString(csa.Description));
            }

            jtw.WritePropertyName("properties");
            jtw.WriteStartObject();

            var rqd = new List<string>();

            foreach (var pi in type.GetProperties())
            {
                var jpa = pi.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (jpa == null)
                    continue;

                var name = jpa.Name ?? StringConverter.ToCamelCase(pi.Name)!;
                jtw.WritePropertyName(name);
                jtw.WriteStartObject();

                var psa = pi.GetCustomAttribute<CodeGenPropertyAttribute>();
                var pcsa = pi.GetCustomAttribute<CodeGenPropertyCollectionAttribute>();
                var property = ReferenceGeneration.GetProperty(pi, psa, pcsa);
                if (psa?.IsMandatory == true || pcsa?.IsMandatory == true)
                    rqd.Add(name);

                if (psa != null)
                {
                    if (property.ChildType == null)
                        WriteJsonTypes(jtw, property.JsonTypes!);
                    jtw.WritePropertyName("title");
                    jtw.WriteStringValue(CleanString(psa.Title) ?? StringConverter.ToSentenceCase(name)!);
                    if (psa.Description != null)
                    {
                        jtw.WritePropertyName("description");
                        jtw.WriteStringValue(CleanString(psa.Description));
                    }

                    if (property.ChildType != null)
                    {
                        jtw.WritePropertyName("allOf");
                        jtw.WriteStartArray();
                        WriteReference(jtw, property.ChildType);
                        jtw.WriteEndArray();
                    }

                    if (psa.Options != null)
                    {
                        jtw.WritePropertyName("enum");
                        jtw.WriteStartArray();

                        foreach (var opt in psa.Options)
                            jtw.WriteStringValue(opt);

                        jtw.WriteEndArray();
                    }
                }
                else
                {
                    jtw.WritePropertyName("type");
                    jtw.WriteStartArray();
                    jtw.WriteStringValue("array");
                    jtw.WriteStringValue("null");
                    jtw.WriteEndArray();
                    if (property.DefaultJson == null)
                    {
                        jtw.WritePropertyName("default");
                        jtw.WriteStartArray();
                        jtw.WriteEndArray();
                    }
                    jtw.WritePropertyName("title");
                    jtw.WriteStringValue(CleanString(pcsa!.Title) ?? StringConverter.ToSentenceCase(name)!);
                    if (pcsa.Description != null)
                    {
                        jtw.WritePropertyName("description");
                        jtw.WriteStringValue(CleanString(pcsa.Description));
                    }

                    jtw.WritePropertyName("items");

                    if (property.ChildType == null)
                    {
                        jtw.WriteStartObject();
                        WriteJsonTypes(jtw, property.JsonTypes!);
                        jtw.WriteEndObject();
                    }
                    else
                        WriteReference(jtw, property.ChildType);
                }

                if (property.DefaultJson != null)
                {
                    jtw.WritePropertyName("default");
                    jtw.WriteRawValue(property.DefaultJson);
                }

                jtw.WriteEndObject();
            }

            jtw.WriteEndObject();

            if (rqd.Count > 0)
            {
                jtw.WritePropertyName("required");
                jtw.WriteStartArray();

                foreach (var name in rqd)
                    jtw.WriteStringValue(name);

                jtw.WriteEndArray();
            }

            jtw.WriteEndObject();
        }

        /// <summary>
        /// Writes a scalar type or union.
        /// </summary>
        private static void WriteJsonTypes(Utf8JsonWriter jtw, string[] types)
        {
            jtw.WritePropertyName("type");
            if (types.Length == 1)
                jtw.WriteStringValue(types[0]);
            else
            {
                jtw.WriteStartArray();
                foreach (var type in types)
                    jtw.WriteStringValue(type);
                jtw.WriteEndArray();
            }
        }

        /// <summary>
        /// Writes a configuration reference.
        /// </summary>
        private static void WriteReference(Utf8JsonWriter jtw, Type type)
        {
            jtw.WriteStartObject();
            jtw.WriteString("$ref", ReferenceGeneration.GetReference(type));
            jtw.WriteEndObject();
        }

        /// <summary>
        /// Gets the item type from an Array or ICollection{T}.
        /// </summary>
        internal static Type GetItemType(Type type)
        {
            if (type == typeof(string) || type.IsPrimitive || type.IsValueType)
                throw new InvalidOperationException($"Type {type.Name} must be an Array or ICollection<T>.");

            if (type.IsArray)
                return type.GetElementType() ?? throw new InvalidOperationException($"Type {type.Name} is an Array but the Element Type is unable to be determined.");

            var t = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>) ? type : type.GetInterfaces().FirstOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
            if (t != null)
                return t.GetGenericArguments()[0];

            throw new InvalidOperationException($"Type {type.Name} must be an Array or ICollection<T>.");
        }

        /// <summary>
        /// Cleans the string.
        /// </summary>
        private static string? CleanString(string? text) => text;
    }
}