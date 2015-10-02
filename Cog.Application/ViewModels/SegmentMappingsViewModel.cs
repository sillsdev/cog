using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
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
				_mappings.ReplaceAll(mappings.Select(m => new SegmentMappingViewModel(_projectService.Project.Segmenter, m.Item1, m.Item2)));
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

		internal void ReplaceAllValidMappings(IEnumerable<UnorderedTuple<string, string>> mappings)
		{
			using (_mappings.BulkUpdate())
			{
				_mappings.RemoveAll(m => m.IsValid);
				_mappings.AddRange(mappings.Select(m => new SegmentMappingViewModel(_projectService.Project.Segmenter, m.Item1, m.Item2)));
				IsChanged = true;
			}
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
