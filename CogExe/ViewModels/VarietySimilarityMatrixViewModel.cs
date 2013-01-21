using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SIL.Cog.ViewModels
{
	public class VarietySimilarityMatrixViewModel : VarietyViewModel
	{
		private readonly ReadOnlyCollection<VarietyPairSimilarityMatrixViewModel> _varietyPairs; 

		public VarietySimilarityMatrixViewModel(SimilarityMetric similarityMetric, IEnumerable<Variety> varieties, Variety variety)
			: base(variety)
		{
			var varietyPairs = new List<VarietyPairSimilarityMatrixViewModel>();
			foreach (Variety v in varieties)
			{
				VarietyPair vp;
				varietyPairs.Add(variety.VarietyPairs.TryGetValue(v, out vp) ? new VarietyPairSimilarityMatrixViewModel(similarityMetric, v, vp) : new VarietyPairSimilarityMatrixViewModel(v));
			}
			_varietyPairs = new ReadOnlyCollection<VarietyPairSimilarityMatrixViewModel>(varietyPairs);
		}

		public ReadOnlyCollection<VarietyPairSimilarityMatrixViewModel> VarietyPairs
		{
			get { return _varietyPairs; }
		}
	}
}
