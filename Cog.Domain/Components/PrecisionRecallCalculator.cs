namespace SIL.Cog.Domain.Components
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
				if (wp.AreCognatePredicted)
				{
					if (wp.AreCognateActual)
						tp++;
					else
						fp++;
				}
				else if (wp.AreCognateActual)
				{
						fn++;
				}
			}

			varietyPair.Precision = (double) tp / (tp + fp);
			varietyPair.Recall = (double) tp / (tp + fn);
		}
	}
}
