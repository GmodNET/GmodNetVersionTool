using NuGet.Frameworks;
using System;
using Xunit;
using GmodNET.VersionTool.Core;
using System.Text.RegularExpressions;
using System.Threading;

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

        [Fact]
        public void Test2()
        {
            VersionGenerator version_generator = new VersionGenerator("Test2.version.json");

            Assert.Equal("2.2.2", version_generator.FullVersion.Split('+')[0]);
        }

        [Fact]
        public void Test3()
        {
            VersionGenerator version_generator = new VersionGenerator("Test1.version.json");

            Regex codename_checker = new Regex(@"\+codename\.Test1", RegexOptions.ECMAScript);

            Assert.Matches(codename_checker, version_generator.FullVersion);
        }

        [Fact]
        public void Test4()
        {
            VersionGenerator versionGenerator = new VersionGenerator("Test2.version.json");

            Regex codename_checker = new Regex(@"codename", RegexOptions.ECMAScript);

            Assert.DoesNotMatch(codename_checker, versionGenerator.FullVersion);
        }
    }
}
