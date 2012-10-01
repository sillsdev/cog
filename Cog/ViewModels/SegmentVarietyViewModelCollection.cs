using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GalaSoft.MvvmLight.Threading;
using SIL.Collections;
using System.Linq;

namespace SIL.Cog.ViewModels
{
	public class SegmentVarietyViewModelCollection : ObservableCollection<SegmentVarietyViewModel>
	{
		private readonly Dictionary<Segment, SegmentVarietyViewModel> _mapping; 

		public SegmentVarietyViewModelCollection(SegmentCollection source)
		{
			_mapping = new Dictionary<Segment, SegmentVarietyViewModel>();
			foreach (Segment segment in source)
				AddSegmentVM(segment);
			source.CollectionChanged += OnSourceCollectionChanged;
		}

		private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() =>
				{
					switch (e.Action)
					{
						case NotifyCollectionChangedAction.Add:
							foreach (Segment segment in e.NewItems)
								AddSegmentVM(segment);
							break;

						case NotifyCollectionChangedAction.Remove:
							var removedSegments = new HashSet<SegmentVarietyViewModel>(e.OldItems.Cast<Segment>().Select(segment => _mapping[segment]));
							this.RemoveAll(removedSegments.Contains);
							break;

						case NotifyCollectionChangedAction.Reset:
							Clear();
							break;
					}
				});
		}

		private void AddSegmentVM(Segment segment)
		{
			var newVM = new SegmentVarietyViewModel(segment);
			_mapping[segment] = newVM;
			Add(newVM);
		}
	}
}
