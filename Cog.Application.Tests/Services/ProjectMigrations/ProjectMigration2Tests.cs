using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SIL.Cog.Application.Services.ProjectMigrations;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Extensions;
using SIL.Machine.FeatureModel;
using SIL.ObjectModel;

namespace SIL.Cog.Application.Tests.Services.ProjectMigrations
{
	[TestFixture]
	public class ProjectMigration2Tests
	{
		private FeatureSystem _featSys;
		private CogProject _project;
		private SegmentPool _segmentPool;

		[SetUp]
		public void SetUp()
		{
			_featSys = new FeatureSystem
				{
					new SymbolicFeature("manner",
						new FeatureSymbol("stop"),
						new FeatureSymbol("affricate"),
						new FeatureSymbol("fricative"),
						new FeatureSymbol("approximant"))
				};
			_project = new CogProject() {FeatureSystem = _featSys};
			_segmentPool = new SegmentPool();
		}

		[Test]
		public void AirstreamFeature()
		{
			var pm = new ProjectMigration2();
			pm.Migrate(_segmentPool, _project);

			Assert.That(_featSys.ContainsFeature("airstream"), Is.True);
			Assert.That(_featSys.ContainsSymbol("pulmonic"), Is.True);
			Assert.That(_featSys.ContainsSymbol("ejective"), Is.True);
			Assert.That(_featSys.ContainsSymbol("implosive"), Is.True);
			Assert.That(_featSys.ContainsSymbol("click"), Is.True);
		}

		[Test]
		public void Pulmonics()
		{
			_project.Segmenter.Vowels.Add("a", FeatureStruct.New().Value);
			_project.Segmenter.Consonants.Add("b", FeatureStruct.New(_featSys).Symbol("stop").Value);

			var pm = new ProjectMigration2();
			pm.Migrate(_segmentPool, _project);

			Assert.That(_project.Segmenter.Vowels["a"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("pulmonic").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
			Assert.That(_project.Segmenter.Consonants["b"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("stop").Symbol("pulmonic").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
		}

		[Test]
		public void Implosives()
		{
			_project.Segmenter.Consonants.Add("ɓ", FeatureStruct.New(_featSys).Symbol("stop").Value);
			_project.Segmenter.Consonants.Add("ɗ", FeatureStruct.New(_featSys).Symbol("stop").Value);
			_project.Segmenter.Consonants.Add("ʄ", FeatureStruct.New(_featSys).Symbol("stop").Value);
			_project.Segmenter.Consonants.Add("ʛ", FeatureStruct.New(_featSys).Symbol("stop").Value);

			var pm = new ProjectMigration2();
			pm.Migrate(_segmentPool, _project);

			Assert.That(_project.Segmenter.Consonants["ɓ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("stop").Symbol("implosive").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
			Assert.That(_project.Segmenter.Consonants["ɗ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("stop").Symbol("implosive").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
			Assert.That(_project.Segmenter.Consonants["ʄ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("stop").Symbol("implosive").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
			Assert.That(_project.Segmenter.Consonants["ʛ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("stop").Symbol("implosive").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
		}

		[Test]
		public void Ejectives()
		{
			_project.Segmenter.Modifiers.Add("ʼ");

			var pm = new ProjectMigration2();
			pm.Migrate(_segmentPool, _project);

			Assert.That(_project.Segmenter.Modifiers["ʼ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("ejective").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
		}

		[Test]
		public void Clicks()
		{
			_project.Segmenter.Consonants.Add("p", FeatureStruct.New(_featSys).Symbol("stop").Value);
			_project.Segmenter.Consonants.Add("θ", FeatureStruct.New(_featSys).Symbol("fricative").Value);
			_project.Segmenter.Consonants.Add("ɬ", FeatureStruct.New(_featSys).Symbol("fricative").Value);
			_project.Segmenter.Consonants.Add("t", FeatureStruct.New(_featSys).Symbol("stop").Value);

			var pm = new ProjectMigration2();
			pm.Migrate(_segmentPool, _project);

			Assert.That(_project.Segmenter.Consonants["ʘ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("affricate").Symbol("click").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
			Assert.That(_project.Segmenter.Consonants["ǀ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("affricate").Symbol("click").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
			Assert.That(_project.Segmenter.Consonants["ǁ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("affricate").Symbol("click").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
			Assert.That(_project.Segmenter.Consonants["ǃ"].FeatureStruct, Is.EqualTo(FeatureStruct.New(_featSys).Symbol("stop").Symbol("click").Value).Using(FreezableEqualityComparer<FeatureStruct>.Default));
			Assert.That(_project.Segmenter.Consonants.Contains("ǂ"), Is.False);
		}

		[Test]
		public void AlineFeatures()
		{
			var manner = _featSys.GetFeature<SymbolicFeature>("manner");
			var aline = new Aline(_segmentPool, Enumerable.Empty<SymbolicFeature>(), manner.ToEnumerable(),
				new Dictionary<SymbolicFeature, int> {{manner, 50}},
				new Dictionary<FeatureSymbol, int>
					{
						{manner.PossibleSymbols["stop"], 100},
						{manner.PossibleSymbols["affricate"], 95},
						{manner.PossibleSymbols["fricative"], 90},
						{manner.PossibleSymbols["approximant"], 80}
					});

			_project.WordAligners["primary"] = aline;
			var pm = new ProjectMigration2();
			pm.Migrate(_segmentPool, _project);

			aline = (Aline) _project.WordAligners["primary"];
			Assert.That(aline.RelevantConsonantFeatures.Select(f => f.ID), Is.EquivalentTo(new[] {"manner", "airstream"}));
			Assert.That(aline.FeatureWeights.Select(kvp => Tuple.Create(kvp.Key.ID, kvp.Value)), Is.EquivalentTo(new[] {Tuple.Create("manner", 50), Tuple.Create("airstream", 5)}));
			Assert.That(aline.ValueMetrics.Select(kvp => Tuple.Create(kvp.Key.ID, kvp.Value)), Is.EquivalentTo(new[]
				{
					Tuple.Create("stop", 100),
					Tuple.Create("affricate", 95),
					Tuple.Create("fricative", 90),
					Tuple.Create("approximant", 80),
					Tuple.Create("pulmonic", 100),
					Tuple.Create("ejective", 66),
					Tuple.Create("implosive", 33),
					Tuple.Create("click", 0)
				}));
		}
	}
}
