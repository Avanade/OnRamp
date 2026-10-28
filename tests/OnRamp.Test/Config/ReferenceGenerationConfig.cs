using NUnit.Framework;
using OnRamp.Config;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable enable

namespace OnRamp.Test.Config
{
    /// <summary>
    /// Provides a metadata-only root fixture that rejects construction and preparation.
    /// </summary>
    [CodeGenClass("Scalar", Title = "Scalar object.")]
    [CodeGenCategory("Primary")]
    public abstract class ReferenceRoot<T> : ConfigRootBase<T> where T : ConfigRootBase<T>
    {
        /// <summary>
        /// Rejects configuration construction during reference generation.
        /// </summary>
        protected ReferenceRoot() => throw new InvalidOperationException("Generation must not construct configurations.");

        /// <summary>
        /// Rejects configuration preparation during reference generation.
        /// </summary>
        protected override Task PrepareAsync() => throw new InvalidOperationException("Generation must not prepare configurations.");
    }

    /// <summary>
    /// Provides a metadata-only child fixture that rejects construction and preparation.
    /// </summary>
    public abstract class ReferenceChild<TParent> : ConfigBase<CodeGenReferenceConfig, TParent> where TParent : ConfigBase
    {
        /// <summary>
        /// Rejects child construction during reference generation.
        /// </summary>
        protected ReferenceChild() => throw new InvalidOperationException("Generation must not construct children.");

        /// <summary>
        /// Rejects child preparation during reference generation.
        /// </summary>
        protected override Task PrepareAsync() => throw new InvalidOperationException("Generation must not prepare children.");
    }

    /// <summary>
    /// Models a root with required, optional, and repeated configuration children.
    /// </summary>
    [CodeGenClass("CodeGen", Title = "Code generator.")]
    public class CodeGenReferenceConfig : ReferenceRoot<CodeGenReferenceConfig>
    {
        /// <summary>
        /// Gets the required source child without allowing runtime access.
        /// </summary>
        [JsonPropertyName("source")]
        [CodeGenProperty("Primary", Title = "Interface source.", Description = "Source details.", IsMandatory = true)]
        public SourceReferenceConfig Source => throw new NotSupportedException();

        /// <summary>
        /// Gets a repeated optional source child with an explicit null default.
        /// </summary>
        [JsonPropertyName("secondarySource")]
        [DefaultValue(null)]
        [CodeGenProperty("Primary", Title = "Optional source.")]
        public SourceReferenceConfig? SecondarySource => throw new NotSupportedException();

        /// <summary>
        /// Gets the file child containing the column collection.
        /// </summary>
        [JsonPropertyName("file")]
        [CodeGenProperty("Primary", Title = "Input file.")]
        public FileReferenceConfig? File => throw new NotSupportedException();

        /// <summary>
        /// Gets the string collection using existing inferred item types.
        /// </summary>
        [JsonPropertyName("tags")]
        [CodeGenPropertyCollection("Primary", Title = "Tags.")]
        public List<string>? Tags => throw new NotSupportedException();
    }

    /// <summary>
    /// Models required and optional collections without runtime construction.
    /// </summary>
    [CodeGenClass("Collections")]
    public class CollectionRequirementsConfig : ReferenceRoot<CollectionRequirementsConfig>
    {
        /// <summary>
        /// Gets the required configuration collection.
        /// </summary>
        [JsonPropertyName("requiredObjects")]
        [CodeGenPropertyCollection("Primary", IsMandatory = true)]
        public List<SourceReferenceConfig>? RequiredObjects => throw new NotSupportedException();

        /// <summary>
        /// Gets the optional configuration collection.
        /// </summary>
        [JsonPropertyName("optionalObjects")]
        [CodeGenPropertyCollection("Primary")]
        public List<SourceReferenceConfig>? OptionalObjects => throw new NotSupportedException();

