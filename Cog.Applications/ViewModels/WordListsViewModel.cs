using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordListsViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly IExportService _exportService;
		private readonly IAnalysisService _analysisService;
		private readonly WordListsVarietyViewModel.Factory _varietyFactory; 
		private WordListsVarietySenseViewModel _selectedVarietySense;
		private ReadOnlyMirroredList<Sense, SenseViewModel> _senses;
		private ReadOnlyMirroredList<Variety, WordListsVarietyViewModel> _varieties;
		private bool _isEmpty;
		private readonly ICommand _findCommand;
		private ICollectionView _varietiesView;

		private WordListsVarietySenseViewModel _startVarietySense;
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

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			_findCommand = new RelayCommand(Find);

			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
					new TaskAreaCommandViewModel("Add a new variety", new RelayCommand(AddNewVariety)),
					new TaskAreaCommandViewModel("Add a new sense", new RelayCommand(AddNewSense)),
					new TaskAreaCommandViewModel("Find words", _findCommand),
					new TaskAreaCommandViewModel("Import word lists", new RelayCommand(Import))));

			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
					new TaskAreaCommandViewModel("Export word lists", new RelayCommand(Export)),
					new TaskAreaCommandViewModel("Remove affixes from all words", new RelayCommand(RunStemmer))));
			_isEmpty = true;
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			CogProject project = _projectService.Project;
			Set("Senses", ref _senses, new ReadOnlyMirroredList<Sense, SenseViewModel>(project.Senses, sense => new SenseViewModel(sense), vm => vm.DomainSense));
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, WordListsVarietyViewModel>(project.Varieties, variety => _varietyFactory(variety), vm => vm.DomainVariety));
			SetIsEmpty();
			project.Varieties.CollectionChanged += VarietiesChanged;
			project.Senses.CollectionChanged += SensesChanged;
		}

		protected override void OnIsSelectedChanged()
		{
			if (IsSelected)
			{
				Messenger.Default.Send(new HookFindMessage(_findCommand));
			}
			else
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
					var sense = (Sense) msg.DomainModels[1];
					SelectedVarietySense = _varieties[variety].Senses.Single(s => s.DomainSense == sense);
				}
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

		private void AddNewSense()
		{
			var vm = new EditSenseViewModel(_projectService.Project.Senses);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				_projectService.Project.Senses.Add(new Sense(vm.Gloss, vm.Category));
				Messenger.Default.Send(new DomainModelChangedMessage(true));
			}
		}

		private void Find()
		{
			if (_findViewModel != null)
				return;

			_findViewModel = new FindViewModel(_dialogService, FindNext);
			_findViewModel.PropertyChanged += (sender, args) => _startVarietySense = null;
			_dialogService.ShowModelessDialog(this, _findViewModel, () => _findViewModel = null);
		}

		private void FindNext()
		{
			WordListsVarietySenseViewModel curVarietySense = _selectedVarietySense;
			if (curVarietySense == null)
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
				if (curVariety != null && _senses.Count > 0)
					curVarietySense = curVariety.Senses.Last();
			}

			if (_varieties.Count > 0 && _senses.Count > 0 && _startVarietySense == null)
			{
				_startVarietySense = curVarietySense;
			}
			else if (curVarietySense == null || _startVarietySense == curVarietySense)
			{
				SearchEnded();
				return;
			}

			Debug.Assert(curVarietySense != null);
			WordListsVarietyViewModel variety = curVarietySense.Variety;
			int senseIndex = variety.Senses.IndexOf(curVarietySense);
			switch (_findViewModel.Field)
			{
				case FindField.Form:
					List<WordListsVarietyViewModel> varieties = _varietiesView.Cast<WordListsVarietyViewModel>().ToList();
					int varietyIndex = varieties.IndexOf(variety);
					do
					{
						senseIndex++;
						if (senseIndex == varieties[varietyIndex].Senses.Count)
						{
							varietyIndex = (varietyIndex + 1) % _varieties.Count;
							senseIndex = 0;
						}

						curVarietySense = varieties[varietyIndex].Senses[senseIndex];
						if (curVarietySense.Words.Any(w => w.StrRep.Contains(_findViewModel.String)))
						{
							Set(() => SelectedVarietySense, ref _selectedVarietySense, curVarietySense);
							return;
						}
					} while (_startVarietySense != curVarietySense);
					break;

				case FindField.Gloss:
					do
					{
						senseIndex = (senseIndex + 1) % variety.Senses.Count;

						curVarietySense = variety.Senses[senseIndex];
						if (curVarietySense.DomainSense.Gloss.Contains(_findViewModel.String))
						{
							Set(() => SelectedVarietySense, ref _selectedVarietySense, curVarietySense);
							return;
						}
					} while (_startVarietySense != curVarietySense);
					break;
			}
			SearchEnded();
		}

		private void SearchEnded()
		{
			_findViewModel.ShowSearchEndedMessage();
			_startVarietySense = null;
		}

		private void Import()
		{
			_importService.ImportWordLists(this);
		}

		private void Export()
		{
			_exportService.ExportWordLists(this);
		}

		private void RunStemmer()
		{
			if (_projectService.Project.Varieties.Count == 0 || _projectService.Project.Senses.Count == 0)
				return;

			var vm = new RunStemmerViewModel(true);
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_analysisService.StemAll(this, vm.Method);
		}

		public bool IsEmpty
		{
			get { return _isEmpty; }
			set { Set(() => IsEmpty, ref _isEmpty, value); }
		}

		public WordListsVarietySenseViewModel SelectedVarietySense
		{
			get { return _selectedVarietySense; }
			set
			{
				if (Set(() => SelectedVarietySense, ref _selectedVarietySense, value))
					_startVarietySense = null;
			}
		}

		public ICommand FindCommand
		{
			get { return _findCommand; }
		}

		public ReadOnlyObservableList<SenseViewModel> Senses
		{
			get { return _senses; }
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

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SetIsEmpty();
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SetIsEmpty();
		}

		private void SetIsEmpty()
		{
			IsEmpty = _varieties.Count == 0 && _senses.Count == 0;
		}
	}
}
