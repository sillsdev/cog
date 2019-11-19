using System.Collections.Generic;
using System.Linq;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Machine.FeatureModel;
using SIL.Extensions;

namespace SIL.Cog.Application.Services.ProjectMigrations
{
	/// <summary>
	/// This migration adds the airstream mechanism feature.
	/// </summary>
	internal class ProjectMigration2 : IProjectMigration
	{
		public int Version
		{
			get { return 2; }
		}

		public void Migrate(SegmentPool segmentPool, CogProject project)
		{
			if (project.FeatureSystem.ContainsFeature("airstream"))
				return;

			var pulmonic = new FeatureSymbol("pulmonic");
			var ejective = new FeatureSymbol("ejective");
			var implosive = new FeatureSymbol("implosive");
			var click = new FeatureSymbol("click");
			var airstream = new SymbolicFeature("airstream",
				pulmonic,
				ejective,
				implosive,
				click) {Description = "Airstream"};
			project.FeatureSystem.Add(airstream);

			AddValue(project.Segmenter.Modifiers, "ʼ", ejective);

			AddValue(project.Segmenter.Consonants, "ɓ", implosive);
			AddValue(project.Segmenter.Consonants, "ɗ", implosive);
			AddValue(project.Segmenter.Consonants, "ʄ", implosive);
			AddValue(project.Segmenter.Consonants, "ɠ", implosive);
			AddValue(project.Segmenter.Consonants, "ʛ", implosive);

			FeatureSymbol affricate;
			if (project.FeatureSystem.TryGetSymbol("affricate", out affricate))
			{
				project.Segmenter.Consonants.AddSymbolBasedOn("ʘ", "p", affricate, click);
				project.Segmenter.Consonants.AddSymbolBasedOn("ǀ", "θ", affricate, click);
				project.Segmenter.Consonants.AddSymbolBasedOn("ǁ", "ɬ", affricate, click);
			}
			project.Segmenter.Consonants.AddSymbolBasedOn("ǃ", "t", click);
			project.Segmenter.Consonants.AddSymbolBasedOn("ǂ", "c", click);

			foreach (Symbol symbol in project.Segmenter.Vowels.ToArray())
				AddValue(project.Segmenter.Vowels, symbol, pulmonic);

			foreach (Symbol symbol in project.Segmenter.Consonants.Where(s => !s.FeatureStruct.ContainsFeature("airstream")).ToArray())
				AddValue(project.Segmenter.Consonants, symbol, pulmonic);

			foreach (KeyValuePair<string, IWordAligner> aligner in project.WordAligners.Where(kvp => kvp.Value is Aline).ToArray())
			{
				var aline = (Aline) aligner.Value;
				Dictionary<SymbolicFeature, int> featWeights = aline.FeatureWeights.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
				featWeights[airstream] = 5;
				Dictionary<FeatureSymbol, int> valueMetrics = aline.ValueMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
				valueMetrics[pulmonic] = 100;
				valueMetrics[ejective] = 66;
				valueMetrics[implosive] = 33;
				valueMetrics[click] = 0;
				project.WordAligners[aligner.Key] = new Aline(segmentPool, aline.RelevantVowelFeatures, aline.RelevantConsonantFeatures.Concat(airstream),
					featWeights, valueMetrics, aline.Settings);
			}
		}

		private static void AddValue(SymbolCollection symbols, string strRep, FeatureSymbol value)
		{
			Symbol symbol;
			if (symbols.TryGet(strRep, out symbol))
				AddValue(symbols, symbol, value);
		}

		private static void AddValue(SymbolCollection symbols, Symbol symbol, FeatureSymbol value)
		{
			FeatureStruct fs = symbol.FeatureStruct.Clone();
			fs.AddValue(value.Feature, value);
			fs.Freeze();
			symbols.Remove(symbol);
			symbols.Add(symbol.StrRep, fs, symbol.Overwrite);
		}
	}
}
