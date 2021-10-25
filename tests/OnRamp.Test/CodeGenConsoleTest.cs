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
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("--help");
            Assert.AreEqual(0, r);
        }

        [Test]
        public async Task A110_HelpSupportedOptions()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>(options: SupportedOptions.IsSimulation | SupportedOptions.ExpectNoChanges);
            var r = await c.RunAsync("--help");
            Assert.AreEqual(0, r);
        }

        [Test]
        public async Task A120_InvalidOptionScript()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-s");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A130_InvalidOptionConfig()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-c");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A140_InvalidOptionDirectory()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-d ../../Bad");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A150_InvalidOptionAssembly()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-a NotExists");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A160_InvalidOptionConnectionString()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-db");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A170_InvalidOptionScriptSupportedOptions()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>(options: SupportedOptions.IsSimulation | SupportedOptions.ExpectNoChanges);
            var r = await c.RunAsync("-c ValidEntity.yaml");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A200_CodeGenException()
        {
            var a = CodeGeneratorArgs.Create<CodeGeneratorTest>("ValidEntity.yaml");
            var c = CodeGenConsole.Create<CodeGenConsoleTest>(a);
            var r = await c.RunAsync();
            Assert.AreEqual(2, r);
        }

        [Test]
        public async Task A300_SuccessWithNoCmdLineArgs()
        {
            if (Directory.Exists("XA300"))
                Directory.Delete("XA300", true);

            var a = CodeGeneratorArgs.Create<CodeGeneratorTest>("ValidEntity.yaml", "Data/ValidEntity.yaml").AddParameter("Directory", "XA300").AddParameter("AppName", "Zzz");
            var c = CodeGenConsole.Create<CodeGenConsoleTest>(a);
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

            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
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

            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-s ValidEntity.yaml -c Data/ValidEntity.yaml -enc -p Directory=XA400 -p AppName=Zzz -a \"OnRamp.Test, Version=1.2.3.0, Culture=neutral, PublicKeyToken=null\"");
            Assert.AreEqual(3, r);
        }

        [Test]
        public void B100_Exe_HelpDefaultOptions()
        {
            var (exitCode, stdOut, stdErr) = ExecuteCommandLine("--help");
            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(File.ReadAllText(Path.Combine("Expected", "HelpDefaultOptions.txt")), stdOut);
            Assert.AreEqual(string.Empty, stdErr);
        }

        [Test]
        public void B110_Exe_InvalidOptionScript()
        {
            var (exitCode, stdOut, stdErr) = ExecuteCommandLine("-s");
            Assert.AreEqual(1, exitCode);
            Assert.AreEqual("Specify --help for a list of available options and commands.\r\n", stdOut);
            Assert.AreEqual("Missing value for option 's'\r\n\r\n", stdErr);
        }

        [Test]
        public void B120_Exe_InvalidOptionAssembly()
        {
            var (exitCode, stdOut, stdErr) = ExecuteCommandLine("-a NotExists");
            Assert.AreEqual(1, exitCode);
            Assert.AreEqual("Specify --help for a list of available options and commands.\r\n", stdOut);
            Assert.AreEqual("The specified assembly 'NotExists' is invalid: Could not load file or assembly 'NotExists, Culture=neutral, PublicKeyToken=null'. The system cannot find the file specified.\r\n", stdErr);
        }

        [Test]
        public void B200_Exe_CodeGenException()
        {
            var (exitCode, stdOut, stdErr) = ExecuteCommandLine("-s ValidEntity.yaml");
            Assert.AreEqual(2, exitCode);
            Assert.AreEqual(File.ReadAllText(Path.Combine("Expected", "CodeGenException.txt")), ScrubText(stdOut));
            Assert.AreEqual("Script 'ValidEntity.yaml' does not exist.\r\n\r\n", stdErr);
        }

        [Test]
        public void B300_Exe_Success()
        {
            if (Directory.Exists("XB300"))
                Directory.Delete("XB300", true);

            var (exitCode, stdOut, stdErr) = ExecuteCommandLine("-s ValidEntity.yaml -c Data/ValidEntity.yaml -p Directory=XB300 -p AppName=Zzz -a OnRamp.Test.dll");
            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(File.ReadAllText(Path.Combine("Expected", "Success.txt")), ScrubText(stdOut));
            Assert.AreEqual(string.Empty, stdErr);

            Assert.IsTrue(Directory.Exists("XB300"));
            Assert.AreEqual(4, Directory.GetFiles("XB300").Length);

            Assert.IsTrue(File.Exists("XB300/Person.txt"));
            Assert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("XB300/Person.txt"));

            Assert.IsTrue(File.Exists("XB300/Name.txt"));
            Assert.AreEqual("Name: Person.Name, Type: string", File.ReadAllText("XB300/Name.txt"));

            Assert.IsTrue(File.Exists("XB300/Age.txt"));
            Assert.AreEqual("Name: Person.Age, Type: int", File.ReadAllText("XB300/Age.txt"));

            Assert.IsTrue(File.Exists("XB300/Salary.txt"));
            Assert.AreEqual("Name: Person.Salary, Type: decimal?", File.ReadAllText("XB300/Salary.txt"));
        }

        [Test]
        public void B400_Exe_ErrorExpectNoChanges()
        {
            if (Directory.Exists("XB400"))
                Directory.Delete("XB400", true);

            var (exitCode, stdOut, stdErr) = ExecuteCommandLine("-s ValidEntity.yaml -c Data/ValidEntity.yaml -enc -p Directory=XB400 -p AppName=Zzz -a OnRamp.Test.dll");
            Assert.AreEqual(3, exitCode);
            Assert.AreEqual(File.ReadAllText(Path.Combine("Expected", "ErrorExpectNoChanges.txt")), ScrubText(stdOut));
            Assert.AreEqual("File 'xxx\\XB400\\Person.txt' would be created as a result of the code generation.\r\n\r\n", ScrubText(stdErr));
        }

        /// <summary>
        /// Execute command line directly.
        /// </summary>
        private (int exitCode, string stdOut, string stdErr) ExecuteCommandLine(string args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Assert.Inconclusive("This test is *only* supported on the Windows platform; otherwise, the Process.Start results in the following error: A fatal error was encountered. The library 'libhostpolicy.so' required to execute the application was not found in '/home/runner/.dotnet'.");

            var process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            process.StartInfo.FileName = "OnRamp";
            process.StartInfo.Arguments = args;
            process.Start();

            var reader = process.StandardOutput;
            var stdOut = new StringBuilder();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                TestContext.WriteLine(line);
                stdOut.AppendLine(line);
            }

            reader = process.StandardError;
            var stdErr = new StringBuilder();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                TestContext.Error.WriteLine(line);
                stdErr.AppendLine(line);
            }

            process.WaitForExit();

            return (process.ExitCode, stdOut.ToString(), stdErr.ToString());
        }

        /// <summary>
        /// Scrub the text for data that may change.
        /// </summary>
        private string ScrubText(string text)
        {
            // Scrub file path.
            text = text.Replace(Environment.CurrentDirectory, "xxx");

            // Scrub version.
            var i = text.IndexOf("OnRamp [v");
            if (i >= 0)
            {
                for (var j = i + 1; j <= i + 25; j++)
                {
                    if (text[j] == ']')
                    {
                        text = text.Replace(text[i..j], "OnRamp [vX.X.X");
                        break;
                    }
                }
            }

            // Scrub elapsed.
            i = text.IndexOf("ms, ");
            if (i < 0)
                return text;

            for (var j = i - 1; j >= 0; j--)
            {
                if (text[j] == '[')
                    return text.Replace($"[{text[(j + 1)..i]}ms, ", "[XXXms, ");
            }

            return text;
        }
    }
}