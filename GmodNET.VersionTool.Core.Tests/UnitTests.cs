using NuGet.Frameworks;
using System;
using Xunit;
using GmodNET.VersionTool.Core;
using System.Text.RegularExpressions;
using System.Threading;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Semver;

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

        [Fact]
        public void CommitHashTest()
        {
            using Repository repo = new Repository("../../../../");

            VersionGenerator versionGenerator = new VersionGenerator("Test1.version.json");

            Assert.Equal(repo.Head.Tip.Sha.Substring(0, 7), versionGenerator.CommitHash);
        }

        [Fact]
        public void BranchTest()
        {
            using Repository repo = new Repository("../../../../");

            VersionGenerator versionGenerator = new VersionGenerator("Test1.version.json");

            Assert.Equal(repo.Head.FriendlyName, versionGenerator.BranchName);
        }

        [Fact]
        public void SemVerCompatibilityTest()
        {
            VersionGenerator a = new VersionGenerator("Test1.version.json");
            VersionGenerator b = new VersionGenerator("Test2.version.json");
            VersionGenerator c = new VersionGenerator("Test3.version.json");

            Assert.True(SemVersion.TryParse(a.FullVersion, out _, true) && SemVersion.TryParse(b.FullVersion, out _, true) 
                && SemVersion.TryParse(c.FullVersion, out _, true));
        }

        [Fact]
        public void Test5()
        {
            VersionGenerator versionGenerator = new VersionGenerator("Test1.version.json");

            using Repository repo = new Repository("../../../../");

            string expected_version = "1.1.1+codename.Test1.head." + new Regex(@"[^0-9A-Za-z-]+", RegexOptions.ECMAScript).Replace(repo.Head.FriendlyName, "-")
                + ".commit." + repo.Head.Tip.Sha.Substring(0, 7);

            Assert.Equal(expected_version, versionGenerator.FullVersion);
        }

        [Fact]
        public void Test6()
        {
            VersionGenerator versionGenerator = new VersionGenerator("Test2.version.json");

            using Repository repo = new Repository("../../../../");

            string expected_version = "2.2.2+head." + new Regex(@"[^0-9A-Za-z-]+", RegexOptions.ECMAScript).Replace(repo.Head.FriendlyName, "-")
                + ".commit." + repo.Head.Tip.Sha.Substring(0, 7);

            Assert.Equal(expected_version, versionGenerator.FullVersion);
        }

        [Fact]
        public void Test7()
        {
            VersionGenerator versionGenerator = new VersionGenerator("Test3.version.json");

            using Repository repo = new Repository("../../../../");

            DateTime time = repo.Head.Tip.Committer.When.UtcDateTime;

            string head_name_normalized = new Regex(@"[^0-9A-Za-z-]+", RegexOptions.ECMAScript).Replace(repo.Head.FriendlyName, "-");

            string expected_string = "3.0.2-alpha.1." + time.Year + "." + time.Month + "." + time.Day + "."
                + time.Hour + "." + time.Minute + "." + time.Second + "." + head_name_normalized
                + "+codename.Test3.head." + head_name_normalized + ".commit." + repo.Head.Tip.Sha.Substring(0, 7) + ".bugfix";

            Assert.Equal(expected_string, versionGenerator.FullVersion);
        }

        [Fact]
        public void Test8()
        {
            VersionGenerator versionGenerator = new VersionGenerator("Test1.version.json");

            Assert.Equal("1.1.1", versionGenerator.VersionWithoutBuildData);
        }

        [Fact]
        public void Test9()
        {
            VersionGenerator versionGenerator = new VersionGenerator("Test2.version.json");

            Assert.Equal("2.2.2", versionGenerator.VersionWithoutBuildData);
        }

        [Fact]
        public void Test10()
        {
            VersionGenerator versionGenerator = new VersionGenerator("Test3.version.json");

            using Repository repo = new Repository("../../../../");

            DateTime time = repo.Head.Tip.Committer.When.UtcDateTime;

            string normalized_head_name = new Regex(@"[^0-9A-Za-z-]+", RegexOptions.ECMAScript).Replace(repo.Head.FriendlyName, "-");

            string expected_version = "3.0.2-alpha.1." + time.Year + "." + time.Month + "." + time.Day + "."
                + time.Hour + "." + time.Minute + "." + time.Second + "." + normalized_head_name;

            Assert.Equal(expected_version, versionGenerator.VersionWithoutBuildData);
        }
    }
}
