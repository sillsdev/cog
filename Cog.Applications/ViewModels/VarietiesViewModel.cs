using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class VarietiesViewModel : WorkspaceViewModelBase
	{
		private readonly IDialogService _dialogService;
		private readonly IProjectService _projectService;
		private readonly IAnalysisService _analysisService;
		private readonly VarietiesVarietyViewModel.Factory _varietyFactory;

		private ReadOnlyMirroredList<Variety, VarietiesVarietyViewModel> _varieties;
		private ICollectionView _varietiesView;
		private VarietiesVarietyViewModel _currentVariety;
		private bool _isVarietySelected;
		private readonly ICommand _findCommand;
		
		private string _sortPropertyName;
		private ListSortDirection _sortDirection;

		private FindViewModel _findViewModel;
		private WordViewModel _startWord;
		private readonly SimpleMonitor _selectedWordsMonitor;

		public VarietiesViewModel(IProjectService projectService, IDialogService dialogService, IAnalysisService analysisService, VarietiesVarietyViewModel.Factory varietyFactory)
			: base("Varieties")
		{
			_projectService = projectService;
			_dialogService = dialogService;
			_analysisService = analysisService;
			_varietyFactory = varietyFactory;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			_selectedWordsMonitor = new SimpleMonitor();

			_sortPropertyName = "Sense.Gloss";
			_sortDirection = ListSortDirection.Ascending;

			Messenger.Default.Register<ViewChangedMessage>(this, HandleViewChanged);
			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);

			_findCommand = new RelayCommand(Find);

			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
					new TaskAreaCommandViewModel("Add a new variety", new RelayCommand(AddNewVariety)),
					new TaskAreaCommandViewModel("Rename this variety", new RelayCommand(RenameCurrentVariety)), 
					new TaskAreaCommandViewModel("Remove this variety", new RelayCommand(RemoveCurrentVariety)),
					new TaskAreaCommandViewModel("Find words", _findCommand),
					new TaskAreaItemsViewModel("Sort words by", new TaskAreaCommandGroupViewModel(
						new TaskAreaCommandViewModel("Sense", new RelayCommand(() => SortWordsBy("Sense.Gloss", ListSortDirection.Ascending))),
						new TaskAreaCommandViewModel("Form", new RelayCommand(() => SortWordsBy("StrRep", ListSortDirection.Ascending)))))));

			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks", 
				new TaskAreaCommandViewModel("Run stemmer on this variety", new RelayCommand(RunStemmer))));
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			CogProject project = _projectService.Project;
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, VarietiesVarietyViewModel>(project.Varieties, variety => _varietyFactory(variety), vm => vm.DomainVariety));
		}

		private void HandleSwitchView(SwitchViewMessage msg)
		{
			if (msg.ViewModelType == GetType())
			{
				CurrentVariety = _varieties[(Variety) msg.DomainModels[0]];
				if (msg.DomainModels.Count > 1)
				{
					var sense = (Sense) msg.DomainModels[1];
					_currentVariety.Words.SelectedWords.Clear();
					_currentVariety.Words.SelectedWords.AddRange(_currentVariety.Words.Words.Where(w => w.Sense.DomainSense == sense));
				}
			}
		}

		private void SortWordsBy(string propertyName, ListSortDirection sortDirection)
		{
			_sortPropertyName = propertyName;
			_sortDirection = sortDirection;

			if (_currentVariety != null)
				_currentVariety.Words.UpdateSort(_sortPropertyName, _sortDirection);
		}

		private void HandleViewChanged(ViewChangedMessage msg)
		{
			if (msg.OldViewModel == this && _findViewModel != null)
			{
				_dialogService.CloseDialog(_findViewModel);
				_findViewModel = null;
			}
		}

		private void Find()
		{
			if ( _findViewModel != null)
				return;

			_findViewModel = new FindViewModel(_dialogService, FindNext);
			_findViewModel.PropertyChanged += (sender, args) => _startWord = null;
			_dialogService.ShowModelessDialog(this, _findViewModel, () => _findViewModel = null);
		}

		private void FindNext()
		{
			if (_currentVariety == null || _currentVariety.Words.Words.Count == 0)
			{
				SearchEnded();
				return;
			}
			if (_currentVariety.Words.SelectedWords.Count == 0)
			{
				_startWord = _currentVariety.Words.WordsView.Cast<WordViewModel>().Last();
			}
			else if (_startWord == null)
			{
				_startWord = _currentVariety.Words.SelectedWords[0];
			}
			else if (_currentVariety.Words.SelectedWords.Contains(_startWord))
			{
				SearchEnded();
				return;
			}

			List<WordViewModel> words = _currentVariety.Words.WordsView.Cast<WordViewModel>().ToList();
			WordViewModel curWord = _currentVariety.Words.SelectedWords.Count == 0 ? _startWord : _currentVariety.Words.SelectedWords[0];
			int wordIndex = words.IndexOf(curWord);
			do
			{
				wordIndex = (wordIndex + 1) % words.Count;
				curWord = words[wordIndex];
				bool match = false;
				switch (_findViewModel.Field)
				{
					case FindField.Form:
						match = curWord.StrRep.Contains(_findViewModel.String);
						break;

					case FindField.Sense:
						match = curWord.Sense.Gloss.Contains(_findViewModel.String);
						break;
				}
				if (match)
				{
					using (_selectedWordsMonitor.Enter())
					{
						_currentVariety.Words.SelectedWords.Clear();
						_currentVariety.Words.SelectedWords.Add(curWord);
					}
					return;
				}
			}
			while (_startWord != curWord);
			SearchEnded();
		}

		private void SearchEnded()
		{
			_findViewModel.ShowSearchEndedMessage();
			_startWord = null;
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_currentVariety == null || !_varieties.Contains(_currentVariety))
				CurrentVariety = _varieties.Count > 0 ? _varietiesView.Cast<VarietiesVarietyViewModel>().First() : null;
		}

		private void AddNewVariety()
		{
			var vm = new EditVarietyViewModel(_projectService.Project.Varieties);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var variety = new Variety(vm.Name);
				_projectService.Project.Varieties.Add(variety);
				Messenger.Default.Send(new DomainModelChangedMessage());
				CurrentVariety = _varieties[variety];
			}
		}

		private void RenameCurrentVariety()
		{
			if (_currentVariety == null)
				return;

			var vm = new EditVarietyViewModel(_projectService.Project.Varieties, _currentVariety.DomainVariety);
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_currentVariety.Name = vm.Name;
		}

		private void RemoveCurrentVariety()
		{
			if (_currentVariety == null)
				return;

			if (_dialogService.ShowYesNoQuestion(this, "Are you sure you want to remove this variety?", "Cog"))
			{
				int index = _varieties.IndexOf(_currentVariety);
				_projectService.Project.Varieties.Remove(_currentVariety.DomainVariety);
				Messenger.Default.Send(new DomainModelChangedMessage());
				if (index == _varieties.Count)
					index--;
				CurrentVariety = _varieties.Count > 0 ? _varieties[index] : null;
			}
		}

		private void RunStemmer()
		{
			if (_currentVariety == null)
				return;

			var vm = new RunStemmerViewModel(false);
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_analysisService.Stem(vm.Method, _currentVariety.DomainVariety);
		}

		public ICommand FindCommand
		{
			get { return _findCommand; }
		}

		public ReadOnlyObservableList<VarietiesVarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public ICollectionView VarietiesView
		{
			get { return _varietiesView; }
			set
			{
				if (Set(() => VarietiesView, ref _varietiesView, value))
				{
					_varietiesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
					_varietiesView.CollectionChanged += VarietiesChanged;
					if (_currentVariety == null)
						CurrentVariety = _varieties.Count > 0 ? _varietiesView.Cast<VarietiesVarietyViewModel>().First() : null;
				}
			}
		}

		public VarietiesVarietyViewModel CurrentVariety
		{
			get { return _currentVariety; }
			set
			{
				VarietiesVarietyViewModel oldCurVariety = _currentVariety;
				if (Set(() => CurrentVariety, ref _currentVariety, value))
				{
					_startWord = null;
					if (oldCurVariety != null)
						oldCurVariety.Words.SelectedWords.CollectionChanged -= SelectedWordsChanged;
					if (_currentVariety != null)
					{
						_currentVariety.Words.UpdateSort(_sortPropertyName, _sortDirection);
						_currentVariety.Words.SelectedWords.CollectionChanged += SelectedWordsChanged;
					}
				}
				IsVarietySelected = _currentVariety != null;
			}
		}

		private void SelectedWordsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_selectedWordsMonitor.Busy)
				_startWord = null;
		}

		public bool IsVarietySelected
		{
			get { return _isVarietySelected; }
			set { Set(() => IsVarietySelected, ref _isVarietySelected, value); }
		}
	}
}
