using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SIL.Extensions;

namespace SIL.Cog.Application.ViewModels
{
	public enum SegmentMappingState
	{
		Unmapped,
		ThresholdMapped,
		DefiniteListMapped,
		IndefiniteListMapped,
	}

	public class SegmentMappingsTableSegmentPairViewModel : ViewModelBase
	{
		public delegate SegmentMappingsTableSegmentPairViewModel Factory(SegmentMappingsTableSegmentViewModel segment1, SegmentMappingsTableSegmentViewModel segment2, int delta, bool enabled);

		private readonly SegmentMappingViewModel.Factory _mappingFactory;
		private readonly SegmentMappingsTableSegmentViewModel _segment1;
		private readonly SegmentMappingsTableSegmentViewModel _segment2;
		private SegmentMappingState _mappingState;
		private bool _meetsThreshold;
		private readonly bool _enabled;
		private readonly ICommand _toggleMappingCommand;
		private readonly int _delta;
		private readonly SegmentMappingsViewModel _mappings;

		public SegmentMappingsTableSegmentPairViewModel(SegmentMappingsViewModel mappings, SegmentMappingViewModel.Factory mappingFactory,
			SegmentMappingsTableSegmentViewModel segment1, SegmentMappingsTableSegmentViewModel segment2, int delta, bool enabled)
		{
			_mappingFactory = mappingFactory;
			_segment1 = segment1;
			_segment2 = segment2;
			_enabled = enabled;
			_delta = delta;
			_mappings = mappings;
			_mappings.ConstrainToSegmentPair(_segment1.StrRep, _segment2.StrRep);
			_mappings.ImportEnabled = false;
			_mappings.Mappings.CollectionChanged += MappingsChanged;
			_toggleMappingCommand = new RelayCommand(ToggleMapping);
		}

		private void MappingsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateMappingState();
		}

		public SegmentMappingsTableSegmentViewModel Segment1
		{
			get { return _segment1; }
		}

		public SegmentMappingsTableSegmentViewModel Segment2
		{
			get { return _segment2; }
		}

		public SegmentMappingsViewModel Mappings
		{
			get { return _mappings; }
		}

		public SegmentMappingState MappingState
		{
			get { return _mappingState; }
			private set { Set(() => MappingState, ref _mappingState, value); }
		}

		public ICommand ToggleMappingCommand
		{
			get { return _toggleMappingCommand; }
		}

		private void ToggleMapping()
		{
			switch (_mappingState)
			{
				case SegmentMappingState.ThresholdMapped:
					_mappings.Mappings.Add(_mappingFactory(_segment1.StrRep, _segment2.StrRep));
					MappingState = SegmentMappingState.DefiniteListMapped;
					break;
				case SegmentMappingState.DefiniteListMapped:
				case SegmentMappingState.IndefiniteListMapped:
					_mappings.Mappings.Clear();
					MappingState = _meetsThreshold ? SegmentMappingState.ThresholdMapped : SegmentMappingState.Unmapped;
					break;
				case SegmentMappingState.Unmapped:
					_mappings.Mappings.Add(_mappingFactory(_segment1.StrRep, _segment2.StrRep));
					MappingState = SegmentMappingState.DefiniteListMapped;
					break;
			}
		}

		public int Delta
		{
			get { return _delta; }
		}

		internal bool MeetsThreshold
		{
			get { return _meetsThreshold; }
			set
			{
				_meetsThreshold = value;
				UpdateMappingState();
			}
		}

		private void UpdateMappingState()
		{
			if (_mappings.Mappings.Count > 0)
				MappingState = _mappings.Mappings.All(m => HasEnvironment(m.Segment1) || HasEnvironment(m.Segment2)) ? SegmentMappingState.IndefiniteListMapped : SegmentMappingState.DefiniteListMapped;
			else if (_meetsThreshold)
				MappingState = SegmentMappingState.ThresholdMapped;
			else
				MappingState = SegmentMappingState.Unmapped;
		}

		public bool IsEnabled
		{
			get { return _enabled; }
		}

		private bool HasEnvironment(string str)
		{
			return str[0].IsOneOf('C', 'V', '#') || str[str.Length - 1].IsOneOf('C', 'V', '#');
		}
	}
}
