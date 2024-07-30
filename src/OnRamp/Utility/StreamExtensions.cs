// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace OnRamp.Utility
{
    /// <summary>
    /// Provides <see cref="TextReader"/> extension methods.
    /// </summary>
    public static class TextReaderExtensions
    {
        /// <summary>
        /// Create an instance of <typeparamref name="T"/> from the <paramref name="json"/> <see cref="TextReader"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="json">The JSON <see cref="TextReader"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static T? DeserializeJson<T>(this TextReader json) where T : class => (T?)DeserializeJson(json, typeof(T));

        /// <summary>
        /// Create an instance of <paramref name="type"/> from the <paramref name="json"/> <see cref="TextReader"/>.
        /// </summary>
        /// <param name="json">The JSON <see cref="TextReader"/>.</param>
        /// <param name="type">The value <see cref="Type"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static object? DeserializeJson(this TextReader json, Type type) => JsonSerializer.Deserialize(json ?? throw new ArgumentNullException(nameof(json)), type);

        /// <summary>
        /// Create an instance of <typeparamref name="T"/> from the <paramref name="yaml"/> <see cref="TextReader"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="yaml">The YAML <see cref="TextReader"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static T? DeserializeYaml<T>(this TextReader yaml) where T : class => (T?)DeserializeYaml(yaml, typeof(T));

        /// <summary>
        /// Create an instance of <paramref name="type"/> from the <paramref name="yaml"/> <see cref="TextReader"/>.
        /// </summary>
        /// <param name="yaml">The YAML <see cref="TextReader"/>.</param>
        /// <param name="type">The value <see cref="Type"/>.</param>
        /// <returns>The corresponding value.</returns>
        public static object? DeserializeYaml(this TextReader yaml, Type type)
        {
            var yml = new DeserializerBuilder().WithNodeTypeResolver(new YamlNodeTypeResolver()).Build().Deserialize(yaml);

#pragma warning disable IDE0063 // Use simple 'using' statement; cannot as need to be more explicit with managing the close and dispose.
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    sw.Write(new SerializerBuilder().JsonCompatible().Build().Serialize(yml!));
                    sw.Flush();

                    ms.Position = 0;
                    using var sr = new StreamReader(ms);
                    return DeserializeJson(sr, type);
                }
            }
#pragma warning restore IDE0063
        }

        /// <summary>
        /// Converts the <paramref name="yaml"/> <see cref="TextReader"/> content into a <see cref="JsonNode"/>.
        /// </summary>
        /// <param name="yaml">The YAML <see cref="TextReader"/>.</param>
        /// <returns>The <see cref="JsonNode"/>.</returns>
        public static JsonNode? YamlToJsonNode(this TextReader yaml)
        {
            var yml = new DeserializerBuilder().WithNodeTypeResolver(new YamlNodeTypeResolver()).Build().Deserialize(yaml);

#pragma warning disable IDE0063 // Use simple 'using' statement; cannot as need to be more explicit with managing the close and dispose.
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    sw.Write(new SerializerBuilder().JsonCompatible().Build().Serialize(yml!));
                    sw.Flush();

                    ms.Position = 0;
                    using var sr = new StreamReader(ms);
                    return sr.JsonToJsonNode();
                }
            }
#pragma warning restore IDE0063
        }

        /// <summary>
        /// Converts the <paramref name="json"/> <see cref="TextReader"/> content into a <see cref="JsonNode"/>.
        /// </summary>
        /// <param name="json">The YAML <see cref="TextReader"/>.</param>
        /// <returns>The <see cref="JsonNode"/>.</returns>
        public static JsonNode? JsonToJsonNode(this TextReader json) => JsonNode.Parse(json.ReadToEnd());

        private class YamlNodeTypeResolver : INodeTypeResolver
        {
            private static readonly string[] boolValues = ["true", "false"];

            /// <inheritdoc/>
            bool INodeTypeResolver.Resolve(NodeEvent? nodeEvent, ref Type currentType)
            {
                if (nodeEvent is Scalar scalar && scalar.Style == YamlDotNet.Core.ScalarStyle.Plain)
                {
                    if (decimal.TryParse(scalar.Value, out var dv))
                    {
                        if (scalar.Value.Length > 1 && scalar.Value.StartsWith('0')) // Valid JSON does not support a number that starts with a zero.
                            currentType = typeof(string);
                        else
                            currentType = dv == Math.Round(dv) ? typeof(long) : typeof(decimal);

                        return true;
                    }

                    if (boolValues.Contains(scalar.Value))
                    {
                        currentType = typeof(bool);
                        return true;
                    }
                }

                return false;
            }
        }
    }
}