using OnRamp.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace OnRamp.Utility
{
    /// <summary>
    /// Provides customized JSON serialization.
    /// </summary>
    public static class JsonSerializer
    {
        /// <summary>
        /// Provides the default <see cref="JsonSerializerOptions"/> including <see cref="OptInJsonTypeInfoResolver"/>.
        /// </summary>
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = false,
            TypeInfoResolver = new OptInJsonTypeInfoResolver()
        };

        /// <summary>
        /// Support opt-in properties only (explicit <see cref="JsonPropertyNameAttribute"/>) where the class is marked with <see cref="CodeGenClassAttribute"/>; otherwise, all properties are included as per default behavior.
        /// </summary>
        public class OptInJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
        {
            /// <inheritdoc/>
            public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
            {
                var jti = base.GetTypeInfo(type, options);
                if (jti.Kind == JsonTypeInfoKind.Object && jti.Type.GetCustomAttributes(typeof(CodeGenClassAttribute), true)?.Length > 0)
                {
                    var props = new List<JsonPropertyInfo>(jti.Properties);
                    jti.Properties.Clear();

                    foreach (var prop in props)
                    {
                        if (prop.IsExtensionData || (prop.AttributeProvider is not null && prop.AttributeProvider.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true)?.Length > 0))
                            jti.Properties.Add(prop);
                    }
                }

                return jti;
            }
        }

        /// <summary>
        /// Deserializes the <paramref name="json"/> <see cref="Stream"/> into the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="json">The UTF8 JSON <see cref="Stream"/>.</param>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>The resulting deserialized value.</returns>
        public static object? Deserialize(Stream json, Type type) => System.Text.Json.JsonSerializer.Deserialize(json, type, Options);

        /// <summary>
        /// Deserializes the <paramref name="json"/> <see cref="TextReader"/> into the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="json">The UTF8 JSON <see cref="TextReader"/>.</param>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>The resulting deserialized value.</returns>
        public static object? Deserialize(TextReader json, Type type) => System.Text.Json.JsonSerializer.Deserialize(json.ReadToEnd(), type, Options);

        /// <summary>
        /// Deserializes the <paramref name="json"/> <see cref="string"/> into the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="json">The UTF8 JSON <see cref="string"/>.</param>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>The resulting deserialized value.</returns>
        public static object? Deserialize(string json, Type type) => System.Text.Json.JsonSerializer.Deserialize(json, type, Options);
    }
}