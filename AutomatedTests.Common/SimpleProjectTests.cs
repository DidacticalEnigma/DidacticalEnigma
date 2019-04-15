using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Project;
using NUnit.Framework;
using Optional;

namespace AutomatedTests
{
    class ProjectApiInvariantsTester<TProject> : IDisposable
        where TProject : IProject
    {
        private TProject Project { get; }

        public ProjectApiInvariantsTester(TProject project)
        {
            this.Project = project;
            InvariantAssert();
            project.TranslationChanged += EventTester;
            InvariantAssert();
        }

        public T WithProject<T>(Func<TProject, T> action)
        {
            InvariantAssert();
            var result = action(Project);
            InvariantAssert();
            return result;
        }

        public void WithProject(Action<TProject> action)
        {
            _ = WithProject(p =>
            {
                action(p);
                return 0;
            });
        }

        private void EventTester(object sender, TranslationChangedEventArgs args)
        {
            Assert.AreSame(Project, sender);
            Assert.IsNotNull(args.Context);
            Assert.IsNotNull(args.Translation);
            Assert.AreNotEqual(Option.None<Guid>(), args.Translation.Guid);
            Assert.IsNotNull(args.Context.Children);
            Assert.IsNotNull(args.Context.ShortDescription);
            Assert.IsNotNull(args.Translation.TranslatedText);
            Assert.IsNotNull(args.Translation.OriginalText);
            Assert.IsNotNull(args.Translation.AlternativeTranslations);
            Assert.IsNotNull(args.Translation.Notes);
            Assert.IsNotNull(args.Translation.Glosses);
        }

        private void InvariantAssert()
        {
            Assert.IsNotNull(Project.Root);

        }

        private void RunTestSuite()
        {

        }

        public void Dispose()
        {
            RunTestSuite();
            Project.Dispose();
        }
    }

    [TestFixture]
    class SimpleProjectTests
    {
        [Test]
        public void Test()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "asdf.proj");
            try
            {
                File.Delete(tempPath);
                using (var tester = new ProjectApiInvariantsTester<SimpleProject>(new SimpleProject(tempPath)))
                {
                    bool called = false;
                    tester.WithProject(p => p.TranslationChanged += (sender, args) =>
                    {
                        if (args.Reason == TranslationChangedReason.InPlaceModification)
                        {
                            called = true;
                            Assert.AreNotEqual(Option.None<Guid>(), args.Translation.Guid);
                            Assert.AreEqual("aaa", args.Translation.OriginalText);
                            Assert.AreEqual("bbb", args.Translation.TranslatedText);
                        }
                    });
                    var a = tester.WithProject(p => (IEditableTranslation)p.Root.AppendEmpty());
                    Assert.False(called);
                    var result = tester.WithProject(_ => a.Modify(a.Translation.With("aaa", "bbb")));
                    Assert.True(called);
                }
            }
            finally
            {
                File.Delete(tempPath);
            }
        }
    }
}
