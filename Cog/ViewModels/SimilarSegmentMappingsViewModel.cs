using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public class SimilarSegmentMappingsViewModel : ViewModelBase
	{
		private readonly IDialogService _dialogService;
		private readonly CogProject _project;
		private readonly ObservableCollection<Tuple<string, string>> _mappings;
		private Tuple<string, string> _currentMapping;
		private readonly ICommand _newCommand;
		private readonly ICommand _removeCommand;

		public SimilarSegmentMappingsViewModel(IDialogService dialogService, CogProject project)
			: this(dialogService, project, Enumerable.Empty<Tuple<string, string>>())
		{
		}

		public SimilarSegmentMappingsViewModel(IDialogService dialogService, CogProject project, IEnumerable<Tuple<string, string>> mappings)
		{
			_dialogService = dialogService;
			_project = project;
			_mappings = new ObservableCollection<Tuple<string, string>>(mappings);
			_newCommand = new RelayCommand(AddMapping);
			_removeCommand = new RelayCommand(RemoveMapping, CanRemoveMapping);
		}

		private void AddMapping()
		{
			var vm = new NewSimilarSegmentMappingViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				Tuple<string, string> mapping = Tuple.Create(vm.Segment1, vm.Segment2);
				_mappings.Add(mapping);
				CurrentMapping = mapping;
			}
		}

		private void RemoveMapping()
		{
			_mappings.Remove(_currentMapping);
		}

		private bool CanRemoveMapping()
		{
			return _currentMapping != null;
		}

		public Tuple<string, string> CurrentMapping
		{
			get { return _currentMapping; }
			set { Set(() => CurrentMapping, ref _currentMapping, value); }
		}

		public ObservableCollection<Tuple<string, string>> Mappings
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
	}
}
