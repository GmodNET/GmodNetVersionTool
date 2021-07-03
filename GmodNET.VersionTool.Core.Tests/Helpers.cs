using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GmodNET.VersionTool.Core.Tests.Helpers
{
    public class TempVersionFileProvider : IDisposable
    {
        string file_path;
        bool was_disposed;

        public string FilePath => this.file_path;

        public TempVersionFileProvider()
        {
            file_path = Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";
            was_disposed = false;
        }

        ~TempVersionFileProvider()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!was_disposed)
            {
                if (File.Exists(file_path))
                {
                    File.Delete(file_path);
                }
                was_disposed = true;
            }
        }
    }
}
