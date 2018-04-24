using System.Linq;
using FluentAssertions;
using GitCommands.Git;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitCommandsTests
{
    public class LocalGitRefListItemTest
    {
        [TestFixture]
        public class LineParsing
        {
            [Test]
            public void Item_should_parse_object_name_from_third_column()
            {
                var item = CreateItem();

                item.Guid.Should().Be("943d230ba465d86c3ad2cd00f7e8c508d144d9a5");
            }

            [Test]
            public void Item_should_parse_object_name_from_fourth_column()
            {
                var item = CreateItem();

                item.CompleteName.Should().Be("refs/tags/0.90");
            }
        }

        [TestFixture]
        public class IntegrationWithGitRef
        {
            [Test]
            [TestCase("refs/heads/trunk", true)]
            [TestCase("/refs/heads/trunk", false)]
            [TestCase("refs/tags/0.90", false)]
            [TestCase("refs/remotes/origin/master", false)]
            public void Item_should_recognize_branches(string refName, bool isBranch)
            {
                var item = CreateItem(refName);

                item.IsHead.Should().Be(isBranch);
            }

            [Test]
            [TestCase("refs/heads/trunk", "trunk")]
            [TestCase("refs/heads/feature/magic", "feature/magic")]
            public void Item_should_return_friendly_names_for_branches(string refName, string expectedName)
            {
                var item = CreateItem(refName);

                item.Name.Should().Be(expectedName);
            }

            [Test]
            [TestCase("refs/heads/trunk", false)]
            [TestCase("refs/tags/0.90", true)]
            [TestCase("!refs/tags/0.90", false)]
            [TestCase("refs/remotes/origin/master", false)]
            public void Item_should_recognize_tags(string refName, bool isTag)
            {
                var item = CreateItem(refName);

                item.IsTag.Should().Be(isTag);
            }

            [Test]
            [TestCase("refs/tags/0.90", "0.90")]
            public void Item_should_return_friendly_names_for_tags(string refName, string expectedName)
            {
                var item = CreateItem(refName);

                item.Name.Should().Be(expectedName);
            }
        }

        private static IGitRef CreateItem(string refName = "refs/tags/0.90")
        {
            return CreateGitRef(@"1229370351 +0100 943d230ba465d86c3ad2cd00f7e8c508d144d9a5 " + refName);
        }

        private static IGitRef CreateGitRef(string output)
        {
            return new LocalGitRefList(new GitCommandResult(output)).AllGitRefs.Single();
        }
    }
}