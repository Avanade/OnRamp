# Change log

Represents the **NuGet** versions.

## v1.0.8
- *Fixed:* Added comparison context where reporting a file update as a result of using `ExpectNoChanges`.

## v1.0.7
- *Fixed:* `CodeGenerator` now supports inheritance as intended; new `CreateAsync<TCodeGenerator>` added to enable.
- *Fixed:* The `CodeGeneratorArgsBase.ConfigFileName` per script `IRootConfig` instance included within `CodeGenStatistics` per script item execution to enable access from `CodeGenerator`.

## v1.0.6
- *Enhancement:* Added new `set-value` function to allow a variable to be updated from the template when executed.
- *Enhancement:* Added new `add-value` function to allow a variable to be added to from the template when executed.

## v1.0.5
- *Enhancement:* Changed the project to be .NET Standard 2.1 versus targeting specific framework version. This has had the side-effect of losing the ability to execute directly from the command-line. Given this should typically be inherited and then executed, this functionality loss is considered a minor inconvenience.

## v1.0.4
- [*Issue 12:*](https://github.com/Avanade/OnRamp/issues/12) Added `CodeGenPropertyAttribute.IsUnique` to enable configurable property value uniqueness validation checking.

## v1.0.3
- *Republish:* Version updated and republished NuGet to correct issue.

## V1.0.2
- *Removed:* `Database` and related capabilities have been removed; these capabilities now reside in [DbEx](https://github.com/Avanade/DbEx).
- *New:* The database connection capabilities within `ICodeGeneratorArgs` have been moved into a new `ICodeGeneratorDbArgs` to enable this _base_ functionality to be leveraged without the other properties.
- *Enhancement:* The `RuntimeParameters` have been changed from `IDictionary<string, string?>` to `IDictionary<string, object?>` to improve flexibility.
- *Enhancement:* The code-generation has been updated to support async.
- *Enhancement:* The `CodeGenConsoleBase` has been removed and functionality moved into `CodeGenConsole`.

## v1.0.1
- *New:* Initial publish to GitHub/NuGet. This was originally harvested from, and will replace, the core code-generation within [Beef](https://github.com/Avanade/Beef/tree/master/tools/Beef.CodeGen.Core).