using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Processors;
using SIL.Cog.Services;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class WordListsViewModel : WorkspaceViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private readonly IDialogService _dialogService;
		private CogProject _project;
		private ViewModelCollection<SenseViewModel, Sense> _senses;
 		private ViewModelCollection<VarietyWordListsViewModel, Variety> _varieties;

		public WordListsViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService)
			: base("Word lists")
		{
			_spanFactory = spanFactory;
			_dialogService = dialogService;
			TaskAreas.Add(new TaskAreaViewModel("Common tasks", new []
				{
					new CommandViewModel("Add a new variety", new RelayCommand(AddNewVariety)),
 					new CommandViewModel("Add a new sense", new RelayCommand(AddNewSense))
				}));
			TaskAreas.Add(new TaskAreaViewModel("Other tasks", new []
				{
					new CommandViewModel("Import word lists", new RelayCommand(() => ViewModelUtilities.ImportWordLists(_dialogService, _project, this))),
					new CommandViewModel("Run stemmer", new RelayCommand(RunStemmer))
				}));
		}

		private void AddNewVariety()
		{
			var vm = new EditVarietyViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
				_project.Varieties.Add(new Variety(vm.Name));
		}

		private void AddNewSense()
		{
			var vm = new EditSenseViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
				_project.Senses.Add(new Sense(vm.Gloss, vm.Category));
		}

		private void RunStemmer()
		{
			var vm = new RunStemmerViewModel(true);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				IEnumerable<IProcessor<Variety>> processors = null;
				switch (vm.Method)
				{
					case StemmingMethod.Automatic:
						foreach (Variety variety in _project.Varieties)
							variety.Affixes.Clear();
						processors = new[] {_project.VarietyProcessors["affixIdentifier"], new Stemmer(_spanFactory)};
						break;
					case StemmingMethod.Hybrid:
						processors = new[] {_project.VarietyProcessors["affixIdentifier"], new Stemmer(_spanFactory)};
						break;
					case StemmingMethod.Manual:
						processors = new[] {new Stemmer(_spanFactory)};
						break;
				}
				Debug.Assert(processors != null);
				var pipeline = new MultiThreadedPipeline<Variety>(processors);

				var progressVM = new ProgressViewModel(pvm =>
					{
						pvm.Text = "Stemming all varieties...";
						pipeline.ProgressUpdated += (sender, e) => pvm.Value = e.PercentCompleted;
						pipeline.Process(_project.Varieties);
					});

				if (_dialogService.ShowDialog(this, progressVM) == false)
					pipeline.Cancel();
			}
		}

		public ObservableCollection<SenseViewModel> Senses
		{
			get { return _senses; }
		}

		public ObservableCollection<VarietyWordListsViewModel> Varieties
		{
			get { return _varieties; }
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Set("Senses", ref _senses, new ViewModelCollection<SenseViewModel, Sense>(project.Senses, sense => new SenseViewModel(sense)));
			Set("Varieties", ref _varieties, new ViewModelCollection<VarietyWordListsViewModel, Variety>(project.Varieties, variety => new VarietyWordListsViewModel(project, variety)));
		}
	}
}
