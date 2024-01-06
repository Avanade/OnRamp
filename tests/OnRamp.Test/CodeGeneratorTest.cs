using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace OnRamp.Test
{
    [TestFixture]
    public class CodeGeneratorTest
    {
        [Test]
        public void A100_Script_DoesNotExist()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync<CodeGenerator>(new CodeGeneratorArgs("DoesNotExist.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'DoesNotExist.yaml' does not exist.", ex.Message);
        }

        [Test]
        public void A110_Script_InvalidFileType()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("InvalidFileType.xml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'InvalidFileType.xml' is invalid: Stream content type is not supported.", ex.Message);
        }

        [Test]
        public void A120_Script_InvalidYamlContent()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("InvalidYamlContent.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'InvalidYamlContent.yaml' is invalid: The JSON value could not be converted to OnRamp.Scripts.CodeGenScript. Path: $ | LineNumber: 0 | BytePositionInLine: 15.", ex.Message);
        }

        [Test]
        public void A130_Script_InvalidJsonContent()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("InvalidJsonContent.json").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'InvalidJsonContent.json' is invalid: '<' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0.", ex.Message);
        }

        [Test]
        public void A140_Script_InvalidEmpty()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("InvalidEmpty.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'InvalidEmpty.yaml' is invalid: Stream is empty.", ex.Message);
        }

        [Test]
        public void A150_Script_InvalidEmptyConfigType()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("InvalidEmptyConfigType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'InvalidEmptyConfigType.yaml' is invalid: [ConfigType] Value is mandatory.", ex.Message);
        }

        [Test]
        public void A160_Script_InvalidConfigType()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("InvalidConfigType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'InvalidConfigType.yaml' is invalid: [ConfigType] Type 'OnRamp.Scripts.CodeGenScriptItem' must inherit from ConfigRootBase<TRoot>.", ex.Message);
        }

        [Test]
        public void B100_Generator_DoesNotExist()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("GeneratorDoesNotExist.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'GeneratorDoesNotExist.yaml' is invalid: [Generate.Type] Type 'OnRamp.Test.Generators.DoesNotExist, OnRamp.Test' does not exist.", ex.Message);
        }

        [Test]
        public void B110_Generator_DiffConfigType()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("GeneratorDiffConfigType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'GeneratorDiffConfigType.yaml' is invalid: [Generate.Type] Type 'OnRamp.Test.Generators.ScriptsGenerator, OnRamp.Test' RootType 'CodeGenScript' must be the same as the ConfigType 'EntityConfig'.", ex.Message);
        }

        [Test]
        public void B120_Generator_NotInherits()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("GeneratorNotInherits.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'GeneratorNotInherits.yaml' is invalid: [Generate.Type] Type 'OnRamp.Test.Generators.NotInheritsGenerator, OnRamp.Test' does not implement CodeGeneratorBase and/or have a default parameterless constructor.", ex.Message);
        }

        [Test]
        public void B130_Generator_TemplateDoesNotExist()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("GeneratorTemplateDoesNotExist.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'GeneratorTemplateDoesNotExist.yaml' is invalid: [Generate.Template] Template 'DoesNotExist.hbs' does not exist.", ex.Message);
        }

        [Test]
        public async Task B140_Generator_RuntimeParams()
        {
            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("GeneratorRuntimeParams.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            ClassicAssert.NotNull(cg);
            ClassicAssert.NotNull(cg.Scripts?.Generators);
            ClassicAssert.AreEqual(1, cg.Scripts.Generators.Count);
            ClassicAssert.AreEqual(3, cg.Scripts.Generators[0].RuntimeParameters.Count);
            ClassicAssert.IsTrue(cg.Scripts.Generators[0].RuntimeParameters.ContainsKey("IsGenOnce"));
            ClassicAssert.AreEqual(false, cg.Scripts.Generators[0].RuntimeParameters["IsGenOnce"]);
            ClassicAssert.IsTrue(cg.Scripts.Generators[0].RuntimeParameters.ContainsKey("Company"));
            ClassicAssert.AreEqual("Xxx", cg.Scripts.Generators[0].RuntimeParameters["Company"]);
            ClassicAssert.IsTrue(cg.Scripts.Generators[0].RuntimeParameters.ContainsKey("AppName"));
            ClassicAssert.AreEqual("Yyy", cg.Scripts.Generators[0].RuntimeParameters["AppName"]);
        }

        [Test]
        public void C100_Inherits_DiffConfigType()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("InheritsDiffConfigType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'InheritsDiffConfigType.yaml' is invalid: Script 'InheritsDiffConfigType2.yaml' is invalid: [ConfigType] Inherited ConfigType 'OnRamp.Test.Config.InheritAlternateConfigType, OnRamp.Test' must be the same as root ConfigType 'OnRamp.Scripts.CodeGenScript'.", ex.Message);
        }

        [Test]
        public async Task C110_Inherits_SameConfigType()
        {
            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("InheritsSameConfigType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            ClassicAssert.NotNull(cg);
        }

        [Test]
        public void D100_Editor_TypeNotFound()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("EditorTypeNotFound.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'EditorTypeNotFound.yaml' is invalid: [EditorType] Type 'OnRamp.Scripts.NotFound' does not exist.", ex.Message);
        }

        [Test]
        public void D110_Editor_InvalidType()
        {
            var ex = Assert.ThrowsAsync<CodeGenException>(() => CodeGenerator.CreateAsync(new CodeGeneratorArgs("EditorInvalidType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            ClassicAssert.AreEqual("Script 'EditorInvalidType.yaml' is invalid: [EditorType] Type 'OnRamp.Scripts.CodeGenScript' does not implement IConfigEditor and/or have a default parameterless constructor.", ex.Message);
        }

        [Test]
        public async Task E100_Config_DoesNotExist()
        {
            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("ValidEntity.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            var ex = Assert.ThrowsAsync<CodeGenException>(() => cg.GenerateAsync("DoesNotExist.yaml"));
            ClassicAssert.AreEqual("Config 'DoesNotExist.yaml' does not exist.", ex.Message);
        }

        [Test]
        public async Task E110_Config_InvalidFileType()
        {
            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("ValidEntity.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            var ex = Assert.ThrowsAsync<CodeGenException>(() => cg.GenerateAsync("Data/InvalidFileType.xml"));
            ClassicAssert.AreEqual("Config 'Data/InvalidFileType.xml' is invalid: Stream content type is not supported.", ex.Message);
        }

        [Test]
        public async Task E120_Config_MandatoryValue()
        {
            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("ValidEntity.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            var ex = Assert.ThrowsAsync<CodeGenException>(() => cg.GenerateAsync("Data/MandatoryValue.yaml"));
            ClassicAssert.AreEqual("Config 'Data/MandatoryValue.yaml' is invalid: [Property.Name] Value is mandatory.", ex.Message);
        }

        [Test]
        public async Task E130_Config_InvalidOption()
        {
            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("ValidEntity").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            var ex = Assert.ThrowsAsync<CodeGenException>(() => cg.GenerateAsync("Data/InvalidOption.yaml"));
            ClassicAssert.AreEqual("Config 'Data/InvalidOption.yaml' is invalid: [Property(Name='Salary').Type] Value 'unknown' is invalid; valid values are: 'string', 'int', 'decimal'.", ex.Message);
        }

        [Test]
        public async Task E140_Config_NonUniqueValue()
        {
            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("ValidEntity.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            var ex = Assert.ThrowsAsync<CodeGenException>(() => cg.GenerateAsync("Data/NonUniqueValue.yaml"));
            ClassicAssert.AreEqual("Config 'Data/NonUniqueValue.yaml' is invalid: [Property(Name='Amount').Name] Value 'Amount' is not unique.", ex.Message);
        }

        [Test]
        public async Task F100_Generate_CreateAll()
        {
            if (Directory.Exists("F100"))
                Directory.Delete("F100", true);

            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("ValidEntity.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly).AddParameter("Directory", "F100").AddParameter("AppName", "Zzz"));
            var stats = await cg .GenerateAsync("Data/ValidEntity.yaml");

            ClassicAssert.NotNull(stats);
            ClassicAssert.AreEqual(4, stats.CreatedCount);
            ClassicAssert.AreEqual(0, stats.UpdatedCount);
            ClassicAssert.AreEqual(0, stats.NotChangedCount);
            ClassicAssert.AreEqual(4, stats.LinesOfCodeCount);
            ClassicAssert.NotNull(stats.ElapsedMilliseconds);

            ClassicAssert.IsTrue(Directory.Exists("F100"));
            ClassicAssert.AreEqual(4, Directory.GetFiles("F100").Length);

            ClassicAssert.IsTrue(File.Exists("F100/Person.txt"));
            ClassicAssert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("F100/Person.txt"));

            ClassicAssert.IsTrue(File.Exists("F100/Name.txt"));
            ClassicAssert.AreEqual("Name: Person.Name, Type: string", File.ReadAllText("F100/Name.txt"));

            ClassicAssert.IsTrue(File.Exists("F100/Age.txt"));
            ClassicAssert.AreEqual("Name: Person.Age, Type: int", File.ReadAllText("F100/Age.txt"));

            ClassicAssert.IsTrue(File.Exists("F100/Salary.txt"));
            ClassicAssert.AreEqual("Name: Person.Salary, Type: decimal?", File.ReadAllText("F100/Salary.txt"));
        }

        [Test]
        public async Task F110_Generate_Mix()
        {
            if (Directory.Exists("F110"))
                Directory.Delete("F110", true);

            Directory.CreateDirectory("F110");
            File.WriteAllText("F110/Person.txt", "Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary");
            File.WriteAllText("F110/Name.txt", "Name: Person.Name, Type: xxx");

            var cg = await CodeGenerator.CreateAsync(CodeGeneratorArgs.Create<CodeGeneratorTest>("ValidEntity.yaml").AddParameter("Directory", "F110").AddParameter("AppName", "Zzz"));
            var stats = await cg .GenerateAsync("Data/ValidEntity.yaml");

            ClassicAssert.NotNull(stats);
            ClassicAssert.AreEqual(2, stats.CreatedCount);
            ClassicAssert.AreEqual(1, stats.UpdatedCount);
            ClassicAssert.AreEqual(1, stats.NotChangedCount);
            ClassicAssert.AreEqual(4, stats.LinesOfCodeCount);
            ClassicAssert.NotNull(stats.ElapsedMilliseconds);

            ClassicAssert.IsTrue(Directory.Exists("F110"));
            ClassicAssert.AreEqual(4, Directory.GetFiles("F110").Length);

            ClassicAssert.IsTrue(File.Exists("F110/Person.txt"));
            ClassicAssert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("F110/Person.txt"));

            ClassicAssert.IsTrue(File.Exists("F110/Name.txt"));
            ClassicAssert.AreEqual("Name: Person.Name, Type: string", File.ReadAllText("F110/Name.txt"));

            ClassicAssert.IsTrue(File.Exists("F110/Age.txt"));
            ClassicAssert.AreEqual("Name: Person.Age, Type: int", File.ReadAllText("F110/Age.txt"));

            ClassicAssert.IsTrue(File.Exists("F110/Salary.txt"));
            ClassicAssert.AreEqual("Name: Person.Salary, Type: decimal?", File.ReadAllText("F110/Salary.txt"));
        }

        [Test]
        public async Task F120_Generate_Simulation()
        {
            if (Directory.Exists("F120"))
                Directory.Delete("F120", true);

            Directory.CreateDirectory("F120");
            File.WriteAllText("F120/Person.txt", "Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary");
            File.WriteAllText("F120/Name.txt", "Name: Person.Name, Type: xxx");

            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("ValidEntity.yaml") { IsSimulation = true }.AddAssembly(typeof(CodeGeneratorTest).Assembly).AddParameter("Directory", "F120").AddParameter("AppName", "Zzz"));
            var stats = await cg.GenerateAsync("Data/ValidEntity.yaml");

            ClassicAssert.NotNull(stats);
            ClassicAssert.AreEqual(2, stats.CreatedCount);
            ClassicAssert.AreEqual(1, stats.UpdatedCount);
            ClassicAssert.AreEqual(1, stats.NotChangedCount);
            ClassicAssert.AreEqual(4, stats.LinesOfCodeCount);
            ClassicAssert.NotNull(stats.ElapsedMilliseconds);

            ClassicAssert.IsTrue(Directory.Exists("F120"));
            ClassicAssert.AreEqual(2, Directory.GetFiles("F120").Length);

            ClassicAssert.IsTrue(File.Exists("F120/Person.txt"));
            ClassicAssert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("F120/Person.txt"));

            ClassicAssert.IsTrue(File.Exists("F120/Name.txt"));
            ClassicAssert.AreEqual("Name: Person.Name, Type: xxx", File.ReadAllText("F120/Name.txt"));

            ClassicAssert.IsFalse(File.Exists("F120/Age.txt"));
            ClassicAssert.IsFalse(File.Exists("F120/Salary.txt"));
        }

        [Test]
        public async Task F130_Generate_WithConfigEditor()
        {
            if (Directory.Exists("F130"))
                Directory.Delete("F130", true);

            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("ValidEntityWithConfigEditor.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly).AddParameter("Directory", "F130"));
            var stats = await cg.GenerateAsync("Data/ValidEntity.yaml");

            ClassicAssert.NotNull(stats);
            ClassicAssert.AreEqual(1, stats.CreatedCount);
            ClassicAssert.AreEqual(0, stats.UpdatedCount);
            ClassicAssert.AreEqual(0, stats.NotChangedCount);
            ClassicAssert.AreEqual(1, stats.LinesOfCodeCount);
            ClassicAssert.NotNull(stats.ElapsedMilliseconds);

            ClassicAssert.IsTrue(Directory.Exists("F130"));
            ClassicAssert.AreEqual(1, Directory.GetFiles("F130").Length);

            ClassicAssert.IsTrue(File.Exists("F130/PERSON.txt"));
            ClassicAssert.AreEqual("Name: PERSON, CompanyName: Xxx, AppName: Yyy, Properties: Name, Age, Salary", File.ReadAllText("F130/PERSON.txt"));
        }

        [Test]
        public async Task F140_Generate_ExpectNoChanges_Error()
        {
            if (Directory.Exists("F140"))
                Directory.Delete("F140", true);

            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("ValidEntity.yaml") { ExpectNoChanges = true }.AddAssembly(typeof(CodeGeneratorTest).Assembly).AddParameter("Directory", "F140"));
            var ex = Assert.ThrowsAsync<CodeGenChangesFoundException>(() => cg.GenerateAsync("Data/ValidEntity.yaml"));
            ClassicAssert.IsTrue(ex.Message.EndsWith("Person.txt' would be created as a result of the code generation."));
        }

        [Test]
        public async Task F150_Generate_ExpectNoChanges_Success()
        {
            if (Directory.Exists("F150"))
                Directory.Delete("F150", true);

            Directory.CreateDirectory("F150");
            File.WriteAllText("F150/Person.txt", "Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary");
            File.WriteAllText("F150/Name.txt", "Name: Person.Name, Type: string");
            File.WriteAllText("F150/Age.txt", "Name: Person.Age, Type: int");
            File.WriteAllText("F150/Salary.txt", "Name: Person.Salary, Type: decimal?");

            var cg = await CodeGenerator.CreateAsync(new CodeGeneratorArgs("ValidEntity.yaml") { ExpectNoChanges = true }.AddAssembly(typeof(CodeGeneratorTest).Assembly).AddParameter("Directory", "F150").AddParameter("AppName", "Zzz"));
            var stats = await cg .GenerateAsync("Data/ValidEntity.yaml");

            ClassicAssert.NotNull(stats);
            ClassicAssert.AreEqual(0, stats.CreatedCount);
            ClassicAssert.AreEqual(0, stats.UpdatedCount);
            ClassicAssert.AreEqual(4, stats.NotChangedCount);
            ClassicAssert.AreEqual(4, stats.LinesOfCodeCount);
            ClassicAssert.NotNull(stats.ElapsedMilliseconds);

            ClassicAssert.IsTrue(File.Exists("F150/Person.txt"));
            ClassicAssert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("F150/Person.txt"));

            ClassicAssert.IsTrue(File.Exists("F150/Name.txt"));
            ClassicAssert.AreEqual("Name: Person.Name, Type: string", File.ReadAllText("F150/Name.txt"));

            ClassicAssert.IsTrue(File.Exists("F150/Age.txt"));
            ClassicAssert.AreEqual("Name: Person.Age, Type: int", File.ReadAllText("F150/Age.txt"));

            ClassicAssert.IsTrue(File.Exists("F150/Salary.txt"));
            ClassicAssert.AreEqual("Name: Person.Salary, Type: decimal?", File.ReadAllText("F150/Salary.txt"));
        }
    }
}