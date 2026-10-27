using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OnRamp.Test
{
    /// <summary>
    /// Verifies argument copying, parameter typing, assembly precedence, and connection overrides.
    /// </summary>
    [TestFixture]
    public class CodeGeneratorArgsTest
    {
        /// <summary>
        /// Verifies generic construction infers the probing assembly and preserves file names.
        /// </summary>
        [Test]
        public void CreatePreservesFileNamesAndInfersAssembly()
        {
            var args = CodeGeneratorArgs.Create<CodeGeneratorArgsTest>("script.yaml", "config.json");
            Assert.That(args.ScriptFileName, Is.EqualTo("script.yaml"));
            Assert.That(args.ConfigFileName, Is.EqualTo("config.json"));
            Assert.That(args.Assemblies, Is.EqualTo(new[] { typeof(CodeGeneratorArgsTest).Assembly }));
        }

        /// <summary>
        /// Verifies assembly insertion order, duplicate elimination, and empty input handling.
        /// </summary>
        [Test]
        public void AddAssemblyPreservesPrecedenceAndDeduplicates()
        {
            var args = new CodeGeneratorArgs();
            var library = typeof(CodeGenerator).Assembly;
            var tests = typeof(CodeGeneratorArgsTest).Assembly;
            var runtime = typeof(object).Assembly;
            Assert.That(args.AddAssembly(library), Is.SameAs(args));
            Assert.That(args.AddAssembly(tests, runtime), Is.SameAs(args));
            args.AddAssembly(tests);
            args.AddAssembly(Array.Empty<Assembly>());
            args.AddAssembly((Assembly[])null);
            Assert.That(args.Assemblies, Is.EqualTo(new[] { tests, runtime, library }));
        }

        /// <summary>
        /// Verifies parameter updates retain arbitrary objects and support convertible scalar values.
        /// </summary>
        [Test]
        public void ParametersPreserveTypesAndMergeWithoutDiscardingExistingValues()
        {
            var args = new CodeGeneratorArgs();
            var payload = new object();
            Assert.That(args.AddParameter("payload", payload), Is.SameAs(args));
            args.AddParameter("count", "12").AddParameter("flag", true).AddParameter<object>("empty", null);
            Assert.That(args.AddParameters(new Dictionary<string, object> { ["count"] = 42, ["added"] = "value" }), Is.SameAs(args));
            args.AddParameters(null);

            Assert.That(args.GetParameter<object>("payload"), Is.SameAs(payload));
            Assert.That(args.GetParameter<int>("count"), Is.EqualTo(42));
            Assert.That(args.GetParameter<string>("count"), Is.EqualTo("42"));
            Assert.That(args.GetParameter<bool>("flag"), Is.True);
            Assert.That(args.GetParameter<object>("empty"), Is.Null);
            Assert.That(args.GetParameter<string>("added"), Is.EqualTo("value"));
            Assert.That(args.GetParameter<int>("absent"), Is.Zero);
            Assert.That(args.GetParameter<string>("absent"), Is.Null);
            var ex = Assert.Throws<CodeGenException>((System.Action)(() => args.GetParameter<int>("absent", true)));
            Assert.That(ex.Message, Is.EqualTo("Parameter 'absent' does not exist."));
        }

        /// <summary>
        /// Verifies invalid conversions are surfaced instead of silently returning defaults.
        /// </summary>
        [Test]
        public void ParametersRejectInvalidConversions()
        {
            var args = new CodeGeneratorArgs().AddParameter("invalid", "not-a-number").AddParameter("large", long.MaxValue);
            Assert.Throws<FormatException>((System.Action)(() => args.GetParameter<int>("invalid")));
            Assert.Throws<OverflowException>((System.Action)(() => args.GetParameter<int>("large")));
        }

        /// <summary>
        /// Verifies copying replaces stale state and independently owns mutable collections.
        /// </summary>
        [Test]
        public void CopyFromReplacesStateWithoutSharingCollections()
        {
            Func<ICodeGeneratorDbArgs, string> factory = _ => "ONRAMP_UNUSED_TEST_VARIABLE";
            var payload = new object();
            var source = CodeGeneratorArgs.Create<CodeGeneratorArgsTest>("source.yaml", "config.json")
                .AddParameter("payload", payload);
            source.OutputDirectory = new DirectoryInfo(Path.GetTempPath());
            source.Logger = NullLogger.Instance;
            source.ExpectNoChanges = true;
            source.IsSimulation = true;
            source.ConnectionStringEnvironmentVariableName = "ONRAMP_UNUSED_TEST_VARIABLE";
            source.CreateConnectionStringEnvironmentVariableName = factory;
            source.OverrideConnectionString("explicit-connection");

            var target = new CodeGeneratorArgs("old.yaml").AddAssembly(typeof(object).Assembly).AddParameter("stale", 1);
            ((ICodeGeneratorArgs)target).CopyFrom(source);

            Assert.That(target.ScriptFileName, Is.EqualTo(source.ScriptFileName));
            Assert.That(target.ConfigFileName, Is.EqualTo(source.ConfigFileName));
            Assert.That(target.OutputDirectory.FullName, Is.EqualTo(source.OutputDirectory.FullName));
            Assert.That(target.OutputDirectory, Is.Not.SameAs(source.OutputDirectory));
            Assert.That(target.Logger, Is.SameAs(source.Logger));
            Assert.That(target.ExpectNoChanges, Is.True);
            Assert.That(target.IsSimulation, Is.True);
            Assert.That(target.ConnectionString, Is.EqualTo("explicit-connection"));
            Assert.That(target.HasOverriddenConnectionString, Is.True);
            Assert.That(target.ConnectionStringEnvironmentVariableName, Is.EqualTo(source.ConnectionStringEnvironmentVariableName));
            Assert.That(target.CreateConnectionStringEnvironmentVariableName, Is.SameAs(factory));
            Assert.That(target.Assemblies, Is.EqualTo(source.Assemblies));
            Assert.That(target.Assemblies, Is.Not.SameAs(source.Assemblies));
            Assert.That(target.Parameters.Keys, Is.EqualTo(new[] { "payload" }));
            Assert.That(target.GetParameter<object>("payload"), Is.SameAs(payload));

            target.AddParameter("target-only", 2);
            target.Assemblies.Clear();
            Assert.That(source.Parameters.ContainsKey("target-only"), Is.False);
            Assert.That(source.Assemblies.Count, Is.EqualTo(1));
            target.OverrideConnectionString("second-connection");
            Assert.That(target.ConnectionString, Is.EqualTo("explicit-connection"));
        }

        /// <summary>
        /// Verifies copying empty arguments clears optional values and rejects null sources.
        /// </summary>
        [Test]
        public void CopyFromClearsOptionalStateAndRejectsNull()
        {
            var args = new CodeGeneratorArgs("old.yaml", "old.json")
            {
                OutputDirectory = new DirectoryInfo(Path.GetTempPath()),
                Logger = NullLogger.Instance,
                IsSimulation = true,
                ExpectNoChanges = true
            };
            args.CopyFrom(new CodeGeneratorArgs());
            Assert.That(args.ScriptFileName, Is.Null);
            Assert.That(args.ConfigFileName, Is.Null);
            Assert.That(args.OutputDirectory, Is.Null);
            Assert.That(args.Logger, Is.Null);
            Assert.That(args.IsSimulation, Is.False);
            Assert.That(args.ExpectNoChanges, Is.False);
            var ex = Assert.Throws<ArgumentNullException>((System.Action)(() => args.CopyFrom((CodeGeneratorArgs)null)));
            Assert.That(ex.ParamName, Is.EqualTo("args"));
        }

        /// <summary>
        /// Verifies explicit overrides win and prevent subsequent overrides or factory evaluation.
        /// </summary>
        [Test]
        public void ExplicitConnectionOverrideWinsAndIsAppliedOnlyOnce()
        {
            var args = new CodeGeneratorArgs
            {
                ConnectionString = "original",
                CreateConnectionStringEnvironmentVariableName = _ => throw new InvalidOperationException("Factory should not run.")
            };
            args.OverrideConnectionString("explicit");
            args.OverrideConnectionString("later");
            Assert.That(args.ConnectionString, Is.EqualTo("explicit"));
            Assert.That(args.HasOverriddenConnectionString, Is.True);
        }

        /// <summary>
        /// Verifies named environment variables precede factory-generated names and missing values permit retry.
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        public void ConnectionEnvironmentPrecedenceAndMissingValueRetry(bool useNamedVariable)
        {
            var variable = $"ONRAMP_TEST_{Guid.NewGuid():N}";
            var factoryVariable = $"ONRAMP_TEST_{Guid.NewGuid():N}";
            var calls = 0;
            try
            {
                var args = new CodeGeneratorArgs
                {
                    ConnectionString = "original",
                    ConnectionStringEnvironmentVariableName = variable,
                    CreateConnectionStringEnvironmentVariableName = a =>
                    {
                        Assert.That(a.ConnectionString, Is.EqualTo("original"));
                        calls++;
                        return factoryVariable;
                    }
                };
                args.OverrideConnectionString();
                Assert.That(args.ConnectionString, Is.EqualTo("original"));
                Assert.That(args.HasOverriddenConnectionString, Is.False);
                Assert.That(calls, Is.EqualTo(1));

                Environment.SetEnvironmentVariable(factoryVariable, "factory");
                if (useNamedVariable)
                    Environment.SetEnvironmentVariable(variable, "named");
                args.OverrideConnectionString("");
                Assert.That(args.ConnectionString, Is.EqualTo(useNamedVariable ? "named" : "factory"));
                Assert.That(args.HasOverriddenConnectionString, Is.True);
                Assert.That(calls, Is.EqualTo(useNamedVariable ? 1 : 2));
            }
            finally
            {
                Environment.SetEnvironmentVariable(variable, null);
                Environment.SetEnvironmentVariable(factoryVariable, null);
            }
        }

        /// <summary>
        /// Verifies absent override sources and empty factory names preserve the original connection.
        /// </summary>
        [TestCase(null)]
        [TestCase("")]
        public void EmptyConnectionFactoryNamesDoNotOverride(string name)
        {
            var args = new CodeGeneratorArgs { ConnectionString = "original" };
            args.OverrideConnectionString();
            args.CreateConnectionStringEnvironmentVariableName = _ => name;
            args.OverrideConnectionString();
            Assert.That(args.ConnectionString, Is.EqualTo("original"));
            Assert.That(args.HasOverriddenConnectionString, Is.False);
        }
    }
}
