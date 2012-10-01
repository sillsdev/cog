namespace SIL.Cog.Processors
{
	public class PrecisionRecallCalculator : IProcessor<VarietyPair>
	{
		public void Process(VarietyPair varietyPair)
		{
			int tp = 0;
			int fp = 0;
			int fn = 0;
			foreach (WordPair wp in varietyPair.WordPairs)
			{
				if (wp.AreCognatesPredicted)
				{
					if (wp.AreCognatesActual)
						tp++;
					else
						fp++;
				}
				else if (wp.AreCognatesActual)
				{
						fn++;
				}
			}

			varietyPair.Precision = (double) tp / (tp + fp);
			varietyPair.Recall = (double) tp / (tp + fn);
		}
	}
}
