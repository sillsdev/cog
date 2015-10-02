using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public enum SegmentMappingState
	{
		Unmapped,
		ThresholdMapped,
		DefiniteListMapped,
		IndefiniteListMapped,
	}

	public class SegmentMappingsChartSegmentPairViewModel : ViewModelBase
	{
		private readonly Segment _segment1;
		private readonly Segment _segment2;
		private SegmentMappingState _mappingState;
		private bool _meetsThreshold;
		private readonly bool _enabled;
		private readonly ICommand _toggleMappingCommand;
		private readonly HashSet<UnorderedTuple<string, string>> _mappings;

		public SegmentMappingsChartSegmentPairViewModel(Segment segment1, Segment segment2, bool enabled)
		{
			_segment1 = segment1;
			_segment2 = segment2;
			_enabled = enabled;
			_mappings = new HashSet<UnorderedTuple<string, string>>();
			_toggleMappingCommand = new RelayCommand(ToggleMapping);
		}

		internal Segment DomainSegment1
		{
			get { return _segment1; }
		}

		internal Segment DomainSegment2
		{
			get { return _segment2; }
		}

		public string StrRep1
		{
			get { return _segment1 == null ? "-" : _segment1.StrRep; }
		}

		public string StrRep2
		{
			get { return _segment2 == null ? "-" : _segment2.StrRep; }
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
					_mappings.Add(UnorderedTuple.Create(StrRep1, StrRep2));
					MappingState = SegmentMappingState.DefiniteListMapped;
					break;
				case SegmentMappingState.DefiniteListMapped:
				case SegmentMappingState.IndefiniteListMapped:
					_mappings.Clear();
					MappingState = _meetsThreshold ? SegmentMappingState.ThresholdMapped : SegmentMappingState.Unmapped;
					break;
				case SegmentMappingState.Unmapped:
					_mappings.Add(UnorderedTuple.Create(StrRep1, StrRep2));
					MappingState = SegmentMappingState.DefiniteListMapped;
					break;
			}
		}

		internal int Delta { get; set; }

		internal bool MeetsThreshold
		{
			get { return _meetsThreshold; }
			set
			{
				_meetsThreshold = value;
				UpdateMappingState();
			}
		}

		internal ISet<UnorderedTuple<string, string>> Mappings
		{
			get { return _mappings; }
		}

		private void UpdateMappingState()
		{
			if (_mappings.Count > 0)
				MappingState = _mappings.All(m => HasEnvironment(m.Item1) || HasEnvironment(m.Item2)) ? SegmentMappingState.IndefiniteListMapped : SegmentMappingState.DefiniteListMapped;
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
