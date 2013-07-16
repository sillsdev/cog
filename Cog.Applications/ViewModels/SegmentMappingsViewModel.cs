using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class SegmentMappingsViewModel : ChangeTrackingViewModelBase
	{
		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly CogProject _project;
		private readonly BindableList<SegmentMappingViewModel> _mappings;
		private SegmentMappingViewModel _currentMapping;
		private readonly ICommand _newCommand;
		private readonly ICommand _removeCommand;
		private readonly ICommand _importCommand;

		public SegmentMappingsViewModel(IDialogService dialogService, IImportService importService, CogProject project)
			: this(dialogService, importService, project, Enumerable.Empty<Tuple<string, string>>())
		{
		}

		public SegmentMappingsViewModel(IDialogService dialogService, IImportService importService, CogProject project, IEnumerable<Tuple<string, string>> mappings)
		{
			_dialogService = dialogService;
			_importService = importService;
			_project = project;
			_mappings = new BindableList<SegmentMappingViewModel>(mappings.Select(mapping => new SegmentMappingViewModel(_project, mapping.Item1, mapping.Item2)));
			_newCommand = new RelayCommand(AddMapping);
			_removeCommand = new RelayCommand(RemoveMapping, CanRemoveMapping);
			_importCommand = new RelayCommand(Import);
		}

		private void AddMapping()
		{
			var vm = new NewSegmentMappingViewModel(_project);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var mapping = new SegmentMappingViewModel(_project, vm.Segment1, vm.Segment2);
				_mappings.Add(mapping);
				CurrentMapping = mapping;
				IsChanged = true;
			}
		}

		private void RemoveMapping()
		{
			_mappings.Remove(_currentMapping);
			IsChanged = true;
		}

		private bool CanRemoveMapping()
		{
			return _currentMapping != null;
		}

		private void Import()
		{
			IEnumerable<Tuple<string, string>> mappings;
			if (_importService.ImportSegmentMappings(this, out mappings))
			{
				_mappings.Clear();
				foreach (Tuple<string, string> mapping in mappings)
					_mappings.Add(new SegmentMappingViewModel(_project, mapping.Item1, mapping.Item2));
				IsChanged = true;
			}
		}

		public SegmentMappingViewModel CurrentMapping
		{
			get { return _currentMapping; }
			set { Set(() => CurrentMapping, ref _currentMapping, value); }
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
