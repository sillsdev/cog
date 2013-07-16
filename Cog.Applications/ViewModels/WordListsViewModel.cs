using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordListsViewModel : WorkspaceViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;
		private readonly IExportService _exportService;
		private VarietySenseViewModel _currentVarietySense;
		private CogProject _project;
		private ReadOnlyMirroredList<Sense, SenseViewModel> _senses;
 		private ReadOnlyMirroredList<Variety, WordListsVarietyViewModel> _varieties;
		private bool _isEmpty;
		private readonly IBusyService _busyService;
		private readonly ICommand _findCommand;

		private VarietySenseViewModel _startVarietySense;
		private FindViewModel _findViewModel;

		public WordListsViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService, IBusyService busyService, IImportService importService, IExportService exportService)
			: base("Word lists")
		{
			_spanFactory = spanFactory;
			_dialogService = dialogService;
			_busyService = busyService;
			_importService = importService;
			_exportService = exportService;

			Messenger.Default.Register<ViewChangedMessage>(this, HandleViewChanged);

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

		private void HandleViewChanged(ViewChangedMessage msg)
		{
			if (msg.OldViewModel == this && _findViewModel != null)
			{
				_dialogService.CloseDialog(_findViewModel);
				_findViewModel = null;
			}
		}

		private void AddNewVariety()
		{
			var vm = new EditVarietyViewModel(_project);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				Messenger.Default.Send(new DomainModelChangingMessage());
				_project.Varieties.Add(new Variety(vm.Name));
			}
		}

		private void AddNewSense()
		{
			var vm = new EditSenseViewModel(_project);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				Messenger.Default.Send(new DomainModelChangingMessage());
				_project.Senses.Add(new Sense(vm.Gloss, vm.Category));
			}
		}

		public override bool SwitchView(Type viewType, IReadOnlyList<object> models)
		{
			if (viewType == typeof(WordListsViewModel))
			{
				if (models.Count == 2)
				{
					var variety = (Variety) models[0];
					var sense = (Sense) models[1];
					CurrentVarietySense = _varieties[variety].Senses.Single(s => s.DomainSense == sense);
				}
				return true;
			}
			return false;
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
			VarietySenseViewModel curVarietySense = _currentVarietySense;
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
			_importService.ImportWordLists(this, _project);
		}

		private void Export()
		{
			_exportService.ExportWordLists(this, _project);
		}

		private void RunStemmer()
		{
			var vm = new RunStemmerViewModel(true);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				Messenger.Default.Send(new DomainModelChangingMessage());
				if (vm.Method == StemmingMethod.Automatic)
				{
					foreach (Variety variety in _project.Varieties)
						variety.Affixes.Clear();
				}

				var pipeline = new MultiThreadedPipeline<Variety>(_project.GetStemmingProcessors(_spanFactory, vm.Method));
				var progressVM = new ProgressViewModel(pvm =>
					{
						pvm.Text = "Stemming all varieties...";
						pipeline.Process(_project.Varieties);
						while (!pipeline.WaitForComplete(100))
						{
							if (pvm.Canceled)
							{
								pipeline.Cancel();
								pipeline.WaitForComplete();
							}
						}
					});
				pipeline.ProgressUpdated += (sender, e) => progressVM.Value = e.PercentCompleted;

				_dialogService.ShowModalDialog(this, progressVM);
			}
		}

		public bool IsEmpty
		{
			get { return _isEmpty; }
			set { Set(() => IsEmpty, ref _isEmpty, value); }
		}

		public VarietySenseViewModel CurrentVarietySense
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

		public override void Initialize(CogProject project)
		{
			_project = project;
			Set("Senses", ref _senses, new ReadOnlyMirroredList<Sense, SenseViewModel>(project.Senses, sense => new SenseViewModel(sense), vm => vm.DomainSense));
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, WordListsVarietyViewModel>(project.Varieties,
				variety => new WordListsVarietyViewModel(_busyService, project, variety), vm => vm.DomainVariety));
			SetIsEmpty();
			_project.Varieties.CollectionChanged += VarietiesChanged;
			_project.Senses.CollectionChanged += SensesChanged;
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
