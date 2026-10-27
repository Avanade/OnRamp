// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

namespace OnRamp.Config
{
    /// <summary>
    /// Defines the non-null JSON scalar types supported by reference generation.
    /// </summary>
    public enum CodeGenJsonType
    {
        /// <summary>A JSON string.</summary>
        String,

        /// <summary>A JSON number, including integers.</summary>
        Number,

        /// <summary>A JSON integer.</summary>
        Integer,

        /// <summary>A JSON Boolean.</summary>
        Boolean
    }
}
