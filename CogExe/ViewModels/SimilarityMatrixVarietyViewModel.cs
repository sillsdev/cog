using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SIL.Cog.ViewModels
{
	public class SimilarityMatrixVarietyViewModel : VarietyViewModel
	{
		private readonly ReadOnlyCollection<SimilarityMatrixVarietyPairViewModel> _varietyPairs; 

		public SimilarityMatrixVarietyViewModel(SimilarityMetric similarityMetric, IEnumerable<Variety> varieties, Variety variety)
			: base(variety)
		{
			var varietyPairs = new List<SimilarityMatrixVarietyPairViewModel>();
			foreach (Variety v in varieties)
			{
				VarietyPair vp;
				varietyPairs.Add(variety.VarietyPairs.TryGetValue(v, out vp) ? new SimilarityMatrixVarietyPairViewModel(similarityMetric, v, vp) : new SimilarityMatrixVarietyPairViewModel(v));
			}
			_varietyPairs = new ReadOnlyCollection<SimilarityMatrixVarietyPairViewModel>(varietyPairs);
		}

		public ReadOnlyCollection<SimilarityMatrixVarietyPairViewModel> VarietyPairs
		{
			get { return _varietyPairs; }
		}
	}
}
