﻿using ApprovalTests;
using CommonTestUtils.TestRepository;
using GitCommands;
using NUnit.Framework;

namespace GitCommandIntegrationTests
{
    /// <summary>
    /// Three simple test cases for git commands copypasted from GitModule.GetRefs.
    /// They are not intended to be fit for any purpose except demonstration of test execution.
    /// </summary>
    [TestFixture]
    public class PoCUsingLibGit2SharpImplementation
    {
        private const string TestRepoName = "poc";

        [Test]
        public void Listing_both_tags_and_branches_should_list_all_tags_and_branches_with_hash_and_refname_columns()
        {
            // incompatibility with command line git: missing 2 lines of output (dereferenced annotated tags)
            RunGitCommandOnTestRepo(tags: true, branches: true);
        }

        [Test]
        public void Listing_tags_should_list_all_tags_with_hash_and_refname_columns()
        {
            RunGitCommandOnTestRepo(tags: true, branches: false);
        }

        [Test]
        public void Listing_branches_should_list_all_branches_with_hash_and_refname_columns()
        {
            // incompatibility with command line git: ordering of branches
            RunGitCommandOnTestRepo(tags: false, branches: true);
        }

        private static void RunGitCommandOnTestRepo(bool tags, bool branches)
        {
            using (ITestRepositoryData tempDir = new ZippedTestRepoDirectory(TestRepoName))
            {
                var gitModule = new GitModule(tempDir.ContentPath);
                var output = new AlternativeImplementationOfGetRefsGitCommandUsingLibGit2SharpForExample(gitModule, tags: tags, branches: branches).Execute();

                Approvals.VerifyAll(output, r => r.Guid + " " + r.CompleteName);
            }
        }
    }
}
