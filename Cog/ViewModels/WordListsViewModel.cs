using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
		private readonly IProgressService _progressService;
		private CogProject _project;
		private ListViewModelCollection<ObservableCollection<Sense>, SenseViewModel, Sense> _senses;
 		private ListViewModelCollection<ObservableCollection<Variety>, VarietyWordListsViewModel, Variety> _varieties;
		private bool _isEmpty;

		public WordListsViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService, IProgressService progressService)
			: base("Word lists")
		{
			_spanFactory = spanFactory;
			_dialogService = dialogService;
			_progressService = progressService;

			TaskAreas.Add(new TaskAreaViewModel("Common tasks",
					new CommandViewModel("Add a new variety", new RelayCommand(AddNewVariety)),
 					new CommandViewModel("Add a new sense", new RelayCommand(AddNewSense))));

			TaskAreas.Add(new TaskAreaViewModel("Other tasks",
					new CommandViewModel("Import word lists", new RelayCommand(Import)),
					new CommandViewModel("Export word lists", new RelayCommand(Export)),
					new CommandViewModel("Run stemmer", new RelayCommand(RunStemmer))));
			_isEmpty = true;
		}

		private void AddNewVariety()
		{
			var vm = new EditVarietyViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				_project.Varieties.Add(new Variety(vm.Name));
				IsChanged = true;
			}
		}

		private void AddNewSense()
		{
			var vm = new EditSenseViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				_project.Senses.Add(new Sense(vm.Gloss, vm.Category));
				IsChanged = true;
			}
		}

		private void Import()
		{
			if (ViewModelUtilities.ImportWordLists(_dialogService, _project, this))
				IsChanged = true;
		}

		private void Export()
		{
			ViewModelUtilities.ExportWordLists(_dialogService, _project, this);
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
						processors = new[] {_project.VarietyProcessors["affixIdentifier"], new Stemmer(_spanFactory, _project)};
						break;
					case StemmingMethod.Hybrid:
						processors = new[] {_project.VarietyProcessors["affixIdentifier"], new Stemmer(_spanFactory, _project)};
						break;
					case StemmingMethod.Manual:
						processors = new[] {new Stemmer(_spanFactory, _project)};
						break;
				}
				Debug.Assert(processors != null);
				var pipeline = new Pipeline<Variety>(processors);
				_progressService.ShowProgress(this, () => pipeline.Process(_project.Varieties));
				IsChanged = true;
			}
		}

		public bool IsEmpty
		{
			get { return _isEmpty; }
			set { Set(() => IsEmpty, ref _isEmpty, value); }
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
			Set("Senses", ref _senses, new ListViewModelCollection<ObservableCollection<Sense>, SenseViewModel, Sense>(project.Senses, sense => new SenseViewModel(sense)));
			Set("Varieties", ref _varieties, new ListViewModelCollection<ObservableCollection<Variety>, VarietyWordListsViewModel, Variety>(project.Varieties, variety =>
				{
					var vm = new VarietyWordListsViewModel(project, variety);
					vm.PropertyChanged += ChildPropertyChanged;
					return vm;
				}));
			SetIsEmpty();
			_project.Varieties.CollectionChanged += VarietiesChanged;
			_project.Senses.CollectionChanged += SensesChanged;
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_varieties);
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