        /// <summary>
        /// Gets the required explicitly typed scalar collection.
        /// </summary>
        [JsonPropertyName("requiredScalars")]
        [CodeGenPropertyCollection("Primary", IsMandatory = true, ItemJsonTypes = new[] { CodeGenJsonType.String, CodeGenJsonType.Number, CodeGenJsonType.Boolean })]
        public List<JsonElement>? RequiredScalars => throw new NotSupportedException();

        /// <summary>
        /// Gets the optional explicitly typed scalar collection.
        /// </summary>
        [JsonPropertyName("optionalScalars")]
        [CodeGenPropertyCollection("Primary", ItemJsonTypes = new[] { CodeGenJsonType.String, CodeGenJsonType.Number, CodeGenJsonType.Boolean })]
        public List<JsonElement>? OptionalScalars => throw new NotSupportedException();

        /// <summary>
        /// Gets the required inferred string collection.
        /// </summary>
        [JsonPropertyName("requiredStrings")]
        [CodeGenPropertyCollection("Primary", IsMandatory = true)]
        public List<string>? RequiredStrings => throw new NotSupportedException();

        /// <summary>
        /// Gets the optional inferred string collection.
        /// </summary>
        [JsonPropertyName("optionalStrings")]
        [CodeGenPropertyCollection("Primary")]
        public List<string>? OptionalStrings => throw new NotSupportedException();
    }

    /// <summary>
    /// Models declared defaults of different JSON value types.
    /// </summary>
    [CodeGenClass("Source", Title = "Source object.")]
    [CodeGenCategory("Primary")]
    public class SourceReferenceConfig : ReferenceChild<CodeGenReferenceConfig>
    {
        /// <summary>
        /// Gets the source type with a case-sensitive string default.
        /// </summary>
        [JsonPropertyName("type")]
        [DefaultValue("AzureServiceBus")]
        [CodeGenProperty("Primary", Title = "Source type.", Options = new[] { "AzureServiceBus" })]
        public string? Type => throw new NotSupportedException();

        /// <summary>
        /// Gets a Boolean property with a true default.
        /// </summary>
        [JsonPropertyName("enabled")]
        [DefaultValue(true)]
        [CodeGenProperty("Primary")]
        public bool? Enabled => throw new NotSupportedException();

        /// <summary>
        /// Gets a Boolean property with a false default.
        /// </summary>
        [JsonPropertyName("disabled")]
        [DefaultValue(false)]
        [CodeGenProperty("Primary")]
        public bool? Disabled => throw new NotSupportedException();

        /// <summary>
        /// Gets a property with an integer default.
        /// </summary>
        [JsonPropertyName("count")]
        [DefaultValue(42)]
        [CodeGenProperty("Primary")]
        public int? Count => throw new NotSupportedException();

        /// <summary>
        /// Gets a property with a fractional decimal default.
        /// </summary>
        [JsonPropertyName("amount")]
        [DefaultValue(typeof(decimal), "12.5")]
        [CodeGenProperty("Primary")]
        public decimal? Amount => throw new NotSupportedException();

        /// <summary>
        /// Gets a scalar property with an explicit null default.
        /// </summary>
        [JsonPropertyName("empty")]
        [DefaultValue(null)]
        [CodeGenProperty("Primary")]
        public string? Empty => throw new NotSupportedException();

        /// <summary>
        /// Gets a string default containing Markdown delimiters and line breaks.
        /// </summary>
        [JsonPropertyName("label")]
        [DefaultValue("A|B`C\r\nD")]
        [CodeGenProperty("Primary")]
        public string? Label => throw new NotSupportedException();

        /// <summary>
        /// Gets a collection whose declared null default replaces the implicit empty array.
        /// </summary>
        [JsonPropertyName("tags")]
        [DefaultValue(null)]
        [CodeGenPropertyCollection("Primary")]
        public List<string>? Tags => throw new NotSupportedException();
    }

