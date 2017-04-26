using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Config;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.CommandLine.Tests
{
	public class FileTests : TestBase
	{
		public CogProject GetDefaultProject()
		{
			var spanFactory = new ShapeSpanFactory();
			var segmentPool = new SegmentPool();
			return VerbBase.GetProjectFromResource(spanFactory, segmentPool);
		}

		public CogProject GetTestProject()
		{
			CogProject project = GetDefaultProject();
			project.FeatureSystem.Add(new SymbolicFeature("Test", new FeatureSymbol("Test1"), new FeatureSymbol("Test2")));
			return project;
		}

		public void SaveProject(CogProject project, string filename)
		{
			ConfigManager.Save(project, filename);
		}

		public void CheckDoesNotHaveTestFeature(CogProject project)
		{
			Assert.That(project.FeatureSystem.ContainsFeature("Test"), Is.False);
		}

		public void CheckHasTestFeature(CogProject project)
		{
			Assert.That(project.FeatureSystem.ContainsFeature("Test"), Is.True);
		}

		// We may not ever use this function; if so, we can delete it later.
		public void CheckIsDefaultProject(CogProject project)
		{
			Assert.That(project, Is.Not.Null);
			Assert.That(project.Varieties, Is.Empty);
			Assert.That(project.FeatureSystem, Is.Not.Empty);
			Assert.That(project.FeatureSystem.Count, Is.EqualTo(17));
			Assert.That(project.FeatureSystem.Select(feat => feat.ToString()).Take(5),
				Is.EquivalentTo(new[] {"Place", "Manner", "Syllabic", "Voice", "Nasal"}));
			IEnumerable<string> consonantSample = project.Segmenter.Consonants.Select(symbol => symbol.StrRep).Skip(20).Take(20);
			string joinedSample = string.Join("", consonantSample);
			Assert.That(joinedSample, Is.EqualTo("zçðħŋᵑɓɕɖɗɟɠɡɢɣɥɦɧɫɬ"));
		}

		// TODO: These two tests are extremely similar to each other. If it's worth the time,
		// they could be refactored into a setup/teardown fixture style, or a TestScenario style.
		[Test]
		public void SetupProject_WithFilename_ShouldLoadThatFile()
		{
			CogProject project = GetTestProject();
			string filename = Path.GetTempFileName();
			try
			{
				SaveProject(project, filename);
				var options = new VerbBase {ConfigFilename = filename};
				options.SetupProject();
				CheckHasTestFeature(options.Project);
				Assert.That(options.Project.FeatureSystem.Count, Is.EqualTo(18));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void SetupProject_WithXmlString_ShouldLoadThatXml()
		{
			CogProject project = GetTestProject();
			string filename = Path.GetTempFileName();
			try
			{
				SaveProject(project, filename);
				string xmlString = File.ReadAllText(filename, Encoding.UTF8);
				var options = new VerbBase {ConfigData = xmlString};
				options.SetupProject();
				CheckHasTestFeature(options.Project);
				Assert.That(options.Project.FeatureSystem.Count, Is.EqualTo(18));
			}
			finally
			{
				File.Delete(filename);
			}
		}

		[Test]
		public void SetupProject_WithNoConfigFile_ShouldLoadDefaultProject()
		{
			var options = new VerbBase();
			options.SetupProject();
			CheckDoesNotHaveTestFeature(options.Project);
			Assert.That(options.Project.FeatureSystem.Count, Is.EqualTo(17));
		}
	}
}
