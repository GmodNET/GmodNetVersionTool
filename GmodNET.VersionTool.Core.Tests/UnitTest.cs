using NuGet.Frameworks;
using System;
using Xunit;
using GmodNET.VersionTool.Core;

namespace GmodNET.VersionTool.Core.Tests
{
    public class UnitTest
    {
        [Fact]
        public void FullVersionTest1()
        {
            VersionGenerator version_generator = new VersionGenerator("Test1.version.json");
        }
    }
}