// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnRamp.Config
{
    /// <summary>
    /// Provides base configuration capabilities.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class ConfigBase
    {
        #region static

        /// <summary>
        /// Check whether the nullable <see cref="bool"/> is <c>true</c>.
        /// </summary>
        public static bool IsTrue(bool? value) => value.HasValue && value.Value;

        /// <summary>
        /// Check whether the nullable <see cref="bool"/> is <c>null</c> or <c>false</c>.
        /// </summary>
        public static bool IsFalse(bool? value) => !value.HasValue || !value.Value;

        /// <summary>
        /// Defaults the <see cref="string"/> <paramref name="value"/> where <c>null</c> using the <paramref name="defaultValue"/> function.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value function.</param>
        public static string? DefaultWhereNull(string? value, Func<string?> defaultValue) => value ?? (defaultValue ?? throw new ArgumentNullException(nameof(defaultValue)))();

        /// <summary>
        /// Defaults the <see cref="int"/> <paramref name="value"/> where <c>null</c> using the <paramref name="defaultValue"/> function.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value function.</param>
        public static int? DefaultWhereNull(int? value, Func<int?> defaultValue) => value ?? (defaultValue ?? throw new ArgumentNullException(nameof(defaultValue)))();

        /// <summary>
        /// Defaults the <see cref="decimal"/> <paramref name="value"/> where <c>null</c> using the <paramref name="defaultValue"/> function.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value function.</param>
        public static decimal? DefaultWhereNull(decimal? value, Func<decimal?> defaultValue) => value ?? (defaultValue ?? throw new ArgumentNullException(nameof(defaultValue)))();

        /// <summary>
        /// Defaults the <see cref="bool"/> <paramref name="value"/> where <c>null</c> using the <paramref name="defaultValue"/> function.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value function.</param>
        public static bool? DefaultWhereNull(bool? value, Func<bool?> defaultValue) => value.HasValue ? value : (defaultValue ?? throw new ArgumentNullException(nameof(defaultValue)))();

        /// <summary>
        /// Compares the <see cref="string"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality, or whether <paramref name="propertyValue"/> is <c>null</c>.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal or <paramref name="propertyValue"/> is <c>null</c>; otherwise, <c>false</c>.</returns>
        public static bool CompareNullOrValue(string? propertyValue, string compareTo) => propertyValue == null || propertyValue == compareTo;

        /// <summary>
        /// Compares the <see cref="int"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality, or whether <paramref name="propertyValue"/> is <c>null</c>.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal or <paramref name="propertyValue"/> is <c>null</c>; otherwise, <c>false</c>.</returns>
        public static bool CompareNullOrValue(int? propertyValue, int compareTo) => propertyValue == null || propertyValue == compareTo;

        /// <summary>
        /// Compares the <see cref="decimal"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality, or whether <paramref name="propertyValue"/> is <c>null</c>.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal or <paramref name="propertyValue"/> is <c>null</c>; otherwise, <c>false</c>.</returns>
        public static bool CompareNullOrValue(decimal? propertyValue, decimal compareTo) => propertyValue == null || propertyValue == compareTo;

        /// <summary>
        /// Compares the <see cref="bool"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality, or whether <paramref name="propertyValue"/> is <c>null</c>.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal or <paramref name="propertyValue"/> is <c>null</c>; otherwise, <c>false</c>.</returns>
        public static bool CompareNullOrValue(bool? propertyValue, bool compareTo) => propertyValue == null || propertyValue == compareTo;

        /// <summary>
        /// Compares the <see cref="string"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal; otherwise, <c>false</c>.</returns>
        public static bool CompareValue(string? propertyValue, string compareTo) => propertyValue != null && propertyValue == compareTo;

        /// <summary>
        /// Compares the <see cref="bool"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal; otherwise, <c>false</c>.</returns>
        public static bool CompareValue(bool? propertyValue, bool compareTo) => propertyValue != null && propertyValue == compareTo;

        /// <summary>
        /// Build the standardized qualified key name.
        /// </summary>
        /// <param name="configName">The <see cref="ConfigBase"/> name.</param>
        /// <param name="keyValue">The key value where applicable.</param>
        /// <param name="keyName">The corresponding key name; defaults to 'Name'.</param>
        /// <returns></returns>
        protected static string BuildQualifiedKeyName(string configName, string? keyValue = null, string keyName = "Name")
            => $"{configName}{(string.IsNullOrEmpty(keyValue) ? "" : $"({keyName}='{(string.IsNullOrEmpty(keyValue) ? "<not specified>" : keyValue)}')")}";

        #endregion

        /// <summary>
        /// Gets the <b>Root</b> configuration.
        /// </summary>
        protected internal ConfigBase? RootConfig { get; set; }

        /// <summary>
        /// Gets the <b>Parent</b> configuration.
        /// </summary>
        protected internal ConfigBase? ParentConfig { get; set; }

        /// <summary>
        /// Gets the root <see cref="Type"/>.
        /// </summary>
        internal abstract Type RootType { get; }

        /// <summary>
        /// Gets the qualified key name for the configuration.
        /// </summary>
        /// <remarks>Used in error messages to assist navigation within configuration.</remarks>
        public virtual string? QualifiedKeyName { get; }

        /// <summary>
        /// Build the fully qualified name for the specified <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The fully qualified name.</returns>
        public string? BuildFullyQualifiedName(string? propertyName)
        {
            var hier = new List<string?>();
            if (propertyName != null)
                hier.Add(propertyName);

            ConfigBase? cb = this;
            while (true)
            {
                hier.Add(cb.QualifiedKeyName);
                if (cb.ParentConfig == null || cb == cb.ParentConfig)
                    break;

                cb = cb.ParentConfig;
            }

            var sb = new StringBuilder();
            foreach (var qn in hier.Reverse<string?>())
            {
                if (!string.IsNullOrEmpty(qn))
                {
                    if (sb.Length > 0)
                        sb.Append('.');

                    sb.Append(qn);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether the <paramref name="type"/> is a subclass of the <paramref name="baseType"/>.
        /// </summary>
        /// <param name="baseType">The base generic <see cref="Type"/>.</param>
        /// <param name="type">The <see cref="Type"/> that is being verifed as being a subclass (inherits from).</param>
        /// <returns><c>true</c> where is a valid subclass; otherwise, <c>false</c>.</returns>
        public static bool IsSubclassOfBaseType(Type baseType, Type type)
        { 
            if (baseType == null)
                throw new ArgumentNullException(nameof(baseType));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            while (type != null && type != typeof(object))
            {
                if (baseType.IsGenericType)
                {
                    var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                    if (baseType == genericType)
                        return true;
                }
                else if (baseType == type)
                    return true;

                type = type.BaseType!;
            }

            return false;
        }

        /// <summary>
        /// Prepares the configuration properties in advance of the code-generation execution (Internal use!).
        /// </summary>
        /// <param name="root">The root <see cref="ConfigBase"/>.</param>
        /// <param name="parent">The parent <see cref="ConfigBase"/>.</param>
        internal abstract Task PrepareAsync(object root, object parent);

        /// <summary>
        /// Gets or sets the <see cref="Dictionary{TKey, TValue}"/> that houses any additional/extra properties/attributes deserialized within the configuration.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JToken>? ExtraProperties { get; set; }

        /// <summary>
        /// Gets the property value from <see cref="ExtraProperties"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value where the property is not found.</param>
        /// <returns>The value.</returns>
        public T GetExtraProperty<T>(string key, T defaultValue = default!) where T : JToken
        {
            if (ExtraProperties != null && ExtraProperties.TryGetValue(key, out var val))
                return (T)Convert.ChangeType(val, typeof(T));
            else
                return defaultValue!;
        }

        /// <summary>
        /// Trys to get the property value from <see cref="ExtraProperties"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The corresponding value.</param>
        /// <returns><c>true</c> if the <paramref name="key"/> is found; otherwise, <c>false</c>.</returns>
        public bool TryGetExtraProperty<T>(string key, out T value) where T : JToken
        {
            if (ExtraProperties != null && ExtraProperties.TryGetValue(key, out var val))
            {
                value = (T)Convert.ChangeType(val, typeof(T));
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="Dictionary{TKey, TValue}"/> that allows for custom property values to be manipulated at runtime.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, object> CustomProperties { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the property value from <see cref="CustomProperties"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value where the property is not found.</param>
        /// <returns>The value.</returns>
        public T GetCustomProperty<T>(string key, T defaultValue = default!)
        {
            if (CustomProperties != null && CustomProperties.TryGetValue(key, out var val))
                return (T)Convert.ChangeType(val, typeof(T));
            else
                return defaultValue!;
        }

        /// <summary>
        /// Trys to get the property value from <see cref="CustomProperties"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The corresponding value.</param>
        /// <returns><c>true</c> if the <paramref name="key"/> is found; otherwise, <c>false</c>.</returns>
        public bool TryGetCustomProperty<T>(string key, out T value)
        {
            if (CustomProperties != null && CustomProperties.TryGetValue(key, out var val))
            {
                value = (T)Convert.ChangeType(val, typeof(T));
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }
    }
}