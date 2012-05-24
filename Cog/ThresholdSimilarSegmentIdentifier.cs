namespace SIL.Cog
{
	public class ThresholdSimilarSegmentIdentifier : IProcessor<VarietyPair>
	{
		private readonly EditDistance _editDistance;
		private readonly int _vowelThreshold;
		private readonly int _consonantThreshold;

		public ThresholdSimilarSegmentIdentifier(EditDistance editDistance, int vowelThreshold, int consonantThreshold)
		{
			_editDistance = editDistance;
			_vowelThreshold = vowelThreshold;
			_consonantThreshold = consonantThreshold;
		}

		public void Process(VarietyPair varietyPair)
		{
			foreach (Segment seg1 in varietyPair.Variety1.Segments)
			{
				foreach (Segment seg2 in varietyPair.Variety2.Segments)
				{
					if (seg1.StrRep == seg2.StrRep)
						continue;

					if (seg1.Type == CogFeatureSystem.VowelType && seg2.Type == CogFeatureSystem.VowelType)
					{
						if (_editDistance.Delta(seg1.FeatureStruct, seg2.FeatureStruct) <= _vowelThreshold)
							varietyPair.AddSimilarSegment(seg1, seg2);
					}
					else if (seg1.Type == CogFeatureSystem.ConsonantType && seg2.Type == CogFeatureSystem.ConsonantType)
					{
						if (_editDistance.Delta(seg1.FeatureStruct, seg2.FeatureStruct) <= _consonantThreshold)
							varietyPair.AddSimilarSegment(seg1, seg2);
					}
				}
			}
		}
	}
}
