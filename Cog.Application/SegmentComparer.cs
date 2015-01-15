using System;
using System.Collections.Generic;
using SIL.Cog.Domain;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application
{
	internal class SegmentComparer : IComparer<Segment>
	{
		private static readonly Dictionary<string, int> PlaceSortOrderLookup = new Dictionary<string, int>
			{
				{"bilabial", 0},
				{"labiodental", 1},
				{"dental", 2},
				{"alveolar", 3},
				{"retroflex", 4},
				{"palato-alveolar", 5},
				{"alveolo-palatal", 6},
				{"palatal", 7},
				{"velar", 8},
				{"uvular", 9},
				{"pharyngeal", 10},
				{"epiglottal", 11},
				{"glottal", 12}
			};

		private static readonly Dictionary<string, int> MannerSortOrderLookup = new Dictionary<string, int>
			{
				{"stop", 0},
				{"affricate", 1},
				{"fricative", 2},
				{"approximant", 3},
				{"trill", 4},
				{"flap", 5}
			};

		private static readonly Dictionary<string, int> NasalSortOrderLookup = new Dictionary<string, int>
			{
				{"nasal-", 0},
				{"nasal+", 1}
			};

		private static readonly Dictionary<string, int> LateralSortOrderLookup = new Dictionary<string, int>
			{
				{"lateral-", 0},
				{"lateral+", 1}
			};

		private static readonly Dictionary<string, int> VoiceSortOrderLookup = new Dictionary<string, int>
			{
				{"voice-", 0},
				{"voice+", 1}
			};

		private static readonly Dictionary<string, int> HeightSortOrderLookup = new Dictionary<string, int>
			{
				{"close", 0},
				{"near-close", 1},
				{"close-mid", 2},
				{"mid", 3},
				{"open-mid", 4},
				{"near-open", 5},
				{"open", 6}
			};

		private static readonly Dictionary<string, int> BacknessSortOrderLookup = new Dictionary<string, int>
			{
				{"front", 0},
				{"near-front", 1},
				{"central", 2},
				{"near-back", 3},
				{"back", 4}
			};

		private static readonly Dictionary<string, int> RoundSortOrderLookup = new Dictionary<string, int>
			{
				{"round-", 0},
				{"round+", 1}
			};

		private static readonly Tuple<string, Dictionary<string, int>>[] ConsonantFeatureSortOrder =
			{
				Tuple.Create("lateral", LateralSortOrderLookup),
				Tuple.Create("nasal", NasalSortOrderLookup),
				Tuple.Create("manner", MannerSortOrderLookup),
				Tuple.Create("place", PlaceSortOrderLookup),
				Tuple.Create("voice", VoiceSortOrderLookup)
			};

		private static readonly Tuple<string, Dictionary<string, int>>[] VowelFeatureSortOrder =
			{
				Tuple.Create("backness", BacknessSortOrderLookup),
				Tuple.Create("height", HeightSortOrderLookup),
				Tuple.Create("round", RoundSortOrderLookup)
			};

		public int Compare(Segment x, Segment y)
		{
			Tuple<string, Dictionary<string, int>>[] features = x.Type == CogFeatureSystem.ConsonantType ? ConsonantFeatureSortOrder : VowelFeatureSortOrder;

			FeatureStruct fsx = x.FeatureStruct;
			if (x.IsComplex)
				fsx = x.FeatureStruct.GetValue(CogFeatureSystem.First);
			FeatureStruct fsy = y.FeatureStruct;
			if (y.IsComplex)
				fsy = y.FeatureStruct.GetValue(CogFeatureSystem.First);

			foreach (Tuple<string, Dictionary<string, int>> feature in features)
			{
				int res = Compare(feature.Item1, feature.Item2, fsx, fsy);
				if (res != 0)
					return res;
			}

			return string.Compare(x.StrRep, y.StrRep, StringComparison.Ordinal);
		}

		private int Compare(string feature, Dictionary<string, int> sortOrderLookup, FeatureStruct fsx, FeatureStruct fsy)
		{
			int valx = sortOrderLookup[((FeatureSymbol) fsx.GetValue<SymbolicFeatureValue>(feature)).ID];
			int valy = sortOrderLookup[((FeatureSymbol) fsy.GetValue<SymbolicFeatureValue>(feature)).ID];
			return valx.CompareTo(valy);
		}
	}
}
