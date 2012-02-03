namespace SIL.Cog
{
	public class SimilarPhonemeIdentifier : IProcessor<VarietyPair>
	{
		private readonly EditDistance _editDistance;
		private readonly int _vowelThreshold;
		private readonly int _consonantThreshold;

		public SimilarPhonemeIdentifier(EditDistance editDistance, int vowelThreshold, int consonantThreshold)
		{
			_editDistance = editDistance;
			_vowelThreshold = vowelThreshold;
			_consonantThreshold = consonantThreshold;
		}

		public void Process(VarietyPair varietyPair)
		{
			foreach (Phoneme ph1 in varietyPair.Variety1.Phonemes)
			{
				foreach (Phoneme ph2 in varietyPair.Variety2.Phonemes)
				{
					if (ph1.Type == CogFeatureSystem.VowelType && ph2.Type == CogFeatureSystem.VowelType)
					{
						if (_editDistance.Delta(ph1.FeatureStruct, ph2.FeatureStruct) <= _vowelThreshold)
							varietyPair.AddSimilarPhoneme(ph1, ph2);
					}
					else if (ph1.Type == CogFeatureSystem.ConsonantType && ph2.Type == CogFeatureSystem.ConsonantType)
					{
						if (_editDistance.Delta(ph1.FeatureStruct, ph2.FeatureStruct) <= _consonantThreshold)
							varietyPair.AddSimilarPhoneme(ph1, ph2);
					}
				}
			}
		}
	}
}
