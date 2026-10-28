using System;
using System.IO;

namespace OnRamp.Test
{
    /// <summary>
    /// Owns a unique temporary directory and removes its files after a test.
    /// </summary>
    internal sealed class TestDirectory : IDisposable
    {
        /// <summary>
        /// Creates an isolated directory without changing the process working directory.
        /// </summary>
        public TestDirectory()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), $"OnRamp-Test-{Guid.NewGuid():N}");
            Directory.CreateDirectory(DirectoryPath);
        }

        /// <summary>
        /// Gets the absolute path owned by this test.
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        /// Removes only the temporary directory created by this instance.
        /// </summary>
        public void Dispose() => Directory.Delete(DirectoryPath, true);
    }
}
