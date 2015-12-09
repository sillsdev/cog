using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public class SimilarSegmentMappingsViewModel : ChangeTrackingViewModelBase
	{
		public delegate SimilarSegmentMappingsViewModel Factory(SoundType soundType);

		private readonly IProjectService _projectService;
		private readonly SegmentMappingsViewModel _mappings;
		private readonly SoundType _soundType;
		private int _threshold;
		private bool _implicitComplexSegments;
		private readonly ICommand _editSegmentMappingsTableCommand;
		private readonly IDialogService _dialogService;
		private readonly SegmentMappingsTableViewModel.Factory _segmentMappingsTableFactory;
		private readonly SegmentMappingViewModel.Factory _mappingFactory;
		
		public SimilarSegmentMappingsViewModel(IProjectService projectService, IDialogService dialogService, SegmentMappingsTableViewModel.Factory segmentMappingsTableFactory,
			SegmentMappingsViewModel mappings, SegmentMappingViewModel.Factory mappingFactory, SoundType soundType)
		{
			_projectService = projectService;
			_mappings = mappings;
			_mappings.PropertyChanged += ChildPropertyChanged;
			_soundType = soundType;
			_dialogService = dialogService;
			_segmentMappingsTableFactory = segmentMappingsTableFactory;
			_mappingFactory = mappingFactory;
			_editSegmentMappingsTableCommand = new RelayCommand(EditSegmentMappingsTable);
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			_mappings.AcceptChanges();
		}

		public int Threshold
		{
			get { return _threshold; }
			set { SetChanged(() => Threshold, ref _threshold, value); }
		}

		public SegmentMappingsViewModel Mappings
		{
			get { return _mappings; }
		}

		public bool ImplicitComplexSegments
		{
			get { return _implicitComplexSegments; }
			set { SetChanged(() => ImplicitComplexSegments, ref _implicitComplexSegments, value); }
		}

		public SoundType SoundType
		{
			get { return _soundType; }
		}

		public UnionSegmentMappings SegmentMappings { get; set; }

		public ICommand EditSegmentMappingsTableCommand
		{
			get { return _editSegmentMappingsTableCommand; }
		}

		private void EditSegmentMappingsTable()
		{
			SegmentMappingsTableViewModel vm = _segmentMappingsTableFactory(_mappings.Mappings, _soundType, _threshold);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				using (_mappings.Mappings.BulkUpdate())
				{
					_mappings.Mappings.RemoveAll(m => m.IsValid);
					_mappings.Mappings.AddRange(vm.Segments.SelectMany(s => s.SegmentPairs).Where(sp => sp.IsEnabled)
						.SelectMany(sp => sp.Mappings.Mappings));
				}
				Threshold = vm.Threshold;
			}
		}

		public void Setup()
		{
			_mappings.SelectedMapping = null;
			_mappings.Mappings.Clear();
			if (SegmentMappings == null)
			{
				Set(() => Threshold, ref _threshold, _soundType == SoundType.Vowel ? 500 : 600);
				Set(() => ImplicitComplexSegments, ref _implicitComplexSegments, false);
			}
			else
			{
				Set(() => Threshold, ref _threshold, ((ThresholdSegmentMappings) SegmentMappings.SegmentMappingsComponents[0]).Threshold);

				var listSegmentMappings = (ListSegmentMappings) SegmentMappings.SegmentMappingsComponents[1];
				_mappings.Mappings.AddRange(listSegmentMappings.Mappings.Select(m => _mappingFactory(m.Item1, m.Item2)));
				Set(() => ImplicitComplexSegments, ref _implicitComplexSegments, listSegmentMappings.ImplicitComplexSegments);
			}
		}

		public void UpdateComponent()
		{
			var thresholdSegmentMappings = new ThresholdSegmentMappings(_projectService.Project, _threshold, ComponentIdentifiers.PrimaryWordAligner);
			var listSegmentMappings = new ListSegmentMappings(_projectService.Project.Segmenter, _mappings.Mappings.Select(m => UnorderedTuple.Create(m.Segment1, m.Segment2)), _implicitComplexSegments);
			SegmentMappings = new UnionSegmentMappings(new ISegmentMappings[] {thresholdSegmentMappings, listSegmentMappings});
		}
	}
}
