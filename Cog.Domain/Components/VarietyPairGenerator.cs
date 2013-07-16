namespace SIL.Cog.Domain.Components
{
	public class VarietyPairGenerator : IProcessor<CogProject>
	{
		public void Process(CogProject data)
		{
			data.VarietyPairs.Clear();
			for (int i = 0; i < data.Varieties.Count; i++)
			{
				for (int j = i + 1; j < data.Varieties.Count; j++)
					data.VarietyPairs.Add(new VarietyPair(data.Varieties[i], data.Varieties[j]));
			}
		}
	}
}
