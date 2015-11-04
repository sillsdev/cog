using System.Linq;
using NUnit.Framework;
using SIL.Cog.Application.Services.ProjectMigrations;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Cog.Domain.Config;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application.Tests.Services.ProjectMigrations
{
	[TestFixture]
	public class ProjectMigration4Tests
	{
		private readonly ShapeSpanFactory _spanFactory = new ShapeSpanFactory();
		private CogProject _project;
		private SegmentPool _segmentPool;

		[SetUp]
		public void SetUp()
		{
			_segmentPool = new SegmentPool();
			_project = ConfigManager.Load(_spanFactory, _segmentPool, "Services\\ProjectMigrations\\ProjectMigration4Tests.cogx");
		}

		[Test]
		public void Migrate_NoPrenasalSuperscript_PrenasalSuperscriptAdded()
		{
			Assert.That(_project.Segmenter.Consonants.Contains("ⁿ"), Is.False);
			var pm = new ProjectMigration4();
			pm.Migrate(_segmentPool, _project);
			Assert.That(_project.Segmenter.Consonants.Contains("ⁿ"), Is.True);
		}

		[Test]
		public void Migrate_ExistingPrenasalSuperscript_PrenasalSuperscriptNotUpdated()
		{
			FeatureStruct fs = FeatureStruct.New(_project.FeatureSystem).Symbol("nasal+").Value;
			_project.Segmenter.Consonants.Add("ⁿ", fs);

			var pm = new ProjectMigration4();
			pm.Migrate(_segmentPool, _project);
			Symbol symbol = _project.Segmenter.Consonants["ⁿ"];
			Assert.That(symbol.FeatureStruct.ValueEquals(fs), Is.True);
		}

		[Test]
		public void Migrate_NewProjectSspSyllabifier_SspSyllabifierUpdated()
		{
			var pm = new ProjectMigration4();
			pm.Migrate(_segmentPool, _project);
			var syllabifier = (SspSyllabifier) _project.VarietyProcessors[ComponentIdentifiers.Syllabifier];
			SonorityClass[] scale = syllabifier.SonorityScale.ToArray();
			Assert.That(scale.Length, Is.EqualTo(17));
			Assert.That(scale[0].SoundClass.Name, Is.EqualTo("Prenasal"));
			Assert.That(((UnnaturalClass) scale[8].SoundClass).IgnoreModifiers, Is.True);
			Assert.That(((NaturalClass) scale[16].SoundClass).FeatureStruct.ValueEquals(FeatureStruct.New(_project.FeatureSystem)
				.Symbol(CogFeatureSystem.VowelType).Symbol("syllabic+").Symbol("open").Value), Is.True);
		}

		[Test]
		public void Migrate_SettingsSspSyllabifier_SspSyllabifierUpdated()
		{
			var oldSyllabifier = (SspSyllabifier) _project.VarietyProcessors[ComponentIdentifiers.Syllabifier];
			SonorityClass[] oldScale = oldSyllabifier.SonorityScale.ToArray();
			oldScale[7] = new SonorityClass(8, new UnnaturalClass("Glide", new[] {"j", "ɥ", "ɰ", "w"}, true, _project.Segmenter));
			oldScale[15] = new SonorityClass(15, new NaturalClass("Open vowel", FeatureStruct.New(_project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("open").Symbol("syllabic+").Value));
			var newSyllabifier = new SspSyllabifier(oldSyllabifier.CombineVowels, oldSyllabifier.CombineConsonants, oldSyllabifier.VowelsSameSonorityTautosyllabic,
				_segmentPool, oldScale);
			_project.VarietyProcessors[ComponentIdentifiers.Syllabifier] = newSyllabifier;

			var pm = new ProjectMigration4();
			pm.Migrate(_segmentPool, _project);
			var syllabifier = (SspSyllabifier) _project.VarietyProcessors[ComponentIdentifiers.Syllabifier];
			SonorityClass[] scale = syllabifier.SonorityScale.ToArray();
			Assert.That(scale.Length, Is.EqualTo(17));
			Assert.That(scale[0].SoundClass.Name, Is.EqualTo("Prenasal"));
		}

		[Test]
		public void Migrate_SimpleSyllabifier_SimpleSyllabifierNotUpdated()
		{
			var syllabifier = new SimpleSyllabifier(true, true);
			_project.VarietyProcessors[ComponentIdentifiers.Syllabifier] = syllabifier;

			var pm = new ProjectMigration4();
			pm.Migrate(_segmentPool, _project);
			Assert.That(_project.VarietyProcessors[ComponentIdentifiers.Syllabifier], Is.EqualTo(syllabifier));
		}

		[Test]
		public void Migrate_ModifiedSspSyllabifier_SspSyllabifierNotUpdated()
		{
			var oldSyllabifier = (SspSyllabifier) _project.VarietyProcessors[ComponentIdentifiers.Syllabifier];
			SonorityClass[] oldScale = oldSyllabifier.SonorityScale.ToArray();
			oldScale[0] = new SonorityClass(1, new NaturalClass("Stop", FeatureStruct.New(_project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("stop").Value));
			var newSyllabifier = new SspSyllabifier(oldSyllabifier.CombineVowels, oldSyllabifier.CombineConsonants, oldSyllabifier.VowelsSameSonorityTautosyllabic,
				_segmentPool, oldScale);
			_project.VarietyProcessors[ComponentIdentifiers.Syllabifier] = newSyllabifier;

			var pm = new ProjectMigration4();
			pm.Migrate(_segmentPool, _project);
			Assert.That(_project.VarietyProcessors[ComponentIdentifiers.Syllabifier], Is.EqualTo(newSyllabifier));
		}
	}
}