    /// <summary>
    /// Models a single-valued child containing configuration collection items.
    /// </summary>
    [CodeGenClass("File", Title = "File object.")]
    [CodeGenCategory("Primary")]
    public class FileReferenceConfig : ReferenceChild<CodeGenReferenceConfig>
    {
        /// <summary>
        /// Gets the collection of column configurations.
        /// </summary>
        [JsonPropertyName("columns")]
        [CodeGenPropertyCollection("Primary", Title = "Columns.")]
        public List<ColumnReferenceConfig>? Columns => throw new NotSupportedException();
    }

    /// <summary>
    /// Models a collection item containing a required single-valued child.
    /// </summary>
    [CodeGenClass("Column", Title = "Column object.")]
    [CodeGenCategory("Primary")]
    public class ColumnReferenceConfig : ReferenceChild<FileReferenceConfig>
    {
        /// <summary>
        /// Gets the required validation child.
        /// </summary>
        [JsonPropertyName("validation")]
        [CodeGenProperty("Primary", Title = "Column validation.", IsMandatory = true)]
        public ValidationReferenceConfig Validation => throw new NotSupportedException();
    }

    /// <summary>
    /// Models explicit scalar unions and recursive configuration references.
    /// </summary>
    [CodeGenClass("Validation", Title = "Validation object.")]
    [CodeGenCategory("Primary")]
    public class ValidationReferenceConfig : ReferenceChild<ColumnReferenceConfig>
    {
        /// <summary>
        /// Gets the minimum union with duplicate declarations and a numeric default.
        /// </summary>
        [JsonPropertyName("min")]
        [DefaultValue(1.5)]
        [CodeGenProperty("Primary", Title = "Inclusive minimum value.", JsonTypes = new[] { CodeGenJsonType.Number, CodeGenJsonType.String, CodeGenJsonType.Number })]
        public JsonElement Min => throw new NotSupportedException();

        /// <summary>
        /// Gets the maximum value permitting strings and numbers.
        /// </summary>
        [JsonPropertyName("max")]
        [CodeGenProperty("Primary", JsonTypes = new[] { CodeGenJsonType.String, CodeGenJsonType.Number })]
        public JsonElement Max => throw new NotSupportedException();

        /// <summary>
        /// Gets scalar collection items permitting strings, numbers, and Booleans.
        /// </summary>
        [JsonPropertyName("options")]
        [CodeGenPropertyCollection("Primary", Title = "Allowed values.", ItemJsonTypes = new[] { CodeGenJsonType.Boolean, CodeGenJsonType.Number, CodeGenJsonType.String })]
        public List<JsonElement>? Options => throw new NotSupportedException();

        /// <summary>
        /// Gets a self-referencing configuration child.
        /// </summary>
        [JsonPropertyName("next")]
        [CodeGenProperty("Primary")]
        public ValidationReferenceConfig? Next => throw new NotSupportedException();

        /// <summary>
        /// Gets a reference closing the cycle back to the root.
        /// </summary>
        [JsonPropertyName("root")]
        [CodeGenProperty("Primary")]
        public CodeGenReferenceConfig? Configuration => throw new NotSupportedException();

