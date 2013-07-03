using System;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Command;
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
		private readonly IProgressService _progressService;
		private ReadOnlyMirroredList<Variety, VarietiesVarietyViewModel> _varieties;
		private VarietiesVarietyViewModel _currentVariety;
		private CogProject _project;
		private bool _isVarietySelected;

		public VarietiesViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService, IProgressService progressService)
			: base("Varieties")
		{
			_spanFactory = spanFactory;
			_dialogService = dialogService;
			_progressService = progressService;

			TaskAreas.Add(new TaskAreaCommandsViewModel("Common tasks",
					new CommandViewModel("Add a new variety", new RelayCommand(AddNewVariety)),
					new CommandViewModel("Rename this variety", new RelayCommand(RenameCurrentVariety)), 
					new CommandViewModel("Remove this variety", new RelayCommand(RemoveCurrentVariety))));

			TaskAreas.Add(new TaskAreaCommandsViewModel("Other tasks", 
				new CommandViewModel("Run stemmer on this variety", new RelayCommand(RunStemmer))));
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			if (_varieties != null)
				_varieties.CollectionChanged -= VarietiesChanged;
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, VarietiesVarietyViewModel>(_project.Varieties,
				variety =>
					{
						var vm = new VarietiesVarietyViewModel(_dialogService, _project, variety);
						vm.PropertyChanged += ChildPropertyChanged;
						return vm;
					}, vm => vm.ModelVariety));
			_varieties.CollectionChanged += VarietiesChanged;
			CurrentVariety = _varieties.Count > 0 ? _varieties[0] : null;
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_currentVariety == null && _varieties.Count > 0)
				CurrentVariety = _varieties[0];
		}

		private void AddNewVariety()
		{
			var vm = new EditVarietyViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				var variety = new Variety(vm.Name);
				_project.Varieties.Add(variety);
				IsChanged = true;
				SwitchVariety(variety);
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
			if (_dialogService.ShowDialog(this, vm) == true)
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
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				if (vm.Method == StemmingMethod.Automatic)
					_currentVariety.ModelVariety.Affixes.Clear();

				var pipeline = new Pipeline<Variety>(_project.GetStemmingProcessors(_spanFactory, vm.Method));
				_progressService.ShowProgress(() => pipeline.Process(_currentVariety.ModelVariety.ToEnumerable()));
				IsChanged = true;
			}
		}

		public ReadOnlyObservableList<VarietiesVarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public VarietiesVarietyViewModel CurrentVariety
		{
			get { return _currentVariety; }
			set
			{
				Set(() => CurrentVariety, ref _currentVariety, value);
				IsVarietySelected = _currentVariety != null;
			}
		}

		public bool IsVarietySelected
		{
			get { return _isVarietySelected; }
			set { Set(() => IsVarietySelected, ref _isVarietySelected, value); }
		}

		public override bool SwitchView(Type viewType, object model)
		{
			if (base.SwitchView(viewType, model))
			{
				SwitchVariety((Variety) model);
				return true;
			}
			return false;
		}

		private void SwitchVariety(Variety variety)
		{
			CurrentVariety = _varieties.Single(vm => vm.ModelVariety == variety);
		}
	}
}
