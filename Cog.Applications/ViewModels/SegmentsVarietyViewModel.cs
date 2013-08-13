using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class SegmentsVarietyViewModel : VarietyViewModel
	{
		private readonly ReadOnlyMirroredList<Segment, VarietySegmentViewModel> _segments;
		private readonly ICommand _switchToVarietyCommand;

		public SegmentsVarietyViewModel(SegmentsViewModel segmentsViewModel, Variety variety)
			: base(variety)
		{
			_segments = new ReadOnlyMirroredList<Segment, VarietySegmentViewModel>(segmentsViewModel.DomainSegments,
				segment => new VarietySegmentViewModel(variety, segment, segmentsViewModel.DomainSyllablePosition), viewModel => viewModel.DomainSegment);
			_switchToVarietyCommand = new RelayCommand(() => Messenger.Default.Send(new SwitchViewMessage(typeof(VarietiesViewModel), DomainVariety)));
		}

		public ReadOnlyObservableList<VarietySegmentViewModel> Segments
		{
			get { return _segments; }
		}

		public ICommand SwitchToVarietyCommand
		{
			get { return _switchToVarietyCommand; }
		}
	}
}
