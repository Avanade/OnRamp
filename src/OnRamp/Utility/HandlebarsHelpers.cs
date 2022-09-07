// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using HandlebarsDotNet;
using Microsoft.Extensions.Logging;
using OnRamp.Console;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;

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
            Handlebars.RegisterHelper("ifeq", (writer, options, context, args) =>
            {
                if (IfEq(ValidateArgs(options.Name.Path, args)))
                    options.Template(writer, context);
                else
                    options.Inverse(writer, context);
            });

            // Will check that the first argument does not equal any of the subsequent arguments.
            Handlebars.RegisterHelper("ifne", (writer, options, context, args) =>
            {
                if (IfEq(ValidateArgs(options.Name.Path, args)))
                    options.Inverse(writer, context);
                else
                    options.Template(writer, context);
            });

            // Will check that the first argument is less than or equal to the subsequent arguments.
            Handlebars.RegisterHelper("ifle", (writer, options, context, args) =>
            {
                if (IfLe(ValidateArgs(options.Name.Path, args)))
                    options.Template(writer, context);
                else
                    options.Inverse(writer, context);
            });

            // Will check that the first argument is greater than or equal to the subsequent arguments.
            Handlebars.RegisterHelper("ifge", (writer, options, context, args) =>
            {
                if (IfGe(ValidateArgs(options.Name.Path, args)))
                    options.Template(writer, context);
                else
                    options.Inverse(writer, context);
            });

            // Will check that all of the arguments have a non-<c>null</c> value.
            Handlebars.RegisterHelper("ifval", (writer, options, context, args) =>
            {
                foreach (var arg in ValidateArgs(options.Name.Path, args))
                {
                    if (arg == null)
                    {
                        options.Inverse(writer, context);
                        return;
                    }
                }

                options.Template(writer, context);
            });

            // Will check that all of the arguments have a <c>null</c> value.
            Handlebars.RegisterHelper("ifnull", (writer, options, context, args) =>
            {
                foreach (var arg in ValidateArgs(options.Name.Path, args))
                {
                    if (arg != null)
                    {
                        options.Inverse(writer, context);
                        return;
                    }
                }

                options.Template(writer, context);
            });

            // Will check that any of the arguments have a <c>true</c> value where bool; otherwise, non-null value.
            Handlebars.RegisterHelper("ifor", (writer, options, context, args) =>
            {
                foreach (var arg in ValidateArgs(options.Name.Path, args))
                {
                    if (arg is bool opt)
                    {
                        if (opt)
                        {
                            options.Template(writer, context);
                            return;
                        }
                    }
                    else if (arg != null)
                    {
                        var opt2 = arg as bool?;
                        if (opt2 != null && !opt2.Value)
                            continue;

                        options.Template(writer, context);
                        return;
                    }
                }

                options.Inverse(writer, context);
            });

            // Formats and writes value.
            Handlebars.RegisterHelper("format", (writer, context, args) => writer.WriteSafeString(FormatString(ValidateArgs("format", args))));

            // Past-tense, pluralize and singularize.
            Handlebars.RegisterHelper("past-tense", (writer, context, args) => writer.WriteSafeString(StringConverter.ToPastTense(ValidateArgs("past-tense", args).FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("pluralize", (writer, context, args) => writer.WriteSafeString(StringConverter.ToPlural(ValidateArgs("pluralize", args).FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("singularize", (writer, context, args) => writer.WriteSafeString(StringConverter.ToSingle(ValidateArgs("singularize", args).FirstOrDefault()?.ToString()) ?? ""));

            // Logs using the String.Format.
            Handlebars.RegisterHelper("log-info", (writer, context, args) => (Logger ?? new ConsoleLogger()).LogInformation(FormatString(ValidateArgs("log-info", args))));
            Handlebars.RegisterHelper("log-warning", (writer, context, args) => (Logger ?? new ConsoleLogger()).LogWarning(FormatString(ValidateArgs("log-warning", args))));
            Handlebars.RegisterHelper("log-error", (writer, context, args) => (Logger ?? new ConsoleLogger()).LogError(FormatString(ValidateArgs("log-error", args))));
            Handlebars.RegisterHelper("log-debug", (writer, context, args) => System.Diagnostics.Debug.WriteLine($"Handlebars > {FormatString(ValidateArgs("log-debug", args))}"));

            // Logs using the String.Format to the debugger and then initiates a break in the debugger itself.
            Handlebars.RegisterHelper("debug", (writer, context, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"Handlebars > {FormatString(ValidateArgs("debug", args))}");
                System.Diagnostics.Debugger.Break();
            });

            // Converts a value to lowercase or uppercase.
            Handlebars.RegisterHelper("lower", (writer, context, args) => writer.WriteSafeString(ValidateArgs("lower", args).FirstOrDefault()?.ToString()?.ToLowerInvariant() ?? ""));
            Handlebars.RegisterHelper("upper", (writer, context, args) => writer.WriteSafeString(ValidateArgs("upper", args).FirstOrDefault()?.ToString()?.ToUpperInvariant() ?? ""));

            // NOTE: Any ending in 'x' are to explicitly ignore special names!!!

            // Converts a value to camelcase.
            Handlebars.RegisterHelper("camel", (writer, context, args) => writer.WriteSafeString(StringConverter.ToCamelCase(ValidateArgs("camel", args).FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("camelx", (writer, context, args) => writer.WriteSafeString(StringConverter.ToCamelCase(ValidateArgs("camelx", args).FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to pascalcase.
            Handlebars.RegisterHelper("pascal", (writer, context, args) => writer.WriteSafeString(StringConverter.ToPascalCase(ValidateArgs("pascal", args).FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("pascalx", (writer, context, args) => writer.WriteSafeString(StringConverter.ToPascalCase(ValidateArgs("pascalx", args).FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to private case.
            Handlebars.RegisterHelper("private", (writer, context, args) => writer.WriteSafeString(StringConverter.ToPrivateCase(ValidateArgs("private", args).FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("privatex", (writer, context, args) => writer.WriteSafeString(StringConverter.ToPrivateCase(ValidateArgs("privatex", args).FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to snake case.
            Handlebars.RegisterHelper("snake", (writer, context, args) => writer.WriteSafeString(StringConverter.ToSnakeCase(ValidateArgs("snake", args).FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("snakex", (writer, context, args) => writer.WriteSafeString(StringConverter.ToSnakeCase(ValidateArgs("snakex", args).FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to kebab case.
            Handlebars.RegisterHelper("kebab", (writer, context, args) => writer.WriteSafeString(StringConverter.ToKebabCase(ValidateArgs("kebabx", args).FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("kebabx", (writer, context, args) => writer.WriteSafeString(StringConverter.ToKebabCase(ValidateArgs("kebabx", args).FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to sentence case.
            Handlebars.RegisterHelper("sentence", (writer, context, args) => writer.WriteSafeString(StringConverter.ToSentenceCase(ValidateArgs("sentence", args).FirstOrDefault()?.ToString()) ?? ""));
            Handlebars.RegisterHelper("sentencex", (writer, context, args) => writer.WriteSafeString(StringConverter.ToSentenceCase(ValidateArgs("sentencex", args).FirstOrDefault()?.ToString(), true) ?? ""));

            // Converts a value to the c# '<see cref="value"/>' comments equivalent.
            Handlebars.RegisterHelper("see-comments", (writer, context, args) => writer.WriteSafeString(StringConverter.ToSeeComments(ValidateArgs("see-comments", args).FirstOrDefault()?.ToString())));

            // Inserts indent spaces based on the passed count value.
            Handlebars.RegisterHelper("indent", (writer, context, args) => writer.WriteSafeString(new string(' ', (int)(ValidateArgs("indent", args).FirstOrDefault() ?? 0))));

            // Adds a value to a value.
            Handlebars.RegisterHelper("add", (writer, context, args) =>
            {
                int sum = 0;
                foreach (var arg in ValidateArgs("add", args))
                {
                    if (arg is int pi)
                        sum += pi;
                    else if (arg is string str)
                        sum += int.Parse(str, NumberStyles.Integer, CultureInfo.InvariantCulture);
                    else
                        throw new CodeGenException($"Handlebars template invokes function 'add' that references value '{arg}' which is unable to be parsed as an integer.");
                }

                writer.WriteSafeString(sum);
            });

            // Sets a value to another value.
            Handlebars.RegisterHelper("set-value", (writer, context, args) =>
            {
                ValidateArgs("set-value", args);
                if (args.Length != 2)
                    throw new CodeGenException($"Handlebars template invokes function 'set-value' which only supports two arguments.");

                if (args[0] is not string name)
                    throw new CodeGenException("Handlebars template invokes function 'set-value' where the first argument must be a string, being the property name.");

                var pi = context.Value.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty);
                if (pi == null)
                    throw new CodeGenException($"Handlebars template invokes function 'set-value' where the property '{name}' does not exist for Type {context.Value.GetType().Name}.");

                try
                {
                    pi.SetValue(context.Value, args[1]);
                }
                catch (Exception ex)
                {
                    throw new CodeGenException($"Handlebars template invokes function 'set-value' where the property '{name}' for Type {context.Value.GetType().Name} had a set value error: {ex.Message}");
                }
            });

            // Adds other value(s) to a value.
            Handlebars.RegisterHelper("add-value", (writer, context, args) =>
            {
                ValidateArgs("add-value", args);
                if (args[0] is not string name)
                    throw new CodeGenException("Handlebars template invokes function 'add-value' where the first argument must be a string, being the property name.");

                var pi = context.Value.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty);
                if (pi == null)
                    throw new CodeGenException($"Handlebars template invokes function 'add-value' where the property '{name}' does not exist for Type {context.Value.GetType().Name}.");

                decimal sum = 0;
                try
                {
                    sum += (decimal)pi.GetValue(context.Value);
                }
                catch (Exception ex)
                {
                    throw new CodeGenException($"Handlebars template invokes function 'add-value' where the property '{name}' for Type {context.Value.GetType().Name} had a get value error: {ex.Message}");
                }

                if (args.Length == 1)
                    sum++;
                else
                {
                    for (int i = 1; i < args.Length; i++)
                    {
                        if (args[i] is int ai)
                            sum += ai;
                        else if (args[i] is decimal di)
                            sum += di;
                        else if (args[i] is string si)
                            sum += decimal.Parse(si, CultureInfo.InvariantCulture);
                    }
                }

                try
                {
                    pi.SetValue(context.Value, sum);
                }
                catch (Exception ex)
                {
                    throw new CodeGenException($"Handlebars template invokes function 'add-value' where the property '{name}' for Type {context.Value.GetType().Name} had a set value error: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Validate arguments and ensure none are undefined.
        /// </summary>
        private static Arguments ValidateArgs(string function, Arguments args)
        {
            var ua = args.OfType<UndefinedBindingResult>().FirstOrDefault();
            if (ua == null)
                return args;

            throw new CodeGenException($"Handlebars template invokes function '{function}' that references '{ua.Value}' which is undefined.");
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
            else if (type == typeof(int))
                return int.Parse(rval.ToString()!, CultureInfo.InvariantCulture);
            else
                return decimal.Parse(rval.ToString()!, CultureInfo.InvariantCulture);
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