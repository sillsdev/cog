namespace SIL.Cog.Processors
{
	public class ThresholdSimilarSegmentIdentifier : ProcessorBase<VarietyPair>
	{
		private readonly int _vowelThreshold;
		private readonly int _consonantThreshold;
		private readonly string _alignerID;

		public ThresholdSimilarSegmentIdentifier(CogProject project, int vowelThreshold, int consonantThreshold, string alignerID)
			: base(project)
		{
			_vowelThreshold = vowelThreshold;
			_consonantThreshold = consonantThreshold;
			_alignerID = alignerID;
		}

		public int VowelThreshold
		{
			get { return _vowelThreshold; }
		}

		public int ConsonantThreshold
		{
			get { return _consonantThreshold; }
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public override void Process(VarietyPair varietyPair)
		{
			IAligner aligner = Project.Aligners[_alignerID];

			foreach (Segment seg1 in varietyPair.Variety1.Segments)
			{
				foreach (Segment seg2 in varietyPair.Variety2.Segments)
				{
					if (seg1.NormalizedStrRep == seg2.NormalizedStrRep)
						continue;

					if (seg1.Type == CogFeatureSystem.VowelType && seg2.Type == CogFeatureSystem.VowelType)
					{
						if (aligner.Delta(seg1.FeatureStruct, seg2.FeatureStruct) <= _vowelThreshold)
							varietyPair.AddSimilarSegment(seg1, seg2);
					}
					else if (seg1.Type == CogFeatureSystem.ConsonantType && seg2.Type == CogFeatureSystem.ConsonantType)
					{
						if (aligner.Delta(seg1.FeatureStruct, seg2.FeatureStruct) <= _consonantThreshold)
							varietyPair.AddSimilarSegment(seg1, seg2);
					}
				}
			}
		}
	}
}
