﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CommonTestUtils.TestRepository;
using CommonTestUtils.TestRepository.WellKnown;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs.CommitDialog
{
    [Apartment(ApartmentState.STA)]
    public class FormCommitTests
    {
        private ITestRepository _referenceRepository;
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            _referenceRepository = new TestRepository(new ReferenceRepositoryData());
            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [TearDown]
        public void TearDown()
        {
            _referenceRepository?.Dispose();
            _commands = null;
        }

        [Test]
        public void PreserveCommitMessageOnReopen()
        {
            var generatedCommitMessage = Guid.NewGuid().ToString();

            RunFormCommitTest(formCommit =>
            {
                Assert.IsEmpty(formCommit.GetTestAccessor().Message.Text);
                formCommit.GetTestAccessor().Message.Text = generatedCommitMessage;
            });

            RunFormCommitTest(formCommit =>
            {
                Assert.AreEqual(generatedCommitMessage, formCommit.GetTestAccessor().Message.Text);
            });
        }

        [TestCase(CommitKind.Fixup)]
        [TestCase(CommitKind.Squash)]
        public void DoNotPreserveCommitMessageOnReopenFromSpecialCommit(CommitKind commitKind)
        {
            var generatedCommitMessage = Guid.NewGuid().ToString();

            RunFormCommitTest(
                formCommit =>
                {
                    string prefix = commitKind.ToString().ToLowerInvariant();
                    Assert.AreEqual($"{prefix}! A commit message", formCommit.GetTestAccessor().Message.Text);
                    formCommit.GetTestAccessor().Message.Text = generatedCommitMessage;
                },
                commitKind);

            RunFormCommitTest(formCommit =>
            {
                Assert.IsEmpty(formCommit.GetTestAccessor().Message.Text);
            });
        }

        [Test]
        public void SelectMessageFromHistory()
        {
            var generatedCommitMessage = Guid.NewGuid().ToString();

            RunFormCommitTest(formCommit =>
            {
                var commitMessageToolStripMenuItem = formCommit.GetTestAccessor().CommitMessageToolStripMenuItem;

                // Verify the message appears correctly
                commitMessageToolStripMenuItem.ShowDropDown();
                Assert.AreEqual("A commit message", commitMessageToolStripMenuItem.DropDownItems[0].Text);

                // Verify the message is selected correctly
                commitMessageToolStripMenuItem.DropDownItems[0].PerformClick();
                Assert.AreEqual("A commit message", formCommit.GetTestAccessor().Message.Text);
            });
        }

        private void RunFormCommitTest(Action<FormCommit> testDriver, CommitKind commitKind = CommitKind.Normal)
        {
            RunFormCommitTest(
                formCommit =>
                {
                    testDriver(formCommit);
                    return Task.CompletedTask;
                },
                commitKind);
        }

        private void RunFormCommitTest(Func<FormCommit, Task> testDriverAsync, CommitKind commitKind = CommitKind.Normal)
        {
            UITest.RunForm(
                () =>
                {
                    switch (commitKind)
                    {
                    case CommitKind.Normal:
                        Assert.True(_commands.StartCommitDialog());
                        break;

                    case CommitKind.Squash:
                        Assert.True(_commands.StartSquashCommitDialog(owner: null, _referenceRepository.Module.GetRevision("HEAD")));
                        break;

                    case CommitKind.Fixup:
                        Assert.True(_commands.StartFixupCommitDialog(owner: null, _referenceRepository.Module.GetRevision("HEAD")));
                        break;

                    default:
                        throw new ArgumentException($"Unsupported commit kind: {commitKind}", nameof(commitKind));
                    }
                },
                testDriverAsync);
        }
    }
}
