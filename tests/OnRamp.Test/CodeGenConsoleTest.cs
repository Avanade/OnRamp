using OnRamp.Console;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

namespace OnRamp.Test
{
    [TestFixture]
    public class CodeGenConsoleTest
    {
        [Test]
        public async Task A100_HelpDefaultOptions()
        {
            var c = new CodeGenConsole();
            var r = await c.RunAsync("--help");
            Assert.That(r, Is.EqualTo(0));
        }

        [Test]
        public async Task A110_HelpSupportedOptions()
        {
            var c = new CodeGenConsole(options: SupportedOptions.IsSimulation | SupportedOptions.ExpectNoChanges);
            var r = await c.RunAsync("--help");
            Assert.That(r, Is.EqualTo(0));
        }

        [Test]
        public async Task A120_InvalidOptionScript()
        {
            var c = new CodeGenConsole();
            var r = await c.RunAsync("-s");
            Assert.That(r, Is.EqualTo(1));
        }

        [Test]
        public async Task A130_InvalidOptionConfig()
        {
            var c = new CodeGenConsole();
            var r = await c.RunAsync("-c");
            Assert.That(r, Is.EqualTo(1));
        }

        [Test]
        public async Task A140_InvalidOptionDirectory()
        {
            var c = new CodeGenConsole();
            var r = await c.RunAsync("-d ../../Bad");
            Assert.That(r, Is.EqualTo(1));
        }

        [Test]
        public async Task A150_InvalidOptionAssembly()
        {
            var c = new CodeGenConsole();
            var r = await c.RunAsync("-a NotExists");
            Assert.That(r, Is.EqualTo(1));
        }

        [Test]
        public async Task A160_InvalidOptionConnectionString()
        {
            var c = new CodeGenConsole();
            var r = await c.RunAsync("-db");
            Assert.That(r, Is.EqualTo(1));
        }

        [Test]
        public async Task A170_InvalidOptionScriptSupportedOptions()
        {
            var c = new CodeGenConsole(options: SupportedOptions.IsSimulation | SupportedOptions.ExpectNoChanges);
            var r = await c.RunAsync("-c ValidEntity.yaml");
            Assert.That(r, Is.EqualTo(1));
        }

        [Test]
        public async Task A200_CodeGenException()
        {
            var a = CodeGeneratorArgs.Create<CodeGeneratorTest>("ValidEntity.yaml");
            var c = new CodeGenConsole(a);
            var r = await c.RunAsync();
            Assert.That(r, Is.EqualTo(2));
        }

        [Test]
        public async Task A300_SuccessWithNoCmdLineArgs()
        {
            if (Directory.Exists("XA300"))
                Directory.Delete("XA300", true);

            var a = CodeGeneratorArgs.Create<CodeGeneratorTest>("ValidEntity.yaml", "Data/ValidEntity.yaml").AddParameter("Directory", "XA300").AddParameter("AppName", "Zzz");
            var c = new CodeGenConsole(a);
            var r = await c.RunAsync();
            Assert.That(r, Is.EqualTo(0));

            Assert.That(Directory.Exists("XA300"), Is.True);
            Assert.That(Directory.GetFiles("XA300").Length, Is.EqualTo(4));

            Assert.That(File.Exists("XA300/Person.txt"), Is.True);
            Assert.That(File.ReadAllText("XA300/Person.txt"), Is.EqualTo("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary"));

            Assert.That(File.Exists("XA300/Name.txt"), Is.True);
            Assert.That(File.ReadAllText("XA300/Name.txt"), Is.EqualTo("Name: Person.Name, Type: string"));

            Assert.That(File.Exists("XA300/Age.txt"), Is.True);
            Assert.That(File.ReadAllText("XA300/Age.txt"), Is.EqualTo("Name: Person.Age, Type: int"));

            Assert.That(File.Exists("XA300/Salary.txt"), Is.True);
            Assert.That(File.ReadAllText("XA300/Salary.txt"), Is.EqualTo("Name: Person.Salary, Type: decimal?"));
        }

