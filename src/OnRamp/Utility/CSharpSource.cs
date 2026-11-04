// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;
using System.Globalization;
using System.Text;

namespace OnRamp.Utility
{
    /// <summary>
    /// Provides C# source emission utilities.
    /// </summary>
    public static class CSharpSource
    {
        /// <summary>
        /// Returns a complete, quoted regular C# string literal preserving every UTF-16 code unit.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>A deterministic, ASCII-only C# string literal.</returns>
        /// <remarks>Non-ASCII characters, including each surrogate code unit, use fixed-width Unicode escapes.
        /// No normalization or replacement of malformed surrogate sequences is performed.</remarks>
        /// <exception cref="ArgumentNullException">The value is null.</exception>
        public static string StringLiteral(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var result = new StringBuilder();
            result.Append('"');
            foreach (var character in value)
            {
                switch (character)
                {
                    case '"': result.Append("\\\""); break;
                    case '\\': result.Append("\\\\"); break;
                    case '\0': result.Append("\\0"); break;
                    case '\a': result.Append("\\a"); break;
                    case '\b': result.Append("\\b"); break;
                    case '\f': result.Append("\\f"); break;
                    case '\n': result.Append("\\n"); break;
                    case '\r': result.Append("\\r"); break;
                    case '\t': result.Append("\\t"); break;
                    case '\v': result.Append("\\v"); break;
                    default:
                        if (character < ' ' || character > '~')
                            result.Append("\\u").Append(((int)character).ToString("X4", CultureInfo.InvariantCulture));
                        else
                            result.Append(character);

                        break;
                }
            }

            return result.Append('"').ToString();
        }
    }
}
