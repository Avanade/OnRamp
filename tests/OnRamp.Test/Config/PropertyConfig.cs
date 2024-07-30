using OnRamp.Config;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable enable

namespace OnRamp.Test.Config
{
    [CodeGenClass("Property", Title = "'Property' object.", Description = "The `Property` object.")]
    [CodeGenCategory("Key", Title = "Provides the _Key_ configuration.")]
    public class PropertyConfig : ConfigBase<EntityConfig, EntityConfig>
    {
        public override string QualifiedKeyName => BuildQualifiedKeyName("Property", Name);

        [JsonPropertyName("name")]
        [CodeGenProperty("Key", Title = "The property name.", IsMandatory = true, IsUnique = true)]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        [CodeGenProperty("Key", Title = "The property type.", Description = "This is a more detailed description for the property type.", IsImportant = true, Options = new string[] { "string", "int", "decimal" })]
        public string? Type { get; set; }

        [JsonPropertyName("isNullable")]
        [CodeGenProperty("Key", Title = "Indicates whether the property is nullable.")]
        public bool? IsNullable { get; set; }

        [JsonPropertyName("count")]
        [CodeGenProperty("Key", Title = "Test out an integer.")]
        public int? Count { get; set; }

        [JsonPropertyName("amount")]
        [CodeGenProperty("Key", Title = "Test out a decimal.")]
        public decimal? Amount { get; set; }

        protected override Task PrepareAsync()
        {
            Type = DefaultWhereNull(Type, () => "string");
            return Task.CompletedTask;
        }
    }
}

#nullable disable