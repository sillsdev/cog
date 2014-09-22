using NUnit.Framework;
using SIL.Cog.Application.Services.ProjectMigrations;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application.Tests.Services.ProjectMigrations
{
	[TestFixture]
	public class ProjectMigration3Tests
	{
		private readonly ShapeSpanFactory _spanFactory = new ShapeSpanFactory();
		private FeatureSystem _featSys;
		private CogProject _project;
		private SegmentPool _segmentPool;

		[SetUp]
		public void SetUp()
		{
			_featSys = new FeatureSystem
				{
					new SymbolicFeature("place",
						new FeatureSymbol("bilabial"),
						new FeatureSymbol("labiodental"),
						new FeatureSymbol("dental"),
						new FeatureSymbol("alveolar"),
						new FeatureSymbol("retroflex"),
						new FeatureSymbol("palato-alveolar"),
						new FeatureSymbol("alveolo-palatal"),
						new FeatureSymbol("palatal"),
						new FeatureSymbol("velar"),
						new FeatureSymbol("uvular"),
						new FeatureSymbol("pharyngeal"),
						new FeatureSymbol("epiglottal"),
						new FeatureSymbol("glottal"))
				};
			_project = new CogProject(_spanFactory) {FeatureSystem = _featSys};
			_segmentPool = new SegmentPool();
		}

		[Test]
		public void ToneNumbers()
		{
			var pm = new ProjectMigration3();
			pm.Migrate(_segmentPool, _project);

			Assert.That(_project.Segmenter.Modifiers.Contains("¹"));
			Assert.That(_project.Segmenter.Modifiers.Contains("²"));
			Assert.That(_project.Segmenter.Modifiers.Contains("³"));
			Assert.That(_project.Segmenter.Modifiers.Contains("⁴"));
			Assert.That(_project.Segmenter.Modifiers.Contains("⁵"));
		}

		[Test]
		public void AlveoloPalatalConsonants()
		{
			_project.Segmenter.Consonants.Add("t", FeatureStruct.New(_featSys).Symbol("alveolar").Value);
			_project.Segmenter.Consonants.Add("d", FeatureStruct.New(_featSys).Symbol("alveolar").Value);
			_project.Segmenter.Consonants.Add("n", FeatureStruct.New(_featSys).Symbol("alveolar").Value);
			_project.Segmenter.Consonants.Add("l", FeatureStruct.New(_featSys).Symbol("alveolar").Value);

			var pm = new ProjectMigration3();
			pm.Migrate(_segmentPool, _project);

			Assert.That(_project.Segmenter.Consonants["ȶ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("alveolo-palatal").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
			Assert.That(_project.Segmenter.Consonants["ȡ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("alveolo-palatal").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
			Assert.That(_project.Segmenter.Consonants["ȵ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("alveolo-palatal").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
			Assert.That(_project.Segmenter.Consonants["ȴ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("alveolo-palatal").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
		}
	}
}
