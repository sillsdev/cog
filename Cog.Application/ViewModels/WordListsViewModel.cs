using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public class WordListsViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly IExportService _exportService;
		private readonly IAnalysisService _analysisService;
		private readonly WordListsVarietyViewModel.Factory _varietyFactory; 
		private WordListsVarietyMeaningViewModel _selectedVarietyMeaning;
		private MirroredBindableList<Meaning, MeaningViewModel> _meanings;
		private MirroredBindableList<Variety, WordListsVarietyViewModel> _varieties;
		private bool _isEmpty;
		private readonly ICommand _findCommand;
		private ICollectionView _varietiesView;

		private WordListsVarietyMeaningViewModel _startVarietyMeaning;
		private FindViewModel _findViewModel;

		public WordListsViewModel(IProjectService projectService, IDialogService dialogService, IImportService importService,
			IExportService exportService, IAnalysisService analysisService, WordListsVarietyViewModel.Factory varietyFactory)
			: base("Word lists")
		{
			_projectService = projectService;
			_dialogService = dialogService;
			_importService = importService;
			_exportService = exportService;
			_analysisService = analysisService;
			_varietyFactory = varietyFactory;

			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);
			Messenger.Default.Register<DomainModelChangedMessage>(this, HandleDomainModelChanged);

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			_findCommand = new RelayCommand(Find);

			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
					new TaskAreaCommandViewModel("Add a new variety", new RelayCommand(AddNewVariety)),
					new TaskAreaCommandViewModel("Add a new meaning", new RelayCommand(AddNewMeaning)),
					new TaskAreaCommandViewModel("Find words", _findCommand),
					new TaskAreaCommandViewModel("Import word lists", new RelayCommand(Import))));

			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
					new TaskAreaCommandViewModel("Export word lists", new RelayCommand(Export, CanExport)),
					new TaskAreaCommandViewModel("Remove affixes from all words", new RelayCommand(RunStemmer, CanRunStemmer))));
			_isEmpty = true;
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			CogProject project = _projectService.Project;
			Set("Meanings", ref _meanings, new MirroredBindableList<Meaning, MeaningViewModel>(project.Meanings, meaning => new MeaningViewModel(meaning), vm => vm.DomainMeaning));
			Set("Varieties", ref _varieties, new MirroredBindableList<Variety, WordListsVarietyViewModel>(project.Varieties, variety => _varietyFactory(this, variety), vm => vm.DomainVariety));
			SetIsEmpty();
			project.Varieties.CollectionChanged += VarietiesChanged;
			project.Meanings.CollectionChanged += MeaningsChanged;
		}

		protected override void OnIsSelectedChanged()
		{
			if (IsSelected)
			{
				Messenger.Default.Send(new HookFindMessage(_findCommand));
			}
			else if (_findViewModel != null)
			{
				_dialogService.CloseDialog(_findViewModel);
				Messenger.Default.Send(new HookFindMessage(null));
			}
		}

		private void HandleSwitchView(SwitchViewMessage msg)
		{
			if (msg.ViewModelType == GetType())
			{
				if (msg.DomainModels.Count == 2)
				{
					var variety = (Variety) msg.DomainModels[0];
					var meaning = (Meaning) msg.DomainModels[1];
					SelectedVarietyMeaning = _varieties[variety].Meanings.Single(m => m.DomainMeaning == meaning);
				}
			}
		}

		private void HandleDomainModelChanged(DomainModelChangedMessage msg)
		{
			if (msg.AffectsComparison)
			{
				foreach (WordListsVarietyViewModel variety in _varieties)
					variety.CheckForErrors();
			}
		}

		private void AddNewVariety()
		{
			var vm = new EditVarietyViewModel(_projectService.Project.Varieties);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var variety = new Variety(vm.Name);
				_projectService.Project.Varieties.Add(variety);
				_analysisService.Segment(variety);
				Messenger.Default.Send(new DomainModelChangedMessage(true));
			}
		}

		private void AddNewMeaning()
		{
			var vm = new EditMeaningViewModel(_projectService.Project.Meanings);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				_projectService.Project.Meanings.Add(new Meaning(vm.Gloss, vm.Category));
				Messenger.Default.Send(new DomainModelChangedMessage(true));
			}
		}

		private void Find()
		{
			if (_findViewModel != null)
				return;

			_findViewModel = new FindViewModel(_dialogService, FindNext);
			_findViewModel.PropertyChanged += (sender, args) => _startVarietyMeaning = null;
			_dialogService.ShowModelessDialog(this, _findViewModel, () => _findViewModel = null);
		}

		private void FindNext()
		{
			WordListsVarietyMeaningViewModel curVarietyMeaning = _selectedVarietyMeaning;
			if (curVarietyMeaning == null)
			{
				WordListsVarietyViewModel curVariety = null;
				switch (_findViewModel.Field)
				{
					case FindField.Form:
						curVariety = _varietiesView.Cast<WordListsVarietyViewModel>().LastOrDefault();
						break;
					case FindField.Gloss:
						curVariety = _varietiesView.Cast<WordListsVarietyViewModel>().FirstOrDefault();
						break;
				}
				if (curVariety != null && _meanings.Count > 0)
					curVarietyMeaning = curVariety.Meanings.Last();
			}

			if (_varieties.Count > 0 && _meanings.Count > 0 && _startVarietyMeaning == null)
			{
				_startVarietyMeaning = curVarietyMeaning;
			}
			else if (curVarietyMeaning == null || _startVarietyMeaning == curVarietyMeaning)
			{
				SearchEnded();
				return;
			}

			Debug.Assert(curVarietyMeaning != null);
			WordListsVarietyViewModel variety = curVarietyMeaning.Variety;
			int meaningIndex = variety.Meanings.IndexOf(curVarietyMeaning);
			switch (_findViewModel.Field)
			{
				case FindField.Form:
					List<WordListsVarietyViewModel> varieties = _varietiesView.Cast<WordListsVarietyViewModel>().ToList();
					int varietyIndex = varieties.IndexOf(variety);
					do
					{
						meaningIndex++;
						if (meaningIndex == varieties[varietyIndex].Meanings.Count)
						{
							varietyIndex = (varietyIndex + 1) % _varieties.Count;
							meaningIndex = 0;
						}

						curVarietyMeaning = varieties[varietyIndex].Meanings[meaningIndex];
						if (curVarietyMeaning.Words.Any(w => w.StrRep.Contains(_findViewModel.String)))
						{
							Set(() => SelectedVarietyMeaning, ref _selectedVarietyMeaning, curVarietyMeaning);
							return;
						}
					} while (_startVarietyMeaning != curVarietyMeaning);
					break;

				case FindField.Gloss:
					do
					{
						meaningIndex = (meaningIndex + 1) % variety.Meanings.Count;

						curVarietyMeaning = variety.Meanings[meaningIndex];
						if (curVarietyMeaning.DomainMeaning.Gloss.Contains(_findViewModel.String))
						{
							Set(() => SelectedVarietyMeaning, ref _selectedVarietyMeaning, curVarietyMeaning);
							return;
						}
					} while (_startVarietyMeaning != curVarietyMeaning);
					break;
			}
			SearchEnded();
		}

		private void SearchEnded()
		{
			_findViewModel.ShowSearchEndedMessage();
			_startVarietyMeaning = null;
		}

		private void Import()
		{
			_importService.ImportWordLists(this);
		}

		private bool CanExport()
		{
			return !_isEmpty;
		}

		private void Export()
		{
			_exportService.ExportWordLists(this);
		}

		private bool CanRunStemmer()
		{
			return !_isEmpty;
		}

		private void RunStemmer()
		{
			var vm = new RunStemmerViewModel(true);
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_analysisService.StemAll(this, vm.Method);
		}

		public bool IsEmpty
		{
			get { return _isEmpty; }
			private set { Set(() => IsEmpty, ref _isEmpty, value); }
		}

		public WordListsVarietyMeaningViewModel SelectedVarietyMeaning
		{
			get { return _selectedVarietyMeaning; }
			set
			{
				if (Set(() => SelectedVarietyMeaning, ref _selectedVarietyMeaning, value))
					_startVarietyMeaning = null;
			}
		}

		public ICommand FindCommand
		{
			get { return _findCommand; }
		}

		public ReadOnlyObservableList<MeaningViewModel> Meanings
		{
			get { return _meanings; }
		}

		public ReadOnlyObservableList<WordListsVarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public ICollectionView VarietiesView
		{
			get { return _varietiesView; }
			set { Set(() => VarietiesView, ref _varietiesView, value); }
		}

		private void MeaningsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SetIsEmpty();
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SetIsEmpty();
		}

		private void SetIsEmpty()
		{
			IsEmpty = _varieties.Count == 0 && _meanings.Count == 0;
		}
	}
}
