using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.Export
{
	public class TextSegmentFrequenciesExporter : ISegmentFrequenciesExporter
	{
		private readonly static Dictionary<string, int> SortOrderLookup = new Dictionary<string, int>
			{
				{"bilabial", 0},
				{"labiodental", 0},
				{"dental", 1},
				{"alveolar", 1},
				{"retroflex", 1},
				{"palato-alveolar", 1},
				{"alveolo-palatal", 1},
				{"palatal", 2},
				{"velar", 2},
				{"uvular", 2},
				{"pharyngeal", 3},
				{"epiglottal", 3},
				{"glottal", 3},

				{"close-vowel", 0},
				{"mid-vowel", 1},
				{"open-vowel", 2}
			};

		public void Export(string path, CogProject project, ViewModelSyllablePosition syllablePosition)
		{
			var domainSyllablePosition = SyllablePosition.Onset;
			switch (syllablePosition)
			{
				case ViewModelSyllablePosition.Onset:
					domainSyllablePosition = SyllablePosition.Onset;
					break;
				case ViewModelSyllablePosition.Nucleus:
					domainSyllablePosition = SyllablePosition.Nucleus;
					break;
				case ViewModelSyllablePosition.Coda:
					domainSyllablePosition = SyllablePosition.Coda;
					break;
			}

			Segment[] segments = project.Varieties
				.SelectMany(v => v.SegmentFrequencyDistributions[domainSyllablePosition].ObservedSamples)
				.Distinct().Where(s => !s.IsComplex).OrderBy(GetSortOrder).ThenBy(s => s.StrRep).ToArray();

			using (var writer = new StreamWriter(path))
			{
				foreach (Segment seg in segments)
				{
					writer.Write("\t");
					writer.Write(seg.StrRep);
				}
				writer.WriteLine();

				foreach (Variety variety in project.Varieties)
				{
					writer.Write(variety.Name);
					foreach (Segment seg in segments)
					{
						writer.Write("\t");
						writer.Write(variety.SegmentFrequencyDistributions[domainSyllablePosition][seg]);
					}
					writer.WriteLine();
				}
			}
		}

		private static int GetSortOrder(Segment segment)
		{
			return SortOrderLookup[segment.Type == CogFeatureSystem.VowelType ? ((FeatureSymbol) segment.FeatureStruct.GetValue<SymbolicFeatureValue>("manner")).ID
				: ((FeatureSymbol) segment.FeatureStruct.GetValue<SymbolicFeatureValue>("place")).ID];
		}
	}
}
