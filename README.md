<br/>

![Logo](./images/Logo256x256.png "OnRamp")

<br/>

## Introduction

Provides extended code-generation capabilities that encapsulates [Handlebars](https://handlebarsjs.com/guide/) as the underlying code generator, or more specifically [Handlebars.Net](https://github.com/Handlebars-Net/Handlebars.Net).

This is intended to provide the base for code-generation tooling enabling a rich and orchestrated code-generation experience in addition to the templating and generation enabled using native _Handlebars_. 

<br/>

## Status

[![CI](https://github.com/Avanade/OnRamp/workflows/CI/badge.svg)](https://github.com/Avanade/OnRamp/actions?query=workflow%3ACI) [![NuGet version](https://badge.fury.io/nu/OnRamp.svg)](https://badge.fury.io/nu/OnRamp)

The included [change log](CHANGELOG.md) details all key changes per published version.

<br/>

## Overview

Code generation can be used to greatly accelerate application development where standard coding patterns can be determined and the opportunity to automate exists.

Code generation can bring many or all of the following benefits:
- **Acceleration** of development;
- **Consistency** of approach;
- **Simplification** of implementation;
- **Reusability** of logic;
- **Evolution** of approach over time;
- **Richness** of capabilities with limited effort.

There are generally two types of code generation:
- **Gen-many** - the ability to consistently generate a code artefact multiple times over its lifetime without unintended breaking side-effects. Considered non-maintainable by the likes of developers as contents may change at any time; however, can offer extensible hooks to enable custom code injection where applicable. This approach offers the greatest long-term benefits.
- **Gen-once** - the ability to generate a code artefact once to effectively start the development process. Considered maintainable, and should not be re-generated as this would override custom coded changes.

The code-generation capabilities within _OnRamp_ support both of the above two types.

<br/>

## Capabilities

_OnRamp_ has been created to provide a rich and standardized foundation for orchestrating [Handlebars](https://handlebarsjs.com/guide/)-based code-generation.

<br/>

### Composition

The _OnRamp_ code-generation tooling is composed of the following:

1. [Configuration](#Configuration) - data used as the input to drive the code-generation via the underlying _templates_.
2. [Templates](#Templates) - [Handlebars](https://handlebarsjs.com/guide/) templates that define a specific artefact's content.
3. [Scripts](#Scripts) - orchestrates one or more _templates_ that are used to generate artefacts for a given _configuration_ input.

<br/>

### Configuration

The code-generation is driven by a configuration data source, in this case a YAML or JSON file. This acts as a type of DSL ([Domain Specific Language](https://en.wikipedia.org/wiki/Domain-specific_language)) to define the key characteristics / properties that will be used to generate the required artefacts.

Configuration consists of the following, which each ultimately inherit from [`ConfigBase`](./src/OnRamp/Config/ConfigBase.cs) for their base capabilities:

- **Root node** - inherits from [`ConfigRootBase`](./src/OnRamp/Config/ConfigRootBase.cs) to enable additional runtime parameters ([`IRootConfig`](./src/OnRamp/Config/IRootConfig.cs)).
- **Child nodes** - zero or more child nodes (hierarchical) that inherit from [`ConfigBase<TRoot, TParent>`](./src/OnRamp/Config/ConfigBaseT.cs) that specify the `Root` and `Parent` hierarchy.

The advantage of using a .NET typed class for the configuration is that additional properties (computed at runtime) can be added to aid the code-generation process. The underlying `Prepare` method provides a consistent means to implement this logic at runtime.

The following attributes should be used when defining the .NET Types, as they enable validation, and corresponding schema/documentation generation (where required):

Attribute | Description
-|-
[`CodeGenClassAttribute`](./src/OnRamp/Config/CodeGenClassAttribute.cs) | Defines the schema/documentation details for the .NET class.
[`CodeGenCategoryAttribute`](./src/OnRamp/Config/CodeGenCategoryAttribute.cs) | Defines one or more documentation categories for a .NET class.
[`CodeGenPropertyAttribute`](./src/OnRamp/Config/CodeGenPropertyAttribute.cs) | Defines validation (`IsMandatory`, `IsUnique` and `Options`) and documentation for a property (non-collection).
[`CodeGenPropertyCollectionAttribute`](./src/OnRamp/Config/CodeGenPropertyCollectionAttribute.cs) | Defines validation (`IsMandatory`) and documentation for a collection property.

The configuration must also use the [Newtonsoft Json.NET serializer attributes](https://www.newtonsoft.com/json/help/html/SerializationAttributes.htm) as [Json.NET](https://www.newtonsoft.com/json/help) is used internally to perform all JSON deserialization.

<br/>

#### Example

An example is as follows:

``` csharp
[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[CodeGenClass("Entity", Title = "'Entity' object.", Description = "The `Entity` object.", Markdown = "This is a _sample_ markdown.", ExampleMarkdown = "This is an `example` markdown.")]
[CodeGenCategory("Key", Title = "Provides the _Key_ configuration.")]
[CodeGenCategory("Collection", Title = "Provides related child (hierarchical) configuration.")]
public class EntityConfig : ConfigRootBase<EntityConfig>
{
    [JsonProperty("name")]
    [CodeGenProperty("Key", Title = "The entity name.", IsMandatory = true)]
    public string? Name { get; set; }

    [JsonProperty("properties")]
    [CodeGenPropertyCollection("Collection", Title = "The `Property` collection.", IsImportant = true)]
    public List<PropertyConfig>? Properties { get; set; }

    protected override void Prepare()
    {
        Properties = PrepareCollection(Properties);
    }
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[CodeGenClass("Property", Title = "'Property' object.", Description = "The `Property` object.")]
[CodeGenCategory("Key", Title = "Provides the _Key_ configuration.")]
public class PropertyConfig : ConfigBase<EntityConfig, EntityConfig>
{
    public override string QualifiedKeyName => BuildQualifiedKeyName("Property", Name);

    [JsonProperty("name")]
    [CodeGenProperty("Key", Title = "The property name.", IsMandatory = true, IsUnique = true)]
    public string? Name { get; set; }

    [JsonProperty("type")]
    [CodeGenProperty("Key", Title = "The property type.", Description = "This is a more detailed description for the property type.", IsImportant = true, Options = new string[] { "string", "int", "decimal" })]
    public string? Type { get; set; }

    [JsonProperty("isNullable")]
    [CodeGenProperty("Key", Title = "Indicates whether the property is nullable.")]
    public bool? IsNullable { get; set; }

    protected override void Prepare()
    {
        Type = DefaultWhereNull(Type, () => "string");
    }
}
```

A corresponding configuration YAML example is as follows:

``` yaml
name: Person
properties:
- { name: Name }
- { name: Age, type: int }
- { name: Salary, type: decimal, isNullable: true }
```

<br/>

### Templates

Once the code-gen configuration data source has been defined, one or more templates will be required to define the artefact output. These templates are defined using [Handlebars](https://handlebarsjs.com/guide/) syntax. Template files can either be added as an embedded resource within a folder named `Templates` (primary), or referenced directly on the file system (secondary), to enable runtime access.

Additionally, Handlebars has been [extended](./src/OnRamp/Utility/HandlebarsHelpers.cs) to add additional capabilities beyond what is available [natively](https://handlebarsjs.com/guide/builtin-helpers.html) to further enable the required generated output. _Note_ that where any of the following documented functions denote `String.Format` will result in the usage of the .NET [`String.Format`](https://docs.microsoft.com/en-us/dotnet/api/system.string.format) where the first argument is the format, and the remainder are considered the arguments referenced by the format.

<br/>

#### Conditional functions

The following functions represent additional Handlebars conditions:

Function | Description
-|-
`ifeq` | Checks that the first argument equals at least one of the subsequent arguments.
`ifne` | Checks that the first argument does not equal any of the subsequent arguments.
`ifle` | Checks that the first argument is less than or equal to the subsequent arguments.
`ifge` | Checks that the first argument is greater than or equal to the subsequent arguments.
`ifval` | Checks that all of the arguments have a non-`null` value.
`ifnull` | Checks that all of the arguments have a `null` value.
`ifor` | Checks that any of the arguments have a `true` value where `bool`; otherwise, non-`null` value.

<br/>

#### String manipulation functions

The following functions perform the specified string manipulation (generally using [`StringConverter`](./src/OnRamp/Utility/StringConverter.cs)) writing the output to the code-generated aretfact:

Function | Description
-|-
`format` | Writes the arguments using `String.Format.`
`lower` | Converts and writes a value as lower case.
`upper` | Converts and writes a value as upper case.
`camel` | Converts and writes a value as camel case.
`pascal` | Converts and writes a value as pascal case.
`private` | Converts and writes a value as private case.
`sentence` | Converts and writes a value as sentence case.
`past-tense` | Converts and writes a value as past tense.
`pluralize` | Converts and writes a singularized value as the plural.
`singularize` | Converts and writes a pluralized value as the single.
`see-comments` | Converts and writes a value as a C# `<see cref="value"/>` comments equivalent.

<br/>

#### Miscellaneous functions

The following functions perform miscellanous 

Function | Description
-|-
`indent` | Inserts indent spaces based on the passed count value.
`add` | Adds all the arguments and writes the sum.

<br/>

#### Troubleshooting functions

As there is no native integrated means to set a breakpoint and debug the template directly, logging and debugger functions have been added to aid troubleshooting.

Function | Description
-|-
`log-error` | Logs ([`ILogger.LogInformation`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions.loginformation)) the arguments using `String.Format`.
`log-warning` | Logs ([`ILogger.LogWarning`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions.logwarning)) the arguments using `String.Format`.
`log-error` | Logs Logs ([`ILogger.LogError`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions.logerror)) the arguments using `String.Format`.
`log-debug` | Logs ([`Debug.WriteLine`](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debug.writeline)) the arguments using `String.Format`.
`debug` | Logs ([`Debug.WriteLine`](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debug.writeline)) the arguments using `String.Format`; then invokes  [`Debugger.Break`](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debugger.break).

Any functions that denote `String.Format` will result in the usage of the .NET [`String.Format`](https://docs.microsoft.com/en-us/dotnet/api/system.string.format) where the first argument is the format, and the remainder are the arguments.

<br/>

#### Example

Example usage is as follows:

``` handlebars
{{#ifeq Type 'int' 'decimal'}}Is a number.{{/ifeq}}
{{format '{0:yyyy-MM-dd HH:mm:ss}' Order.Date}}
{{camel Name}}
{{log-warn 'The name {0} is not valid.' Name}}
```

<br/>

### Scripts

To orchestrate the code generation, in terms of the [Templates](#Templates) to be used, a YAML-based script-like file is used. Script files can either be added as an embedded resource within a folder named `Scripts` (primary), or referenced directly on the file system (secondary), to enable runtime access.

<br/>

#### Root

The following are the root [`CodeGenScript`](./src/OnRamp/Scripts/CodeGenScript.cs) properties:

Property | Description
-|-
`configType` | The expected .NET [Configuration](#Configuration) root node `Type` name (as used by [`Type.GetType`](https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype#System_Type_GetType_System_String_)). This ensures that the code generator will only be executed with the specified configuration source.
`inherits` | A script file can inherit the script configuration from one or more parent script files specified by a script name array. This is intended to simplify/standardize the addition of additional artefact generation without the need to repeat.
`editorType` | The .NET `Type` name (as used by [`Type.GetType`](https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype#System_Type_GetType_System_String_)) that provides an opportunity to modify the loaded configuration. The `Type` must implement [`IConfigEditor`](./src/OnRamp/Config/IConfigEditor.cs). This enables runtime changes to configuration where access to the underlying source code for the configuration is unavailable; see [Personalization](#Personalization).
`generators` | A collection of none of more scripted generators.

<br/>

#### Generators

The following are the `generators` collection [`CodeGenScriptItem`](./src/OnRamp/Scripts/CodeGenScriptItem.cs) properties:

Attribute | Description
-|-
`type` | The .NET `Type` name (as used by [`Type.GetType`](https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype#System_Type_GetType_System_String_)) that will perform the underlying configuration data selection, where the corresponding `Template` will be invoked per selected item. This `Type` must inherit from either [`CodeGeneratorBase<TRootConfig>`](./src/OnRamp/Generators/CodeGeneratorBaseT.cs) or [`CodeGeneratorBase<TRootConfig, TGenConfig>`](./src/OnRamp/Generators/CodeGeneratorBaseT2.cs). The inherited `SelectGenConfig` method should be overridden where applicable to perform the actual selection.
`template` | The name of the [Handlebars template](https://handlebarsjs.com/guide/) that should be used.
`file` | The name of the file (artefact) that will be generated; this also supports _Handlebars_ syntax to enable runtime computation.
`directory` | This is the sub-directory (path) where the file (artefact) will be generated; this also supports _Handlebars_ syntax to enable runtime computation.
`genOnce` | This boolean (`true`/`false`) indicates whether the file is to be only generated once; i.e. only created where it does not already exist (optional).
`genOncePattern` | The file name pattern to search, including wildcards, to validate if the file (artefact) already exists (where `genOnce` is `true`). This is optional and where not specified will default to `file`. This is useful in scenarios where the file name is not fixed; for example, contains date and time. 
`text` | The text written to the log / console to enable additional context (optional).

Any other YAML properties specified will be automatically passed in as runtime parameters (name/value pairs); see [`IRootConfig.RuntimeParameters`](./src/OnRamp/Config/IRootConfig.cs).

<br/>

#### Example

An example of a Script YAML file is as follows:

``` yaml
configType: OnRamp.Test.Config.EntityConfig, OnRamp.Test
generators:
- { type: 'OnRamp.Test.Generators.EntityGenerator, OnRamp.Test', template: EntityExample.hbs, directory: "{{lookup RuntimeParameters 'Directory'}}", file: '{{Name}}.txt', Company: Xxx, AppName: Yyy }
- { type: 'OnRamp.Test.Generators.PropertyGenerator, OnRamo.Test', template: PropertyExample.hbs, directory: "{{lookup Root.RuntimeParameters 'Directory'}}", file: '{{Name}}.txt' }
```

A [`CodeGeneratorBase<TRootConfig, TGenConfig>`](./src/OnRamp/Generators/CodeGeneratorBaseT2.cs) example is as follows:

``` csharp
// A generator that is targeted at the Root (EntityConfig); pre-selects automatically.
public class EntityGenerator : CodeGeneratorBase<EntityConfig> { }

// A generator that targets a Child (PropertyConfig); requires selection from the Root config (EntityConfig).
public class PropertyGenerator : CodeGeneratorBase<EntityConfig, PropertyConfig>
{
    protected override IEnumerable<PropertyConfig> SelectGenConfig(EntityConfig config) => config.Properties!;
}
```

<br/>

## Code-generation

To enable code-generation the [`CodeGenerator`](./src/OnRamp/CodeGenerator.cs) is used. The constructor for this class takes a [`CodeGeneratorArgs`](./src/OnRamp/CodeGeneratorArgs.cs) that specifies the key input, such as the [script](#Scripts) file name. The additional argument properties enable additional capabilities within. The `CodeGenerator` constructor will ensure that the underlying script configuration is valid before continuing.

To perform the code-generation the `Generate` method is invoked passing the [configuration](#Configuration) file input. The configuration is initially parsed and validated, then each of the _generators_ described within the corresponding _script_ file are instantiated, the configuration data selected and iterated, then passed into the [`HandlebarsCodeGenerator`](./src/OnRamp/Utility/HandlebarsCodeGenerator.cs) to perform the code-generation proper against the scripted _template_. A [`CodeGenStatistics`](./src/OnRamp/CodeGenStatistics.cs) is returned providing basic statistics from the code-generation execution.

The `CodeGenerator` can be inherited to further customize; with the `OnBeforeScript`, `OnCodeGenerated`, and `OnAfterScript` methods available for override.

An example is as follows:

``` csharp
var cg = new CodeGenerator(new CodeGeneratorArgs("Script.yaml") { Assemblies = new Assembly[] { typeof(Program).Assembly } });
var stats = cg.Generate("Configuration.yaml");
```

<br/>

## Console application

[`OnRamp`](./src/OnRamp/Console/CodeGenConsole.cs) has been optimized so that a new console application can reference and inherit the underlying capabilities.

Where executing directly the default command-line options are as follows.

```
Xxx Code Generator.

Usage: Xxx [options]

Options:
  -?|-h|--help              Show help information.
  -s|--script               Script orchestration file/resource name.
  -c|--config               Configuration data file name.
  -o|--output               Output directory path.
  -a|--assembly             Assembly containing embedded resources (multiple can be specified in probing order).
  -p|--param                Parameter expressed as a 'Name=Value' pair (multiple can be specified).
  -cs|--connection-string   Database connection string.
  -cv|--connection-varname  Database connection string environment variable name.
  -enc|--expect-no-changes  Indicates to expect _no_ changes in the artefact output (e.g. error within build pipeline).
  -sim|--simulation         Indicates whether the code-generation is a simulation (i.e. does not create/update any artefacts).
```

The recommended approach is to [invoke](#Invoke) the _OnRamp_ capabilities directly and include the required _Scripts_ and _Templates_ as embedded resources to be easily referenced at runtime. Otherwise, the capabilities can be [inherited](#Inherit) and overridden to enhance the experience.

<br/>

### Invoke

Where the out-of-the-box capabiltity of _OnRamp_ is acceptable, then simply invoking the [`CodeGenConsole`](./src/OnRamp/Console/CodeGenConsole.cs) will perform the code-generation using the embedded resources. The command-line arguments need to be passed through to support the standard options. Additional methods exist to specify defaults or change behaviour as required. An example `Program.cs` is as follows.

``` csharp
using OnRamp;
using System.Threading.Tasks;

namespace My.Application
{
    public class Program
    {
        static Task<int> Main(string[] args) => CodeGenConsole.Create<Program>().RunAsync(args);
    }
}
```

<br/>

### Inherit

The [`CodeGenConsoleBase`](./src/OnRamp/Console/CodeGenConsoleBase.cs) is designed to be inherited, with opportunities within to tailor the console application experience via the following key overrideable methods:

Method | Description
-|-
`OnBeforeExecute` | Invoked before the underlying console execution occurs. This provides an opportunity to remove or add to the command-line commands and options. The command-line capabilities are enabled using [`CommandLineUtils`](https://github.com/natemcmaster/CommandLineUtils).
`OnValidation` | Invoked after command parsing is complete and before the underlying code-generation. This provides an opportunity to perform additional validation on the command-line commands and options.
`OnCodeGeneration` | Invoked to instantiate and run a [`CodeGenerator`](./src/OnRamp/CodeGenerator.cs) using the [`CodeGeneratorArgs`](./src/OnRamp/CodeGeneratorArgs.cs) returning the corresponding [`CodeGenStatistics`](./src/OnRamp/CodeGenStatistics.cs). This provides an opportunity to fully manage / orchestrate the code-generation.

For an example of advanced usage, see [`Beef.CodeGen.Core`](https://github.com/Avanade/Beef/blob/master/tools/Beef.CodeGen.Core/CodeGenConsole.cs) that uses this capability as the basis for its code-generation. 


<br/>

## Personalization

To enable consumers of a code-generator to personalize, and/or override, the published code-generation behaviour the [Scripts](#Scripts) and [Templates](#Templates) can be overridden. This will avoid the need for a consumer to clone the solution and update unless absolutely neccessary. This is achieved by passing in `Assembly` references which contain embedded resources of the same name; the underlying [`CodeGenerator`](./src/OnRamp/CodeGenerator.cs) will use these and fall back to the original version where not overridden.

There is _no_ means to extend the underlying configuration .NET types directly. However, as all the configuration types inherit from [`ConfigBase`](./src/OnRamp/Config/ConfigBase.cs) the `ExtraProperties` hash table is populated with any additional configurations during the deserialization process. These values can then be referenced direcly within the Templates as required. To perform further changes to the configuration at runtime an [`IConfigEditor`](./src/OnRamp/Config/IConfigEditor.cs) can be added and then referenced from within the corresponding `Scripts` file; it will then be invoked during code generation enabling further changes to occur. The `ConfigBase.CustomProperties` hash table is further provided to enable custom properties to be set and referenced in a consistent manner.

<br/>

## Utility

Some additional utility capabilites have been provided:

Class | Description
-|-
[`JsonSchemaGenerator`](./src/OnRamp/Utility/JsonSchemaGenerator.cs) | Provides the capability to generate a [JSON Schema](https://json-schema.org/) from the [configuration](#Configuration). This can then be published to the likes of the [JSON Schema Store](https://www.schemastore.org/) so that it can be used in Visual Studio and Visual Studio Code (or other editor of choice) to provide editor intellisense and basic validation.
[`MarkdownDocumentationGenerator`](./src/OnRamp/Utility/MarkdownDocumentationGenerator.cs) | Provides the capability to generate [markdown](https://en.wikipedia.org/wiki/Markdown) documentation files from the [configuration](#Configuration). These can then be published within the owning source code repository or to a wiki to provide corresponding documentation.
[`StringConverter`](./src/OnRamp/Utility/StringConverter.cs) | Provides additional string conversions that are useful where generating code; for example: `ToCamelCase`, `ToPascalCase`, `ToPrivateCase`, `ToSentenceCase`, `ToSnakeCase`, `ToKebabCase`, `ToPastTense`, `ToPlural`, `ToSingle`, `ToComments` and `ToSeeComments`.

<br/>

## Other repos

These other _Avanade_ repositories leverage _OnRamp_ to provide code-generation capabilities:
- [DbEx](https://github.com/Avanade/DbEx) - Database and DBUP extensions.
- [NTangle](https://github.com/Avanade/NTangle) - Change Data Capture (CDC) code generation tool and runtime.
- [Beef](https://github.com/Avanade/Beef) - Business Entity Execution Framework to enable industralisation of API development.

<br/>

## License

_OnRamp_ is open source under the [MIT license](./LICENSE) and is free for commercial use.

<br/>

## Contributing

One of the easiest ways to contribute is to participate in discussions on GitHub issues. You can also contribute by submitting pull requests (PR) with code changes. Contributions are welcome. See information on [contributing](./CONTRIBUTING.md), as well as our [code of conduct](https://avanade.github.io/code-of-conduct/).

<br/>

## Security

See our [security disclosure](./SECURITY.md) policy.

<br/>

## Who is Avanade?

[Avanade](https://www.avanade.com) is the leading provider of innovative digital and cloud services, business solutions and design-led experiences on the Microsoft ecosystem, and the power behind the Accenture Microsoft Business Group.
