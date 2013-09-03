using System;
using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Applications.Services;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class SegmentMappingsViewModel : ChangeTrackingViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly BindableList<SegmentMappingViewModel> _mappings;
		private SegmentMappingViewModel _selectedMapping;
		private readonly ICommand _newCommand;
		private readonly ICommand _removeCommand;
		private readonly ICommand _importCommand;

		public SegmentMappingsViewModel(IProjectService projectService, IDialogService dialogService, IImportService importService)
		{
			_projectService = projectService;
			_dialogService = dialogService;
			_importService = importService;
			_mappings = new BindableList<SegmentMappingViewModel>();
			_newCommand = new RelayCommand(AddMapping);
			_removeCommand = new RelayCommand(RemoveMapping, CanRemoveMapping);
			_importCommand = new RelayCommand(Import);
		}

		private void AddMapping()
		{
			var vm = new NewSegmentMappingViewModel(_projectService.Project.Segmenter);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var mapping = new SegmentMappingViewModel(_projectService.Project.Segmenter, vm.Segment1, vm.Segment2);
				_mappings.Add(mapping);
				SelectedMapping = mapping;
				IsChanged = true;
			}
		}

		private void RemoveMapping()
		{
			_mappings.Remove(_selectedMapping);
			IsChanged = true;
		}

		private bool CanRemoveMapping()
		{
			return _selectedMapping != null;
		}

		private void Import()
		{
			IEnumerable<Tuple<string, string>> mappings;
			if (_importService.ImportSegmentMappings(this, out mappings))
			{
				_mappings.Clear();
				foreach (Tuple<string, string> mapping in mappings)
					_mappings.Add(new SegmentMappingViewModel(_projectService.Project.Segmenter, mapping.Item1, mapping.Item2));
				IsChanged = true;
			}
		}

		public SegmentMappingViewModel SelectedMapping
		{
			get { return _selectedMapping; }
			set { Set(() => SelectedMapping, ref _selectedMapping, value); }
		}

		public ObservableList<SegmentMappingViewModel> Mappings
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
