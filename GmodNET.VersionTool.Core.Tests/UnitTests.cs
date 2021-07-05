using NuGet.Frameworks;
using System;
using Xunit;
using GmodNET.VersionTool.Core;
using System.Text.RegularExpressions;
using System.Threading;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Semver;
using System.IO;
using GmodNET.VersionTool.Core.Tests.Helpers;

namespace GmodNET.VersionTool.Core.Tests
{
    public class UnitTests
    {
        [Fact]
        public void Test1()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test1.version.json"))
            {
                VersionGenerator version_generator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                Assert.Equal("1.1.1", version_generator.FullVersion.Split('+')[0]);
            }
        }

        [Fact]
        public void Test2()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test2.version.json"))
            {
                VersionGenerator version_generator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                Assert.Equal("2.2.2", version_generator.FullVersion.Split('+')[0]);
            }
        }

        [Fact]
        public void Test3()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test1.version.json"))
            {
                VersionGenerator version_generator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                Regex codename_checker = new Regex(@"\+codename\.Test1", RegexOptions.ECMAScript);

                Assert.Matches(codename_checker, version_generator.FullVersion);
            }
        }

        [Fact]
        public void Test4()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test2.version.json"))
            {
                VersionGenerator versionGenerator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                Regex codename_checker = new Regex(@"codename", RegexOptions.ECMAScript);

                Assert.DoesNotMatch(codename_checker, versionGenerator.FullVersion);
            }
        }

        [Fact]
        public void CommitHashTest()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test1.version.json"))
            {
                using Repository repo = new Repository(tempRepo.RepoDirectory.FullName);

                VersionGenerator versionGenerator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                Assert.Equal(repo.Head.Tip.Sha, versionGenerator.CommitHash);
            }
        }

        [Fact]
        public void BranchTest()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test1.version.json"))
            {
                using Repository repo = new Repository(tempRepo.RepoDirectory.FullName);

                Branch test_branch = repo.CreateBranch("testbranch111");

                Commands.Checkout(repo, test_branch);

                VersionGenerator versionGenerator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                Assert.Equal("testbranch111", versionGenerator.BranchName);
            }
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
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test1.version.json"))
            {
                using Repository repo = new Repository(tempRepo.RepoDirectory.FullName);

                Commands.Checkout(repo, repo.CreateBranch("pup22"));

                VersionGenerator versionGenerator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                string expected_version = "1.1.1+codename.Test1.head.pup22.commit." + repo.Head.Tip.Sha;

                Assert.Equal(expected_version, versionGenerator.FullVersion);
            }
        }

        [Fact]
        public void Test6()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test2.version.json"))
            {
                using Repository repo = new Repository(tempRepo.RepoDirectory.FullName);

                Commands.Checkout(repo, repo.CreateBranch("oh333"));

                VersionGenerator versionGenerator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                string expected_version = "2.2.2+head.oh333.commit." + repo.Head.Tip.Sha;

                Assert.Equal(expected_version, versionGenerator.FullVersion);
            }
        }

        [Fact]
        public void Test7()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test3.version.json"))
            {
                using Repository repo = new Repository(tempRepo.RepoDirectory.FullName);

                Commands.Checkout(repo, repo.CreateBranch("snd5"));

                VersionGenerator versionGenerator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                DateTime time = repo.Head.Tip.Committer.When.UtcDateTime;

                string expected_string = "3.0.2-alpha.1."
                    + (new DateTimeOffset(time).ToUnixTimeSeconds() - new DateTimeOffset(new DateTime(2020, 1, 1), TimeSpan.Zero).ToUnixTimeSeconds())
                    + ".snd5+codename.Test3.head.snd5.commit." + repo.Head.Tip.Sha + ".bugfix";

                Assert.Equal(expected_string, versionGenerator.FullVersion);
            }
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
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test3.version.json"))
            {
                using Repository repo = new Repository(tempRepo.RepoDirectory.FullName);

                Commands.Checkout(repo, repo.CreateBranch("uuuu8"));

                VersionGenerator versionGenerator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                DateTime time = repo.Head.Tip.Committer.When.UtcDateTime;

                string expected_version = "3.0.2-alpha.1."
                    + (new DateTimeOffset(time).ToUnixTimeSeconds() - new DateTimeOffset(new DateTime(2020, 1, 1), TimeSpan.Zero).ToUnixTimeSeconds())
                    + ".uuuu8";

                Assert.Equal(expected_version, versionGenerator.VersionWithoutBuildData);
            }
        }

        [Fact]
        public void ThrowOnNoFileTest()
        {
            Assert.ThrowsAny<Exception>(() => { new VersionGenerator("NonExistingFile.v"); });
        }

        [Fact]
        public void IncorrectFileTest()
        {
            Assert.Throws<ArgumentException>(() => { new VersionGenerator("Test4.version.json"); });
        }

        [Fact]
        public void VersionFileIsNotInRepositoryTest()
        {
            using TempVersionFileProvider temp_version_file = new TempVersionFileProvider();
            File.Copy("Test3.version.json", temp_version_file.FilePath);
            Assert.Throws<ArgumentException>(() => { new VersionGenerator(temp_version_file.FilePath); });
        }

        [Fact]
        public void RepoHasNoCommitsTest()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test1.version.json", DateTimeOffset.Now, false))
            {
                Assert.Throws<ArgumentException>(() => new VersionGenerator(tempRepo.RepoVersionFilePath));
            }
        }

        [Fact]
        public void CommitTimeTest()
        {
            // DateTime structure which represents February 3nd, 2020. It must be 2851200 seconds since January 1st, 2020
            DateTimeOffset commit_time = new DateTimeOffset(new DateTime(2020, 2, 3), TimeSpan.Zero);
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test3.version.json", commit_time))
            {
                using Repository repo = new Repository(tempRepo.RepoDirectory.FullName);

                Commands.Checkout(repo, repo.CreateBranch("aabb"));

                VersionGenerator version_generator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                string expected_version = "3.0.2-alpha.1.2851200.aabb";

                Assert.Equal(expected_version, version_generator.VersionWithoutBuildData);
            }
        }

        [Fact]
        public void TagNameTest1()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test3.version.json"))
            {
                using Repository repo = new Repository(tempRepo.RepoDirectory.FullName);
                Commands.Checkout(repo, repo.CreateBranch("ccdd"));

                Signature sig = new Signature("Test runner", "support@gmodnet.xyz", DateTimeOffset.Now);

                repo.ApplyTag("1.2.3", sig, "Release 1.2.3");

                VersionGenerator versionGenerator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                Assert.Equal("tag 1.2.3", versionGenerator.BranchName);
            }
        }

        [Fact]
        public void TagNameTest2()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test3.version.json"))
            {
                using Repository repo = new Repository(tempRepo.RepoDirectory.FullName);
                Commands.Checkout(repo, repo.CreateBranch("ccdd"));

                Signature sig = new Signature("Test runner", "support@gmodnet.xyz", DateTimeOffset.Now);

                repo.ApplyTag("release/2.2.2", sig, "Version 2.2.2");

                VersionGenerator versionGenerator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                Assert.Equal("tag release/2.2.2", versionGenerator.BranchName);
            }
        }

        [Fact]
        public void LightTagNameTest()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test3.version.json"))
            {
                using Repository repo = new Repository(tempRepo.RepoDirectory.FullName);
                Commands.Checkout(repo, repo.CreateBranch("ccdd"));

                repo.ApplyTag("v3.2.1");

                VersionGenerator versionGenerator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                Assert.Equal("tag v3.2.1", versionGenerator.BranchName);
            }
        }

        [Fact]
        public void DetachedHeadTest()
        {
            using (TempRepoProvider tempRepo = new TempRepoProvider("Test1.version.json"))
            {
                using Repository repo = new Repository(tempRepo.RepoDirectory.FullName);
                Commands.Checkout(repo, repo.CreateBranch("bd"));

                Commit firstCommit = repo.Head.Tip;

                File.Copy("Test3.version.json", Path.Combine(tempRepo.RepoDirectory.FullName, "Test3.version.json"));

                Signature sig = new Signature("Test runner", "support@gmodnet.xyz", DateTimeOffset.Now);

                Commands.Stage(repo, "*");
                repo.Commit("Second commit", sig, sig);

                Commands.Checkout(repo, firstCommit);

                VersionGenerator versionGenerator = new VersionGenerator(tempRepo.RepoVersionFilePath);

                Assert.Equal("detached HEAD", versionGenerator.BranchName);
            }
        }
    }
}
