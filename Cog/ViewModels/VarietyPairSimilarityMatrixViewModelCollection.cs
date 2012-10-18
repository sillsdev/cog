using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace SIL.Cog.ViewModels
{
	public class VarietyPairSimilarityMatrixViewModelCollection : ListViewModelCollection<ObservableCollection<Variety>, VarietyPairSimilarityMatrixViewModel, Variety>
	{
		private readonly Variety _variety;

		public VarietyPairSimilarityMatrixViewModelCollection(ObservableCollection<Variety> varieties, Variety variety)
			: base(varieties, v =>
				{
					VarietyPair vp;
					if (variety.VarietyPairs.TryGetValue(v, out vp))
						return new VarietyPairSimilarityMatrixViewModel(v, vp);
					return new VarietyPairSimilarityMatrixViewModel(v);
				})
		{
			_variety = variety;
			variety.VarietyPairs.CollectionChanged += VarietyPairsChanged;
		}

		private void VarietyPairsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Dictionary<Variety, VarietyPair> addedPairs = e.NewItems.Cast<VarietyPair>().ToDictionary(vp => vp.GetOtherVariety(_variety));
					foreach (VarietyPairSimilarityMatrixViewModel vm in this)
					{
						VarietyPair vp;
						if (addedPairs.TryGetValue(vm.ModelOtherVariety, out vp))
						{
							vm.ModelVarietyPair = vp;
							addedPairs.Remove(vm.ModelOtherVariety);
						}
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					var removedPairs = new HashSet<VarietyPair>(e.OldItems.Cast<VarietyPair>());
					foreach (VarietyPairSimilarityMatrixViewModel vm in this)
					{
						if (vm.ModelVarietyPair != null && removedPairs.Contains(vm.ModelVarietyPair))
						{
							removedPairs.Remove(vm.ModelVarietyPair);
							vm.ModelVarietyPair = null;
							if (removedPairs.Count == 0)
								break;
						}
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					foreach (VarietyPairSimilarityMatrixViewModel vm in this)
						vm.ModelVarietyPair = null;
					break;
			}
		}
	}
}
