# Change log

Represents the **NuGet** versions.

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