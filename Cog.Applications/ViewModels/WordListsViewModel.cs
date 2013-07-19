using System;
using System.Collections.Specialized;
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
		private WordListsVarietySenseViewModel _currentVarietySense;
		private ReadOnlyMirroredList<Sense, SenseViewModel> _senses;
 		private ReadOnlyMirroredList<Variety, WordListsVarietyViewModel> _varieties;
		private bool _isEmpty;
		private readonly ICommand _findCommand;

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

			Messenger.Default.Register<ViewChangedMessage>(this, HandleViewChanged);
			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			_findCommand = new RelayCommand(Find);

			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
					new TaskAreaCommandViewModel("Add a new variety", new RelayCommand(AddNewVariety)),
 					new TaskAreaCommandViewModel("Add a new sense", new RelayCommand(AddNewSense)),
					new TaskAreaCommandViewModel("Find words", _findCommand)));

			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
					new TaskAreaCommandViewModel("Import word lists", new RelayCommand(Import)),
					new TaskAreaCommandViewModel("Export word lists", new RelayCommand(Export)),
					new TaskAreaCommandViewModel("Run stemmer", new RelayCommand(RunStemmer))));
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

		private void HandleViewChanged(ViewChangedMessage msg)
		{
			if (msg.OldViewModel == this && _findViewModel != null)
			{
				_dialogService.CloseDialog(_findViewModel);
				_findViewModel = null;
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
					CurrentVarietySense = _varieties[variety].Senses.Single(s => s.DomainSense == sense);
				}
			}
		}

		private void AddNewVariety()
		{
			var vm = new EditVarietyViewModel(_projectService.Project.Varieties);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				Messenger.Default.Send(new DomainModelChangingMessage());
				_projectService.Project.Varieties.Add(new Variety(vm.Name));
			}
		}

		private void AddNewSense()
		{
			var vm = new EditSenseViewModel(_projectService.Project.Senses);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				Messenger.Default.Send(new DomainModelChangingMessage());
				_projectService.Project.Senses.Add(new Sense(vm.Gloss, vm.Category));
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
			if (_varieties.Count > 0 && _senses.Count > 0 && _startVarietySense == null)
			{
				_startVarietySense = _currentVarietySense;
			}
			else if (_startVarietySense == _currentVarietySense)
			{
				SearchEnded();
				return;
			}
			WordListsVarietyViewModel variety = _currentVarietySense.Variety;
			WordListsVarietySenseViewModel curVarietySense = _currentVarietySense;
			int senseIndex = variety.Senses.IndexOf(curVarietySense);
			switch (_findViewModel.Field)
			{
				case FindField.Word:
					int varietyIndex = _varieties.IndexOf(variety);
					do
					{
						senseIndex++;
						if (senseIndex == _varieties[varietyIndex].Senses.Count)
						{
							varietyIndex = (varietyIndex + 1) % _varieties.Count;
							senseIndex = 0;
						}

						curVarietySense = _varieties[varietyIndex].Senses[senseIndex];
						if (curVarietySense.Words.Any(w => w.StrRep.Contains(_findViewModel.String)))
						{
							Set(() => CurrentVarietySense, ref _currentVarietySense, curVarietySense);
							return;
						}
					} while (_startVarietySense != curVarietySense);
					break;

				case FindField.Sense:
					do
					{
						senseIndex = (senseIndex + 1) % variety.Senses.Count;

						curVarietySense = variety.Senses[senseIndex];
						if (curVarietySense.DomainSense.Gloss.Contains(_findViewModel.String))
						{
							Set(() => CurrentVarietySense, ref _currentVarietySense, curVarietySense);
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
			var vm = new RunStemmerViewModel(true);
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_analysisService.StemAll(this, vm.Method);
		}

		public bool IsEmpty
		{
			get { return _isEmpty; }
			set { Set(() => IsEmpty, ref _isEmpty, value); }
		}

		public WordListsVarietySenseViewModel CurrentVarietySense
		{
			get { return _currentVarietySense; }
			set
			{
				if (Set(() => CurrentVarietySense, ref _currentVarietySense, value))
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