        /// <summary>
        /// Gets a collection of the same configuration type.
        /// </summary>
        [JsonPropertyName("alternatives")]
        [CodeGenPropertyCollection("Primary")]
        public List<ValidationReferenceConfig>? Alternatives => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests explicit string compatibility for a CLR type.
    /// </summary>
    public class StringScalar<T> : ReferenceRoot<StringScalar<T>>
    {
        /// <summary>
        /// Gets the value declared as a JSON string.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary", JsonTypes = new[] { CodeGenJsonType.String })]
        public T Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests explicit Boolean compatibility for a CLR type.
    /// </summary>
    public class BooleanScalar<T> : ReferenceRoot<BooleanScalar<T>>
    {
        /// <summary>
        /// Gets the value declared as a JSON Boolean.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary", JsonTypes = new[] { CodeGenJsonType.Boolean })]
        public T Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests explicit number compatibility for a CLR type.
    /// </summary>
    public class NumberScalar<T> : ReferenceRoot<NumberScalar<T>>
    {
        /// <summary>
        /// Gets the value declared as a JSON number.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary", JsonTypes = new[] { CodeGenJsonType.Number })]
        public T Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests number/integer unions and duplicate type normalization.
    /// </summary>
    public class IntegerScalar<T> : ReferenceRoot<IntegerScalar<T>>
    {
        /// <summary>
        /// Gets the value with redundant number and integer declarations.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary", JsonTypes = new[] { CodeGenJsonType.Integer, CodeGenJsonType.Number, CodeGenJsonType.Integer })]
        public T Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests explicit integer compatibility for a CLR type.
    /// </summary>
    public class IntegerOnlyScalar<T> : ReferenceRoot<IntegerOnlyScalar<T>>
    {
        /// <summary>
        /// Gets the value declared as a JSON integer.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary", JsonTypes = new[] { CodeGenJsonType.Integer })]
        public T Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests explicit scalar unions for a CLR type.
    /// </summary>
    public class UnionScalar<T> : ReferenceRoot<UnionScalar<T>>
    {
        /// <summary>
        /// Gets the value declared as a string, number, or Boolean.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary", JsonTypes = new[] { CodeGenJsonType.Boolean, CodeGenJsonType.String, CodeGenJsonType.Number })]
        public T Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests explicit numeric items on a collection interface.
    /// </summary>
    public class NumberItems<T> : ReferenceRoot<NumberItems<T>>
    {
        /// <summary>
        /// Gets the collection whose items are declared as JSON numbers.
        /// </summary>
        [JsonPropertyName("value"), CodeGenPropertyCollection("Primary", ItemJsonTypes = new[] { CodeGenJsonType.Number })]
        public ICollection<T>? Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests scalar union items and type normalization on arrays.
    /// </summary>
    public class UnionItems<T> : ReferenceRoot<UnionItems<T>>
    {
        /// <summary>
        /// Gets the array whose items are strings, numbers, or Booleans.
        /// </summary>
        [JsonPropertyName("value"), CodeGenPropertyCollection("Primary", ItemJsonTypes = new[] { CodeGenJsonType.Boolean, CodeGenJsonType.Number, CodeGenJsonType.String, CodeGenJsonType.Boolean })]
        public T[]? Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests existing scalar inference without explicit type declarations.
    /// </summary>
    public class InferredScalar<T> : ReferenceRoot<InferredScalar<T>>
    {
        /// <summary>
        /// Gets the scalar value whose JSON type must be inferred.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary")]
        public T Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests collection item inference without explicit type declarations.
    /// </summary>
    public class InferredItems<T> : ReferenceRoot<InferredItems<T>>
    {
        /// <summary>
        /// Gets the collection whose item type must be inferred.
        /// </summary>
        [JsonPropertyName("value"), CodeGenPropertyCollection("Primary")]
        public List<T>? Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests rejection of an explicitly empty scalar declaration.
    /// </summary>
    public class EmptyScalar : ReferenceRoot<EmptyScalar>
    {
        /// <summary>
        /// Gets the value with an invalid empty scalar declaration.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary", JsonTypes = new CodeGenJsonType[0])]
        public JsonElement Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests rejection of an explicitly empty item declaration.
    /// </summary>
    public class EmptyItems : ReferenceRoot<EmptyItems>
    {
        /// <summary>
        /// Gets the collection with an invalid empty item declaration.
        /// </summary>
        [JsonPropertyName("value"), CodeGenPropertyCollection("Primary", ItemJsonTypes = new CodeGenJsonType[0])]
        public List<JsonElement>? Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests rejection of an undefined scalar type enum value.
    /// </summary>
    public class UndefinedScalar : ReferenceRoot<UndefinedScalar>
    {
        /// <summary>
        /// Gets the value with an undefined scalar type.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary", JsonTypes = new[] { (CodeGenJsonType)99 })]
        public JsonElement Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests rejection of an undefined item type enum value.
    /// </summary>
    public class UndefinedItems : ReferenceRoot<UndefinedItems>
    {
        /// <summary>
        /// Gets the collection with an undefined item type.
        /// </summary>
        [JsonPropertyName("value"), CodeGenPropertyCollection("Primary", ItemJsonTypes = new[] { (CodeGenJsonType)99 })]
        public List<JsonElement>? Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests rejection of collection metadata on a non-collection CLR type.
    /// </summary>
    public class InvalidCollection : ReferenceRoot<InvalidCollection>
    {
        /// <summary>
        /// Gets the non-collection value incorrectly declared as a collection.
        /// </summary>
        [JsonPropertyName("value"), CodeGenPropertyCollection("Primary", ItemJsonTypes = new[] { CodeGenJsonType.Number })]
        public int Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests rejection of a non-finite numeric default.
    /// </summary>
    public class NonFiniteDefault : ReferenceRoot<NonFiniteDefault>
    {
        /// <summary>
        /// Gets the numeric value with a default that JSON cannot represent.
        /// </summary>
        [JsonPropertyName("value"), DefaultValue(double.NaN)]
        [CodeGenProperty("Primary", JsonTypes = new[] { CodeGenJsonType.Number })]
        public double Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests rejection of an unsupported default value type.
    /// </summary>
    public class UnserializableDefault : ReferenceRoot<UnserializableDefault>
    {
        /// <summary>
        /// Gets the property whose declared default is an unserializable CLR type.
        /// </summary>
        [JsonPropertyName("value"), DefaultValue(typeof(string))]
        [CodeGenProperty("Primary")]
        public string? Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Claims the same configuration name as its root to test collision detection.
    /// </summary>
    [CodeGenClass("Scalar")]
    public class CollisionChild : ReferenceChild<CodeGenReferenceConfig> { }

