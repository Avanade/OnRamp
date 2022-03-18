using OnRamp.Console;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Text;
using System.Runtime.InteropServices;

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
            Assert.AreEqual(0, r);
        }

        [Test]
        public async Task A110_HelpSupportedOptions()
        {
            var c = new CodeGenConsole(options: SupportedOptions.IsSimulation | SupportedOptions.ExpectNoChanges);
            var r = await c.RunAsync("--help");
            Assert.AreEqual(0, r);
        }

        [Test]
        public async Task A120_InvalidOptionScript()
        {
            var c = new CodeGenConsole();
            var r = await c.RunAsync("-s");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A130_InvalidOptionConfig()
        {
            var c = new CodeGenConsole();
            var r = await c.RunAsync("-c");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A140_InvalidOptionDirectory()
        {
            var c = new CodeGenConsole();
            var r = await c.RunAsync("-d ../../Bad");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A150_InvalidOptionAssembly()
        {
            var c = new CodeGenConsole();
            var r = await c.RunAsync("-a NotExists");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A160_InvalidOptionConnectionString()
        {
            var c = new CodeGenConsole();
            var r = await c.RunAsync("-db");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A170_InvalidOptionScriptSupportedOptions()
        {
            var c = new CodeGenConsole(options: SupportedOptions.IsSimulation | SupportedOptions.ExpectNoChanges);
            var r = await c.RunAsync("-c ValidEntity.yaml");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A200_CodeGenException()
        {
            var a = CodeGeneratorArgs.Create<CodeGeneratorTest>("ValidEntity.yaml");
            var c = new CodeGenConsole(a);
            var r = await c.RunAsync();
            Assert.AreEqual(2, r);
        }

        [Test]
        public async Task A300_SuccessWithNoCmdLineArgs()
        {
            if (Directory.Exists("XA300"))
                Directory.Delete("XA300", true);

            var a = CodeGeneratorArgs.Create<CodeGeneratorTest>("ValidEntity.yaml", "Data/ValidEntity.yaml").AddParameter("Directory", "XA300").AddParameter("AppName", "Zzz");
            var c = new CodeGenConsole(a);
            var r = await c.RunAsync();
            Assert.AreEqual(0, r);

            Assert.IsTrue(Directory.Exists("XA300"));
            Assert.AreEqual(4, Directory.GetFiles("XA300").Length);

            Assert.IsTrue(File.Exists("XA300/Person.txt"));
            Assert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("XA300/Person.txt"));

            Assert.IsTrue(File.Exists("XA300/Name.txt"));
            Assert.AreEqual("Name: Person.Name, Type: string", File.ReadAllText("XA300/Name.txt"));

            Assert.IsTrue(File.Exists("XA300/Age.txt"));
            Assert.AreEqual("Name: Person.Age, Type: int", File.ReadAllText("XA300/Age.txt"));

            Assert.IsTrue(File.Exists("XA300/Salary.txt"));
            Assert.AreEqual("Name: Person.Salary, Type: decimal?", File.ReadAllText("XA300/Salary.txt"));
        }

        [Test]
        public async Task A310_SuccessWithCmdLineArgs()
        {
            if (Directory.Exists("XA310"))
                Directory.Delete("XA310", true);

            var c = new CodeGenConsole();
            var r = await c.RunAsync("-s ValidEntity.yaml -c Data/ValidEntity.yaml -p Directory=XA310 -p AppName=Zzz -a \"OnRamp.Test, Version=1.2.3.0, Culture=neutral, PublicKeyToken=null\"");
            Assert.AreEqual(0, r);

            Assert.IsTrue(Directory.Exists("XA310"));
            Assert.AreEqual(4, Directory.GetFiles("XA310").Length);

            Assert.IsTrue(File.Exists("XA310/Person.txt"));
            Assert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("XA310/Person.txt"));

            Assert.IsTrue(File.Exists("XA310/Name.txt"));
            Assert.AreEqual("Name: Person.Name, Type: string", File.ReadAllText("XA310/Name.txt"));

            Assert.IsTrue(File.Exists("XA310/Age.txt"));
            Assert.AreEqual("Name: Person.Age, Type: int", File.ReadAllText("XA310/Age.txt"));

            Assert.IsTrue(File.Exists("XA310/Salary.txt"));
            Assert.AreEqual("Name: Person.Salary, Type: decimal?", File.ReadAllText("XA310/Salary.txt"));
        }

        [Test]
        public async Task A400_ErrorExpectNoChanges()
        {
            if (Directory.Exists("XA400"))
                Directory.Delete("XA400", true);

            var c = new CodeGenConsole();
            var r = await c.RunAsync("-s ValidEntity.yaml -c Data/ValidEntity.yaml -enc -p Directory=XA400 -p AppName=Zzz -a \"OnRamp.Test, Version=1.2.3.0, Culture=neutral, PublicKeyToken=null\"");
            Assert.AreEqual(3, r);
        }

        [Test]
        public void C100_GetBaseExeDirectory()
        {
            var ed = Environment.CurrentDirectory;
            Assert.IsTrue(ed.Contains(Path.Combine("bin", "debug"), StringComparison.InvariantCultureIgnoreCase) || ed.Contains(Path.Combine("bin", "release"), StringComparison.InvariantCultureIgnoreCase));

            ed = CodeGenConsole.GetBaseExeDirectory();
            Assert.IsFalse(ed.Contains(Path.Combine("bin", "debug"), StringComparison.InvariantCultureIgnoreCase) || ed.Contains(Path.Combine("bin", "release"), StringComparison.InvariantCultureIgnoreCase));
        }
    }
}