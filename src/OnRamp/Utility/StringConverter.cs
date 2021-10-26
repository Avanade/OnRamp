// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using Pluralize.NET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OnRamp.Utility
{
    /// <summary>
    /// Provides special case string conversions.
    /// </summary>
    public static class StringConverter
    {
        /// <summary>
        /// The <see cref="Regex"/> expression pattern for splitting strings into words.
        /// </summary>
        public const string WordSplitPattern = "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))";

        /// <summary>
        /// Converts <paramref name="text"/> to camelCase (e.g. 'SomeValue' would return 'someValue').
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToCamelCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (!ignoreSpecialNames && TwoCharacterPrefixes.Any(x => text.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                return $"{char.ToLower(text[0], CultureInfo.InvariantCulture)}{char.ToLower(text[1], CultureInfo.InvariantCulture)}{text[2..]}";
            else
                return char.ToLower(text[0], CultureInfo.InvariantCulture) + text[1..];
        }

        /// <summary>
        /// Converts <paramref name="text"/> to _camelCase (e.g. 'SomeValue' would return '_someValue').
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToPrivateCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return "_" + ToCamelCase(text, ignoreSpecialNames);
        }

        /// <summary>
        /// Converts <paramref name="text"/> to PascalCase (e.g. 'someValue' would return 'SomeValue').
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToPascalCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (!ignoreSpecialNames && TwoCharacterPrefixes.Any(x => text.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                return $"{char.ToUpper(text[0], CultureInfo.InvariantCulture)}{char.ToUpper(text[1], CultureInfo.InvariantCulture)}{text[2..]}";
            else
                return char.ToUpper(text[0], CultureInfo.InvariantCulture) + text[1..];
        }

        /// <summary>
        /// Converts <paramref name="text"/> to a Sentence Case ('someValueXML' would return 'Some Value XML'); splits on capitals and attempts to keep acronyms.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToSentenceCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = Regex.Replace(text, WordSplitPattern, "$1 "); // Split the string into words.
            if (!ignoreSpecialNames)
                s = SpecialCaseHandling(s);

            return char.ToUpper(s[0], CultureInfo.InvariantCulture) + s[1..]; // Make sure the first character is always upper case.
        }

        /// <summary>
        /// Converts <paramref name="text"/> to a Snake Case ('someValueXML' would return 'some_value_xml'); splits on capitals and attempts to keep acronyms.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToSnakeCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = Regex.Replace(text, WordSplitPattern, "$1 "); // Split the string into words.
            if (!ignoreSpecialNames)
                s = SpecialCaseHandling(s);

            return s.Replace(" ", "_", StringComparison.InvariantCulture).ToLowerInvariant(); // Replace space with _ and make lowercase.
        }

        /// <summary>
        /// Converts <paramref name="text"/> to a Kebab Case ('someValueXML' would return 'some-value-xml'); splits on capitals and attempts to keep acronyms.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ignoreSpecialNames">Indicates whether to ignore specific handling of special names.</param>
        /// <returns>The converted text.</returns>
        public static string? ToKebabCase(string? text, bool ignoreSpecialNames = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = Regex.Replace(text, WordSplitPattern, "$1 "); // Split the string into words.
            if (!ignoreSpecialNames)
                s = SpecialCaseHandling(s);

            return s.Replace(" ", "-", StringComparison.InvariantCulture).ToLowerInvariant(); // Replace space with - and make lowercase.
        }

        /// <summary>
        /// Gets or sets the special prefixes whereby the first two characters will be converted to lowercase versus the standard one.
        /// </summary>
        /// <remarks>Defaults to "ETag" and "OData".</remarks>
        public static string[] TwoCharacterPrefixes { get; set; } = new string[] { "ETag", "OData" };

        /// <summary>
        /// Performs the special case handling.
        /// </summary>
        private static string SpecialCaseHandling(string text) => SpecialCaseHandler == null ? text : SpecialCaseHandler(text);

        /// <summary>
        /// Get or sets the special case handler function that occurs directly after the <see cref="WordSplitPattern"/> <see cref="Regex"/> has been applied.
        /// </summary>
        /// <remarks>Defaults to replace "E Tag" with "ETag", and "O Data" with "OData".</remarks>
        public static Func<string, string>? SpecialCaseHandler { get; set; } =
            new Func<string, string>(text =>
            {
                if (string.IsNullOrEmpty(text))
                    return text;

                var s = text.Replace("E Tag", "ETag", StringComparison.InvariantCulture); 
                return s.Replace("O Data", "OData", StringComparison.InvariantCulture); 
            });

        /// <summary>
        /// Converts any '{{ }}' delimited sub-string with the <paramref name="text"/> to C# comments equivalent; e.g. 'See {{xyz}}.' would become 'See &lt;see cref="xyz"/&gt;.'. 
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        /// <remarks>Additionally, any '&lt;' or '&gt;' within the sub-string would become '{' or '}' respectively; e.g. '{{List&lt;int&gt;}} would become '&lt;see cref="List{int}"/&gt;'.</remarks>
        public static string? ToComments(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = text;
            while (true)
            {
                var start = s.IndexOf("{{", StringComparison.InvariantCultureIgnoreCase);
                var end = s.IndexOf("}}", StringComparison.InvariantCultureIgnoreCase);

                if (start < 0 && end < 0)
                    break;

                if (start < 0 || end < 0 || end < start)
                    throw new CodeGenException("Start and End {{ }} parameter mismatch.", text);

                var sub = s.Substring(start, end - start + 2);
                s = s.Replace(sub, ToSeeComments(sub[2..^2]));
            }

            return s;
        }

        /// <summary>
        /// Converts <paramref name="text"/> to C# comments equivalent; e.g. 'ABC' would become '&lt;see cref="ABC"/&gt;'. 
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        /// <remarks>Additionally, any '&lt;' or '&gt;' within the <paramref name="text"/> would become '{' or '}' respectively; e.g. 'List&lt;int&gt;' would become '&lt;see cref="List{int}"/&gt;'.</remarks>
        public static string? ToSeeComments(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return $"<see cref=\"{ReplaceGenericsBracketWithCommentsBracket(text)}\"/>";
        }


        /// <summary>
        /// Converts <paramref name="text"/> to c# 'see cref=' Comments ('List&lt;int&gt;' would become 'List{int}' respectively). 
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        private static string? ReplaceGenericsBracketWithCommentsBracket(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = text.Replace("<", "{", StringComparison.InvariantCulture);
            s = s.Replace(">", "}", StringComparison.InvariantCulture);
            return s;
        }

        /// <summary>
        /// Gets the <see cref="ToPastTense(string?)"/> dictionary of special conversions.
        /// </summary>
        public static Dictionary<string, string> PastTenseDictionary { get; } = new Dictionary<string, string> { // List inspired by https://github.com/samuellawrentz/PastTenser/blob/master/data/pastdata.json
            { "send", "sent" }, { "sell", "sold" }, { "lead", "led" }, { "remake", "remade" }, { "put", "put" }, { "shoot", "shot" }, { "withhold", "withheld" }, { "withdraw", "withdrawn" }, { "show", "shown" },
            { "run", "ran" }, { "seek", "sought" }, { "drive", "driven" }, { "resend", "resent" }, { "input", "inputted" }, { "draw", "drawn" }, { "missend", "missent" }, { "resell", "resold" }, { "bet", "bet" },
            { "slide", "slid" }, { "lie", "lay" }, { "spell", "spelt" }, { "misspell", "misspelt" }, { "overlay", "overlain" }, { "tell", "told" }, { "teach", "taught" }, { "cut", "cut" }, { "fall", "fallen" },
            { "spin", "spun" }, { "throw", "thrown" }, { "overthrow", "overthrown" }, { "see", "seen" }, { "oversee", "overseen" }, { "hide", "hidden" }, { "hit", "hit" }, { "preset", "preset" }, { "build", "built" },
            { "say", "said" }, { "arise", "arisen" }, { "spend", "spent" }, { "overspend", "overspent" }, { "underspend", "underspent" }, { "blow", "blown" }, { "troubleshoot", "toubleshot" }, { "overdrew", "overdrawn" },
            { "offset", "offset" }, { "bind", "bound" }, { "overrun", "overran" }, { "keep", "kept" }, { "redo", "redone" }, { "win", "won" }, { "lose", "lost" }, { "lean", "leant" }, { "freeze", "frozen" },
            { "unfreeze", "unfrozen" }, { "read", "read" }, { "lay", "laid" }, { "come", "came" }, { "begin", "begun" } };

        /// <summary>
        /// Converts a text to past tense (English only).
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        /// <remarks>This only supports a number of basic rules and is not guaranteed to cover all scenarios.</remarks>
        public static string? ToPastTense(string? text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 3 || text.EndsWith("ed", StringComparison.InvariantCultureIgnoreCase))
                return text;

            // Check the past tense dictionary.
            if (PastTenseDictionary.TryGetValue(text.ToLowerInvariant(), out var pt))
                return char.IsUpper(text[0]) ? ToPascalCase(pt) : pt;

            // Ends with the letter e, then remove the final e and add the -ed suffix: tie->tied, like->liked, agree->agreed.
            if (text[^1] == 'e')
                return text + "d";

            // Ends with the letter y preceded by a consonant, then change the y to an i and add the -ed suffix: apply->applied, pry->pried, study->studied.
            if (text[^1] == 'y' && !IsVowel(text[^2]))
                return text[0..^1] + "ied";

            // Ends with the letters ic, then add the letter k followed by the -ed suffix: frolic->frolicked, picnic->picnicked.
            if (text[^2..] == "ic")
                return text + "ked";

            // Ends with a single consonant other than w or y preceded by a single vowel, then double the final consonant and add the -ed suffix: drop->dropped, admit->admitted, concur->concurred.
            if (!IsVowel(text[^1]) && text[^1] != 'w' && text[^1] != 'y' && text[^2] != 'e' && IsVowel(text[^2]) && !IsVowel(text[^3]))
                return text + text[^1] + "ed";

            // Add the -ed suffix.
            return text + "ed";
        }

        /// <summary>
        /// Determine whether the character is a vowel (English only).
        /// </summary>
        /// <returns><c>true</c> where the character is a vowel; otherwise, <c>false</c>.</returns>
        public static bool IsVowel(char c) => char.IsLetter(c) && (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u');

        /// <summary>
        /// Gets or sets the <see cref="IPluralize"/>; defaults to <see cref="Pluralizer"/>.
        /// </summary>
        public static IPluralize? Pluralizer { get; set; }

        /// <summary>
        /// Converts singularized <paramref name="text"/> to a plural.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        /// <remarks>This only supports a number of basic rules and is not guaranteed to cover all scenarios.</remarks>
        public static string ToPlural(string? text) => (Pluralizer ??= new Pluralizer()).Pluralize(text);

        /// <summary>
        /// Converts pluralized <paramref name="text"/> to a single.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        /// <remarks>This only supports a number of basic rules and is not guaranteed to cover all scenarios.</remarks>
        public static string ToSingle(string? text) => (Pluralizer ??= new Pluralizer()).Singularize(text);
    }
}