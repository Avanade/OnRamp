﻿using OnRamp.Config;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable enable

namespace OnRamp.Test.Config
{
    [CodeGenClass("Entity", Title = "'Entity' object.", Description = "The `Entity` object.", Markdown = "This is a _sample_ markdown.", ExampleMarkdown = "This is an `example` markdown.")]
    [CodeGenCategory("Key", Title = "Provides the _Key_ configuration.")]
    [CodeGenCategory("Collection", Title = "Provides related child (hierarchical) configuration.")]
    public class EntityConfig : ConfigRootBase<EntityConfig>
    {
        [JsonPropertyName("name")]
        [CodeGenProperty("Key", Title = "The entity name.", IsMandatory = true)]
        public string? Name { get; set; }

        [JsonPropertyName("properties")]
        [CodeGenPropertyCollection("Collection", Title = "The `Property` collection.", IsImportant = true)]
        public List<PropertyConfig>? Properties { get; set; }

        protected override async Task PrepareAsync()
        {
            Properties = await PrepareCollectionAsync(Properties).ConfigureAwait(false);
        }
    }
}

#nullable disable