using System.Collections.ObjectModel;

namespace SIL.Cog.ViewModels
{
	public class VarietySimilarityMatrixViewModel : VarietyViewModel
	{
		private readonly VarietyPairSimilarityMatrixViewModelCollection _varietyPairs; 

		public VarietySimilarityMatrixViewModel(ObservableCollection<Variety> varieties, Variety variety)
			: base(variety)
		{
			_varietyPairs = new VarietyPairSimilarityMatrixViewModelCollection(varieties, ModelVariety);
		}

		public ObservableCollection<VarietyPairSimilarityMatrixViewModel> VarietyPairs
		{
			get { return _varietyPairs; }
		}
	}
}
