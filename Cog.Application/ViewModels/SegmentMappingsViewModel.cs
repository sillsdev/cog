using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Collections;
using SIL.ObjectModel;

namespace SIL.Cog.Application.ViewModels
{
	public class SegmentMappingsViewModel : ChangeTrackingViewModelBase
	{
		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly SegmentMappingViewModel.Factory _mappingFactory;
		private readonly NewSegmentMappingViewModel.Factory _newMappingFactory;
		private readonly BindableList<SegmentMappingViewModel> _mappings;
		private SegmentMappingViewModel _selectedMapping;
		private readonly ICommand _newCommand;
		private readonly ICommand _removeCommand;
		private readonly ICommand _importCommand;
		private string _segment1;
		private string _segment2;
		private bool _importEnabled = true;

		public SegmentMappingsViewModel(IDialogService dialogService, IImportService importService, SegmentMappingViewModel.Factory mappingFactory,
			NewSegmentMappingViewModel.Factory newMappingFactory)
		{
			_dialogService = dialogService;
			_importService = importService;
			_mappingFactory = mappingFactory;
			_newMappingFactory = newMappingFactory;
			_mappings = new BindableList<SegmentMappingViewModel>();
			_newCommand = new RelayCommand(AddMapping);
			_removeCommand = new RelayCommand(RemoveMapping, CanRemoveMapping);
			_importCommand = new RelayCommand(Import);
			_mappings.CollectionChanged += MappingsChanged;
		}

		private void MappingsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		private void AddMapping()
		{
			NewSegmentMappingViewModel vm = _newMappingFactory();
			vm.Segment1 = _segment1;
			vm.Segment2 = _segment2;
			vm.SegmentsEnabled = _segment1 == null && _segment2 == null;
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				SegmentMappingViewModel mapping = _mappingFactory(vm.LeftEnvironment1 + vm.Segment1 + vm.RightEnvironment1,
					vm.LeftEnvironment2 + vm.Segment2 + vm.RightEnvironment2);
				SegmentMappingViewModel existingMapping = _mappings.FirstOrDefault(m => (m.Segment1 == mapping.Segment1 && m.Segment2 == mapping.Segment2)
					|| (m.Segment1 == mapping.Segment2 && m.Segment2 == mapping.Segment1));
				if (existingMapping == null)
					_mappings.Add(mapping);
				else
					mapping = existingMapping;
				SelectedMapping = mapping;
			}
		}

		private void RemoveMapping()
		{
			_mappings.Remove(_selectedMapping);
		}

		private bool CanRemoveMapping()
		{
			return _selectedMapping != null;
		}

		private void Import()
		{
			IEnumerable<Tuple<string, string>> mappings;
			if (_importService.ImportSegmentMappings(this, out mappings))
				_mappings.ReplaceAll(mappings.Select(m => _mappingFactory(m.Item1, m.Item2)));
		}

		internal void ConstrainToSegmentPair(string segment1, string segment2)
		{
			_segment1 = segment1;
			_segment2 = segment2;
		}

		public bool ImportEnabled
		{
			get { return _importEnabled; }
			set { Set(() => ImportEnabled, ref _importEnabled, value); }
		}

		public SegmentMappingViewModel SelectedMapping
		{
			get { return _selectedMapping; }
			set { Set(() => SelectedMapping, ref _selectedMapping, value); }
		}

		public BulkObservableList<SegmentMappingViewModel> Mappings
		{
			get { return _mappings; }
		}

		public ICommand NewCommand
		{
			get { return _newCommand; }
		}

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

		public ICommand ImportCommand
		{
			get { return _importCommand; }
		}
	}
}