    /// <summary>
    /// Tests configuration name collisions across distinct CLR types.
    /// </summary>
    public class CollisionRoot : ReferenceRoot<CollisionRoot>
    {
        /// <summary>
        /// Gets the child claiming the root configuration name.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary")]
        public CollisionChild? Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Provides a configuration child without required class metadata.
    /// </summary>
    public class MissingClassChild : ReferenceChild<CodeGenReferenceConfig> { }

    /// <summary>
    /// Tests rejection of referenced children without class metadata.
    /// </summary>
    public class MissingClassRoot : ReferenceRoot<MissingClassRoot>
    {
        /// <summary>
        /// Gets the child missing class metadata.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary")]
        public MissingClassChild? Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Tests escaped definition references and recursive property annotations.
    /// </summary>
    [CodeGenClass("Root/~ #")]
    public class EscapedNameRoot : ReferenceRoot<EscapedNameRoot>
    {
        /// <summary>
        /// Gets the annotated reference back to the same escaped definition.
        /// </summary>
        [JsonPropertyName("value"), CodeGenProperty("Primary", Title = "Recursive root.", Description = "Reference annotations.")]
        public EscapedNameRoot? Value => throw new NotSupportedException();
    }

    /// <summary>
    /// Supplies shared metadata cases and generator invocation helpers.
    /// </summary>
    public static class ReferenceGenerationCases
    {
        /// <summary>
        /// Enumerates valid and invalid scalar declarations for both generators.
        /// </summary>
        public static IEnumerable<TestCaseData> Metadata()
        {
            yield return Valid(typeof(StringScalar<string>), "string");
            yield return Valid(typeof(BooleanScalar<bool>), "boolean");
            yield return Valid(typeof(BooleanScalar<bool?>), "boolean");
            yield return Valid(typeof(StringScalar<JsonElement>), "string");
            yield return Valid(typeof(BooleanScalar<JsonElement>), "boolean");
            yield return Valid(typeof(NumberScalar<JsonElement>), "number");
            yield return Valid(typeof(IntegerOnlyScalar<JsonElement>), "integer");
            yield return Valid(typeof(IntegerScalar<JsonElement>), "number,integer");
            yield return Valid(typeof(UnionScalar<JsonElement>), "string,number,boolean");
            yield return Valid(typeof(UnionScalar<JsonElement?>), "string,number,boolean");
            yield return Valid(typeof(UnionItems<JsonElement>), "string,number,boolean");
            yield return Valid(typeof(InferredScalar<string>), "string");
            yield return Valid(typeof(InferredScalar<bool?>), "boolean");
            yield return Valid(typeof(InferredScalar<int?>), "integer");
            yield return Valid(typeof(InferredScalar<decimal?>), "number");
            yield return Valid(typeof(InferredItems<string>), "string");

            foreach (var type in new[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong) })
            {
                foreach (var scalar in new[] { type, typeof(Nullable<>).MakeGenericType(type) })
                {
                    yield return Valid(typeof(NumberScalar<>).MakeGenericType(scalar), "number");
                    yield return Valid(typeof(IntegerScalar<>).MakeGenericType(scalar), "number,integer");
                    yield return Valid(typeof(IntegerOnlyScalar<>).MakeGenericType(scalar), "integer");
                    yield return Valid(typeof(NumberItems<>).MakeGenericType(scalar), "number");
                    yield return Invalid(typeof(StringScalar<>).MakeGenericType(scalar));
                }
            }

