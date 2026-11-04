// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using OnRamp.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnRamp.Utility
{
    /// <summary>
    /// Shares metadata interpretation between schema and documentation generation.
    /// </summary>
    internal static class ReferenceGeneration
    {
        /// <summary>
        /// Holds validated reference-generation metadata for a property.
        /// </summary>
        internal sealed class Property
        {
            /// <summary>
            /// Gets or sets the property shape.
            /// </summary>
            public Shape Shape { get; set; }

            /// <summary>
            /// Gets or sets the referenced configuration type, or null for scalar values.
            /// </summary>
            public Type? ChildType { get; set; }

            /// <summary>
            /// Gets or sets the normalized JSON scalar types for the value or collection items.
            /// </summary>
            public string[]? JsonTypes { get; set; }

            /// <summary>
            /// Gets or sets the serialized declared default, or null when no default is declared.
            /// </summary>
            public string? DefaultJson { get; set; }
        }

        /// <summary>
        /// Describes the configuration property shape.
        /// </summary>
        internal enum Shape
        {
            Scalar,
            Sequence,
            Map
        }

        /// <summary>
        /// Discovers configuration types in stable order and validates their metadata and unique names.
        /// </summary>
        internal static List<Type> FindTypes(Type root)
        {
            var types = new List<Type>();
            var visited = new HashSet<Type>();
            var names = new Dictionary<string, Type>(StringComparer.Ordinal);
            Visit(root);
            return types;

            void Visit(Type type)
            {
                if (!visited.Add(type))
                    return;

                var name = GetClass(type).Name;
                if (names.TryGetValue(name, out var other))
                    throw new InvalidOperationException($"Types '{other.FullName}' and '{type.FullName}' have the same CodeGenClass.Name '{name}'.");

                names.Add(name, type);
                types.Add(type);
                foreach (var pi in type.GetProperties().Where(p => p.GetCustomAttribute<JsonPropertyNameAttribute>() != null))
                {
                    var property = GetProperty(pi, pi.GetCustomAttribute<CodeGenPropertyAttribute>(), pi.GetCustomAttribute<CodeGenPropertyCollectionAttribute>(), pi.GetCustomAttribute<CodeGenPropertyMapAttribute>());
                    if (property.ChildType != null)
                        Visit(property.ChildType);
                }
            }
        }

        /// <summary>
        /// Gets the required configuration class metadata.
        /// </summary>
        internal static CodeGenClassAttribute GetClass(Type type) =>
            type.GetCustomAttribute<CodeGenClassAttribute>() ?? throw new InvalidOperationException($"Type '{type.FullName}' does not have required CodeGenClassAttribute metadata.");

        /// <summary>
        /// Validates and interprets property metadata, including scalar declarations and serialized defaults.
        /// </summary>
        internal static Property GetProperty(PropertyInfo pi, CodeGenPropertyAttribute? scalar, CodeGenPropertyCollectionAttribute? collection, CodeGenPropertyMapAttribute? map = null)
        {
            var shapeCount = (scalar == null ? 0 : 1) + (collection == null ? 0 : 1) + (map == null ? 0 : 1);
            if (shapeCount == 0)
                throw Error(pi, "does not have a required CodeGenPropertyAttribute, CodeGenPropertyCollectionAttribute, or CodeGenPropertyMapAttribute.");
            if (shapeCount > 1)
                throw Error(pi, "has conflicting property shape attributes; specify exactly one of CodeGenPropertyAttribute, CodeGenPropertyCollectionAttribute, or CodeGenPropertyMapAttribute.");

            var shape = map != null ? Shape.Map : collection != null ? Shape.Sequence : Shape.Scalar;
            var type = shape switch
            {
                Shape.Map => GetMapValueType(pi),
                Shape.Sequence => GetItemType(pi),
                _ => pi.PropertyType
            };
            var property = new Property { Shape = shape };

            if (shape == Shape.Map)
            {
                if (!typeof(ConfigBase).IsAssignableFrom(type))
                    throw Error(pi, $"has unsupported map value type '{type.Name}'; map values must derive from ConfigBase.");

                property.ChildType = type;
            }
            else
            {
                var declared = shape == Shape.Sequence ? collection!.ItemJsonTypes : scalar!.JsonTypes;
                if (typeof(ConfigBase).IsAssignableFrom(type))
                {
                    if (declared != null)
                        throw Error(pi, "cannot declare scalar JSON types for configuration objects or items.");

                    property.ChildType = type;
                }
                else if (declared != null)
                    property.JsonTypes = GetDeclaredTypes(pi, type, declared);
                else if (shape == Shape.Sequence && type == typeof(string))
                    property.JsonTypes = ["string"];
                else if (shape == Shape.Sequence)
                    throw Error(pi, $"has unsupported collection item type '{type.Name}'; scalar items require explicit ItemJsonTypes.");
                else
                    property.JsonTypes = [GetInferredType(pi)];
            }

            var defaultValue = pi.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValue != null)
            {
                try
                {
                    property.DefaultJson = System.Text.Json.JsonSerializer.Serialize(defaultValue.Value, defaultValue.Value?.GetType() ?? typeof(object));
                }
                catch (Exception ex) when (ex is JsonException or NotSupportedException or ArgumentException or InvalidOperationException)
                {
                    throw new InvalidOperationException($"Type '{pi.DeclaringType?.Name}' Property '{pi.Name}' has a default that cannot be serialized as JSON.", ex);
                }
            }

            return property;
        }

        /// <summary>
        /// Gets the value type from a supported Dictionary&lt;string, T&gt; property.
        /// </summary>
        private static Type GetMapValueType(PropertyInfo pi)
        {
            var type = pi.PropertyType;
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Dictionary<,>))
                throw Error(pi, $"must be a Dictionary<string, TConfig>; type '{type.Name}' is not supported as a map.");

            var arguments = type.GetGenericArguments();
            if (arguments[0] != typeof(string))
                throw Error(pi, $"has unsupported map key type '{arguments[0].Name}'; map keys must be strings.");

            return arguments[1];
        }

        /// <summary>
        /// Gets the collection item type with property-specific error context.
        /// </summary>
        private static Type GetItemType(PropertyInfo pi)
        {
            try
            {
                return JsonSchemaGenerator.GetItemType(pi.PropertyType);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Type '{pi.DeclaringType?.Name}' Property '{pi.Name}' must be an Array or ICollection<T>.", ex);
            }
        }

        /// <summary>
        /// Validates CLR compatibility and normalizes explicit JSON scalar declarations.
        /// </summary>
        private static string[] GetDeclaredTypes(PropertyInfo pi, Type type, CodeGenJsonType[] declared)
        {
            if (declared.Length == 0)
                throw Error(pi, "has an empty JSON scalar type declaration.");

            type = Nullable.GetUnderlyingType(type) ?? type;
            foreach (var value in declared)
            {
                if (!Enum.IsDefined(typeof(CodeGenJsonType), value))
                    throw Error(pi, $"has undefined JSON scalar type '{(int)value}'.");

                var compatible = type == typeof(JsonElement)
                    || (type == typeof(string) && value == CodeGenJsonType.String)
                    || (type == typeof(bool) && value == CodeGenJsonType.Boolean)
                    || (IsIntegral(type) && value is CodeGenJsonType.Integer or CodeGenJsonType.Number)
                    || ((type == typeof(decimal) || type == typeof(float) || type == typeof(double)) && value == CodeGenJsonType.Number);
                if (!compatible)
                    throw Error(pi, $"declares JSON scalar type '{value}' incompatible with CLR type '{type.Name}'.");
            }

            return declared.Distinct().OrderBy(x => x).Select(x => x.ToString().ToLowerInvariant()).ToArray();
        }

        /// <summary>
        /// Determines whether the CLR type is a supported integral numeric type.
        /// </summary>
        private static bool IsIntegral(Type type) =>
            type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort)
            || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong);

        /// <summary>
        /// Gets the existing implicit JSON scalar mapping for the property.
        /// </summary>
        private static string GetInferredType(PropertyInfo pi) => pi.PropertyType switch
        {
            Type t when t == typeof(string) => "string",
            Type t when t == typeof(bool?) => "boolean",
            Type t when t == typeof(int?) => "integer",
            Type t when t == typeof(long) || t == typeof(long?) => "integer",
            Type t when t == typeof(decimal?) => "number",
            _ => throw Error(pi, $"has a Type '{pi.PropertyType.Name}' that is not supported without explicit JsonTypes metadata.")
        };

        /// <summary>
        /// Creates a metadata error identifying the declaring type and property.
        /// </summary>
        private static InvalidOperationException Error(PropertyInfo pi, string message) =>
            new($"Type '{pi.DeclaringType?.Name}' Property '{pi.Name}' {message}");

        /// <summary>
        /// Creates a local schema reference with JSON Pointer and URI fragment escaping.
        /// </summary>
        internal static string GetReference(Type type) =>
            $"#/definitions/{Uri.EscapeDataString(GetClass(type).Name.Replace("~", "~0").Replace("/", "~1"))}";
    }
}