        [Test]
        public async Task A310_SuccessWithCmdLineArgs()
        {
            if (Directory.Exists("XA310"))
                Directory.Delete("XA310", true);

            var c = new CodeGenConsole();
            var r = await c.RunAsync("-s ValidEntity.yaml -c Data/ValidEntity.yaml -p Directory=XA310 -p AppName=Zzz -a \"OnRamp.Test, Version=1.2.3.0, Culture=neutral, PublicKeyToken=null\"");
            Assert.That(r, Is.EqualTo(0));

            Assert.That(Directory.Exists("XA310"), Is.True);
            Assert.That(Directory.GetFiles("XA310").Length, Is.EqualTo(4));

            Assert.That(File.Exists("XA310/Person.txt"), Is.True);
            Assert.That(File.ReadAllText("XA310/Person.txt"), Is.EqualTo("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary"));

            Assert.That(File.Exists("XA310/Name.txt"), Is.True);
            Assert.That(File.ReadAllText("XA310/Name.txt"), Is.EqualTo("Name: Person.Name, Type: string"));

            Assert.That(File.Exists("XA310/Age.txt"), Is.True);
            Assert.That(File.ReadAllText("XA310/Age.txt"), Is.EqualTo("Name: Person.Age, Type: int"));

            Assert.That(File.Exists("XA310/Salary.txt"), Is.True);
            Assert.That(File.ReadAllText("XA310/Salary.txt"), Is.EqualTo("Name: Person.Salary, Type: decimal?"));
        }

        [Test]
        public async Task A400_ErrorExpectNoChanges()
        {
            if (Directory.Exists("XA400"))
                Directory.Delete("XA400", true);

            // Run and it should fail as it cannot create.
            var c = new CodeGenConsole();
            var r = await c.RunAsync("-s ValidEntity.yaml -c Data/ValidEntity.yaml -enc -p Directory=XA400 -p AppName=Zzz -a \"OnRamp.Test, Version=1.2.3.0, Culture=neutral, PublicKeyToken=null\"");
            Assert.That(r, Is.EqualTo(3));

            // Run again and let it make changes.
            c = new CodeGenConsole();
            r = await c.RunAsync("-s ValidEntity.yaml -c Data/ValidEntity.yaml -p Directory=XA400 -p AppName=Zzz -a \"OnRamp.Test, Version=1.2.3.0, Culture=neutral, PublicKeyToken=null\"");
            Assert.That(r, Is.EqualTo(0));

            // Change the file and run again and it should fail as it cannot update.
            var files = Directory.GetFiles("XA400");
            Assert.That(files.Length, Is.Not.EqualTo(0));

            var content = File.ReadAllText(files.Last());
            File.WriteAllText(files.Last(), content + "X");

            c = new CodeGenConsole();
            r = await c.RunAsync("-s ValidEntity.yaml -c Data/ValidEntity.yaml -enc -p Directory=XA400 -p AppName=Zzz -a \"OnRamp.Test, Version=1.2.3.0, Culture=neutral, PublicKeyToken=null\"");
            Assert.That(r, Is.EqualTo(3));
        }

        [Test]
        public void C100_GetBaseExeDirectory()
        {
            var ed = Environment.CurrentDirectory;
            Assert.That(ed.Contains(Path.Combine("bin", "debug"), StringComparison.InvariantCultureIgnoreCase) || ed.Contains(Path.Combine("bin", "release"), StringComparison.InvariantCultureIgnoreCase), Is.True);

            ed = CodeGenConsole.GetBaseExeDirectory();
            Assert.That(ed.Contains(Path.Combine("bin", "debug"), StringComparison.InvariantCultureIgnoreCase) || ed.Contains(Path.Combine("bin", "release"), StringComparison.InvariantCultureIgnoreCase), Is.False);
        }
    }
}