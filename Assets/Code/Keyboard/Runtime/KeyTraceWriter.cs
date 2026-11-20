using System;
using System.IO;
using UnityEngine;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>Appends timestamped lines to a text file, flushed at once, so the file is complete even when the editor hangs or stops. Debug aid.</summary>
    public sealed class KeyTraceWriter : IDisposable
    {
        readonly StreamWriter writer;

        public string FilePath { get; }

        public KeyTraceWriter(string filePath)
        {
            FilePath = filePath;
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            writer = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) { AutoFlush = true };
        }

        public void Log(string message) =>
            writer.WriteLine($"f={Time.frameCount} t={Time.realtimeSinceStartup:0.000} {message}");

        public void Dispose() => writer.Dispose();
    }
}
