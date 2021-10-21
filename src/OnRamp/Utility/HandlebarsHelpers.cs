// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using HandlebarsDotNet;
using Microsoft.Extensions.Logging;
using OnRamp.Config;
using OnRamp.Console;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace OnRamp.Utility
{
    /// <summary>
    /// Provides the <b>Handlebars.Net</b> <see cref="RegisterHelpers"/> capability.
    /// </summary>
    public static class HandlebarsHelpers
    {
        private static bool _areRegistered = false;

        /// <summary>
        /// Gets or sets the global logger used by the <c>log</c> helper.
        /// </summary>
        public static ILogger? Logger { get; set; }

        /// <summary>
        /// Registers all of the required Handlebars helpers.
        /// </summary>
        public static void RegisterHelpers()
        {
            if (_areRegistered)
                return;

            _areRegistered = true;

            // Will check that the first argument equals at least one of the subsequent arguments.
            Handlebars.RegisterHelper("ifeq", (writer, context, parameters, args) =>
            {
                if (IfEq(args))
                    context.Template(writer, parameters);
                else
                    context.Inverse(writer, parameters);
            });

            // Will check that the first argument does not equal any of the subsequent arguments.
            Handlebars.RegisterHelper("ifne", (writer, context, parameters, args) =>
            {
                if (IfEq(args))
                    context.Inverse(writer, parameters);
                else
                    context.Template(writer, parameters);
            });

            // Will check that the first argument is less than or equal to the subsequent arguments.
            Handlebars.RegisterHelper("ifle", (writer, context, parameters, args) =>
            {
                if (IfLe(args))
                    context.Template(writer, parameters);
                else
                    context.Inverse(writer, parameters);
            });

            // Will check that the first argument is greater than or equal to the subsequent arguments.
            Handlebars.RegisterHelper("ifge", (writer, context, parameters, args) =>
            {
                if (IfGe(args))
                    context.Template(writer, parameters);
                else
                    context.Inverse(writer, parameters);
            });

            // Will check that all of the arguments have a non-<c>null</c> value.
            Handlebars.RegisterHelper("ifval", (writer, context, parameters, args) =>
            {
                foreach (var arg in args)
                {
                    if (arg == null)
                    {
                        context.Inverse(writer, parameters);
                        return;
                    }
                }

                context.Template(writer, parameters);
            });

            // Will check that all of the arguments have a <c>null</c> value.
            Handlebars.RegisterHelper("ifnull", (writer, context, parameters, args) =>
            {
                foreach (var arg in args)
                {
                    if (arg != null)
                    {
                        context.Inverse(writer, parameters);
                        return;
                    }
                }

                context.Template(writer, parameters);
            });

            // Will check that any of the arguments have a <c>true</c> value where bool; otherwise, non-null value.
            Handlebars.RegisterHelper("ifor", (writer, context, parameters, args) =>
            {
                foreach (var arg in args)
                {
                    if (arg is bool opt)
                    {
                        if (opt)
                        {
                            context.Template(writer, parameters);
                            return;
                        }
                    }
                    else if (arg != null)
                    {
                        var opt2 = arg as bool?;
                        if (opt2 != null && !opt2.Value)
                            continue;

                        context.Template(writer, parameters);
                        return;
                    }
                }

                context.Inverse(writer, parameters);
            });

            // Logs using the String.Format.
            Handlebars.RegisterHelper("log-info", (writer, context, parameters) => (Logger ?? new ConsoleLogger()).LogInformation(FormatString(parameters)));
            Handlebars.RegisterHelper("log-warning", (writer, context, parameters) => (Logger ?? new ConsoleLogger()).LogWarning(FormatString(parameters)));
            Handlebars.RegisterHelper("log-error", (writer, context, parameters) => (Logger ?? new ConsoleLogger()).LogError(FormatString(parameters)));
            Handlebars.RegisterHelper("log-debug", (writer, context, parameters) => System.Diagnostics.Debug.WriteLine($"Handlebars > {FormatString(parameters)}"));

            // Logs using the String.Format to the debugger and then initiates a break in the debugger itself.
            Handlebars.RegisterHelper("debug", (writer, context, parameters) =>
            {
                System.Diagnostics.Debug.WriteLine($"Handlebars > {FormatString(parameters)}");
                System.Diagnostics.Debugger.Break();
            });

            // Converts a value to lowercase or uppercase.
            Handlebars.RegisterHelper("lower", (writer, context, parameters) => writer.WriteSafeString(parameters.FirstOrDefault()?.ToString()?.ToLowerInvariant() ?? ""));
            Handlebars.RegisterHelper("upper", (writer, context, parameters) => writer.WriteSafeString(parameters.FirstOrDefault()?.ToString()?.ToUpperInvariant() ?? ""));

            // NOTE: Any ending in 'x' are to explicitly ignore special names!!!

            // Converts a value to camelcase.
            Handlebars.RegisterHelper("camel", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToCamelCase(parameters.FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("camelx", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToCamelCase(parameters.FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to pascalcase.
            Handlebars.RegisterHelper("pascal", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToPascalCase(parameters.FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("pascalx", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToPascalCase(parameters.FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to private case.
            Handlebars.RegisterHelper("private", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToPrivateCase(parameters.FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("privatex", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToPrivateCase(parameters.FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to snake case.
            Handlebars.RegisterHelper("snake", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToSnakeCase(parameters.FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("snakex", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToSnakeCase(parameters.FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to kebab case.
            Handlebars.RegisterHelper("kebab", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToKebabCase(parameters.FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("kebabx", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToKebabCase(parameters.FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to sentence case.
            Handlebars.RegisterHelper("sentence", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToSentenceCase(parameters.FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("sentencex", (writer, context, parameters) => writer.WriteSafeString(StringConversion.ToSentenceCase(parameters.FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to the c# '<see cref="value"/>' comments equivalent.
            Handlebars.RegisterHelper("see-comments", (writer, context, parameters) => writer.WriteSafeString(ConfigBase.ToSeeComments(parameters.FirstOrDefault()?.ToString())));

            // Inserts indent spaces based on the passed count value.
            Handlebars.RegisterHelper("indent", (writer, context, parameters) => writer.WriteSafeString(new string(' ', (int)(parameters.FirstOrDefault() ?? 0))));

            // Adds a value to a value.
            Handlebars.RegisterHelper("add", (writer, context, parameters) =>
            {
                int sum = 0;
                foreach (var p in parameters)
                {
                    if (p is int pi)
                        sum += pi;
                    else if (p is string ps)
                        sum += int.Parse(ps, NumberStyles.Integer, CultureInfo.InvariantCulture);
                    else
                        writer.WriteSafeString("!!! add with invalid integer !!!");
                }

                writer.WriteSafeString(sum);
            });
        }

        /// <summary>
        /// Perform the actual IfEq equality check.
        /// </summary>
        private static bool IfEq(Arguments args)
        {
            bool func()
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (Comparer.Default.Compare(args[0], RValConvert(args[0], args[i])) == 0)
                        return true;
                }

                return false;
            }

            return args.Length switch
            {
                0 => true,
                1 => args[0] == null,
                2 => Comparer.Default.Compare(args[0], RValConvert(args[0], args[1])) == 0,
                _ => func()
            };
        }

        /// <summary>
        /// Perform the actual IfLe equality check.
        /// </summary>
        private static bool IfLe(Arguments args)
        {
            bool func()
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (Comparer.Default.Compare(args[0], RValConvert(args[0], args[i])) > 0)
                        return false;
                }

                return true;
            }

            return args.Length switch
            {
                0 => false,
                1 => false,
                _ => func()
            };
        }

        /// <summary>
        /// Perform the actual IfGe equality check.
        /// </summary>
        private static bool IfGe(Arguments args)
        {
            bool func()
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (Comparer.Default.Compare(args[0], RValConvert(args[0], args[i])) < 0)
                        return false;
                }

                return true;
            }

            return args.Length switch
            {
                0 => false,
                1 => false,
                _ => func()
            };
        }

        /// <summary>
        /// Converts the rval to match the lval type.
        /// </summary>
        private static object? RValConvert(object lval, object rval)
        {
            if (lval == null || rval == null)
                return rval;

            var type = lval.GetType();
            if (type == typeof(string))
                return rval;
            else if (type == typeof(bool))
                return bool.Parse(rval.ToString()!);
            else
                return int.Parse(rval.ToString()!, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Format a string using the first parameter as string format and the remainder of parameters as the arguments.
        /// </summary>
        private static string? FormatString(Arguments parameters)
        {
            if (parameters.Length == 0)
                return null;
            else if (parameters.Length == 1)
                return parameters[0].ToString();
            else
                return string.Format(parameters[0].ToString()!, parameters.TakeLast(parameters.Length - 1).ToArray());
        }
    }
}