using NuGet.Frameworks;
using System;
using Xunit;
using GmodNET.VersionTool.Core;

namespace GmodNET.VersionTool.Core.Tests
{
    public class UnitTests
    {
        [Fact]
        public void Test1()
        {
            VersionGenerator version_generator = new VersionGenerator("Test1.version.json");

            Assert.Equal("1.1.1", version_generator.FullVersion.Split('+')[0]);
        }
    }
}