            foreach (var type in new[] { typeof(decimal), typeof(float), typeof(double) })
            {
                foreach (var scalar in new[] { type, typeof(Nullable<>).MakeGenericType(type) })
                {
                    yield return Valid(typeof(NumberScalar<>).MakeGenericType(scalar), "number");
                    yield return Invalid(typeof(IntegerScalar<>).MakeGenericType(scalar));
                }
            }

            foreach (var type in new[]
            {
                typeof(EmptyScalar), typeof(EmptyItems), typeof(UndefinedScalar), typeof(UndefinedItems), typeof(InvalidCollection),
                typeof(InferredScalar<JsonElement>), typeof(InferredItems<JsonElement>),
                typeof(NumberScalar<string>), typeof(StringScalar<bool>), typeof(NumberScalar<bool?>),
                typeof(UnionScalar<string>), typeof(UnionItems<string>), typeof(NumberItems<bool>),
                typeof(StringScalar<DateTime>), typeof(StringScalar<char>), typeof(NumberScalar<object>),
                typeof(UnionScalar<JsonElement[]>), typeof(UnionItems<JsonElement[]>), typeof(UnionItems<object>),
                typeof(StringScalar<SourceReferenceConfig>), typeof(NumberItems<SourceReferenceConfig>),
                typeof(InferredScalar<DateTime>), typeof(InferredItems<int>)
            })
                yield return Invalid(type);
        }

        /// <summary>
        /// Creates a valid metadata test case with the expected JSON types.
        /// </summary>
        private static TestCaseData Valid(Type type, string types) => new TestCaseData(type, types);

        /// <summary>
        /// Creates a metadata test case that must fail generation.
        /// </summary>
        private static TestCaseData Invalid(Type type) => new TestCaseData(type, null);

        /// <summary>
        /// Generates a JSON schema for the specified root configuration type.
        /// </summary>
        public static string Schema(Type type)
        {
            using var stream = new MemoryStream();
            var method = typeof(JsonSchemaGenerator).GetMethods().Single(m => m.Name == "Generate" && m.GetParameters()[0].ParameterType == typeof(Stream));
            Invoke(method.MakeGenericMethod(type), new object?[] { stream, null });
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        /// <summary>
        /// Generates Markdown pages for the specified root configuration type.
        /// </summary>
        public static void Markdown(Type type, string directory)
        {
            var method = typeof(MarkdownDocumentationGenerator).GetMethod("Generate")!.MakeGenericMethod(type);
            Invoke(method, new object?[] { null, directory, true, false, "<br/>&dagger; ", null, null });
        }

        /// <summary>
        /// Invokes a generic generator and propagates its underlying exception.
        /// </summary>
        private static void Invoke(MethodInfo method, object?[] args)
        {
            try
            {
                method.Invoke(null, args);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
