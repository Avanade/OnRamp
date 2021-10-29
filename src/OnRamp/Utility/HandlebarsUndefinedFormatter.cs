// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using HandlebarsDotNet;
using HandlebarsDotNet.IO;
using System;

namespace OnRamp.Utility
{
    internal sealed class HandlebarsUndefinedFormatter : IFormatter, IFormatterProvider
    {
        public bool TryCreateFormatter(Type type, out IFormatter formatter)
        {
            if (type != typeof(UndefinedBindingResult))
            {
                formatter = null!;
                return false;
            }

            formatter = this;
            return true;
        }

        /// <summary>
        /// Formats the value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="writer"></param>
        public void Format<T>(T value, in EncodedTextWriter writer) => throw new CodeGenException($"Handlebars template references '{(value as UndefinedBindingResult)!.Value}' which is undefined.");
    }
}