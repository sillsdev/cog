using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Components;
using SIL.Cog.Services;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class VarietiesViewModel : WorkspaceViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;
		private ReadOnlyMirroredList<Variety, VarietiesVarietyViewModel> _varieties;
		private ListCollectionView _varietiesView;
		private VarietiesVarietyViewModel _currentVariety;
		private CogProject _project;
		private bool _isVarietySelected;

		private FindViewModel _findViewModel;
		private WordViewModel _startWord;
		private readonly SimpleMonitor _selectedWordsMonitor;

		public VarietiesViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService, IBusyService busyService)
			: base("Varieties")
		{
			_spanFactory = spanFactory;
			_dialogService = dialogService;
			_busyService = busyService;

			_selectedWordsMonitor = new SimpleMonitor();

			Messenger.Default.Register<Message>(this, HandleMessage);

			TaskAreas.Add(new TaskAreaCommandsViewModel("Common tasks",
					new CommandViewModel("Add a new variety", new RelayCommand(AddNewVariety)),
					new CommandViewModel("Rename this variety", new RelayCommand(RenameCurrentVariety)), 
					new CommandViewModel("Remove this variety", new RelayCommand(RemoveCurrentVariety)),
					new CommandViewModel("Find words", new RelayCommand(Find))));

			TaskAreas.Add(new TaskAreaCommandsViewModel("Other tasks", 
				new CommandViewModel("Run stemmer on this variety", new RelayCommand(RunStemmer))));
		}

		private void HandleMessage(Message msg)
		{
			switch (msg.Type)
			{
				case MessageType.ViewChanged:
					var data = (ViewChangedData) msg.Data;
					if (data.OldViewModel == this && _findViewModel != null)
					{
						_dialogService.CloseDialog(_findViewModel);
						_findViewModel = null;
					}
					break;
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
					case FindField.Word:
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

		public override void Initialize(CogProject project)
		{
			_project = project;
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, VarietiesVarietyViewModel>(_project.Varieties,
				variety =>
					{
						var vm = new VarietiesVarietyViewModel(_dialogService, _busyService, _project, variety);
						vm.PropertyChanged += ChildPropertyChanged;
						return vm;
					}, vm => vm.ModelVariety));
			_varieties.CollectionChanged += VarietiesChanged;
			Set("VarietiesView", ref _varietiesView, new ListCollectionView(_varieties) {SortDescriptions = {new SortDescription("Name", ListSortDirection.Ascending)}});
			CurrentVariety = _varietiesView.Count > 0 ? (VarietiesVarietyViewModel) _varietiesView.GetItemAt(0) : null;
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_currentVariety == null && _varieties.Count > 0)
				CurrentVariety = _varieties[0];
		}

		private void AddNewVariety()
		{
			var vm = new EditVarietyViewModel(_project);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var variety = new Variety(vm.Name);
				_project.Varieties.Add(variety);
				IsChanged = true;
				CurrentVariety = _varieties[variety];
			}
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_varieties);
		}

		private void RenameCurrentVariety()
		{
			if (_currentVariety == null)
				return;

			var vm = new EditVarietyViewModel(_project, _currentVariety.ModelVariety);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				_currentVariety.Name = vm.Name;
				IsChanged = true;
			}
		}

		private void RemoveCurrentVariety()
		{
			if (_currentVariety == null)
				return;

			if (_dialogService.ShowYesNoQuestion(this, "Are you sure you want to remove this variety?", "Cog"))
			{
				int index = _varieties.IndexOf(_currentVariety);
				_project.Varieties.Remove(_currentVariety.ModelVariety);
				if (index == _varieties.Count)
					index--;
				CurrentVariety = _varieties.Count > 0 ? _varieties[index] : null;
				IsChanged = true;
			}
		}

		private void RunStemmer()
		{
			var vm = new RunStemmerViewModel(false);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				_busyService.ShowBusyIndicatorUntilUpdated();

				if (vm.Method == StemmingMethod.Automatic)
					_currentVariety.ModelVariety.Affixes.Clear();

				var pipeline = new Pipeline<Variety>(_project.GetStemmingProcessors(_spanFactory, vm.Method));
				pipeline.Process(_currentVariety.ModelVariety.ToEnumerable());
				IsChanged = true;
			}
		}

		public ReadOnlyObservableList<VarietiesVarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public ICollectionView VarietiesView
		{
			get { return _varietiesView; }
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
						_currentVariety.Words.SelectedWords.CollectionChanged += SelectedWordsChanged;
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

		public override bool SwitchView(Type viewType, IReadOnlyList<object> models)
		{
			if (viewType == typeof(VarietiesViewModel))
			{
				CurrentVariety = _varieties[(Variety) models[0]];
				if (models.Count > 1)
				{
					var sense = (Sense) models[1];
					_currentVariety.Words.SelectedWords.Clear();
					_currentVariety.Words.SelectedWords.AddRange(_currentVariety.Words.Words.Where(w => w.Sense.ModelSense == sense));
				}
				return true;
			}
			return false;
		}
	}
}
