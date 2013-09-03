using System;
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
		private VarietiesVarietyViewModel _selectedVariety;
		private bool _isVarietySelected;
		private readonly ICommand _findCommand;
		
		private string _sortPropertyName;
		private ListSortDirection _sortDirection;

		private FindViewModel _findViewModel;

		public VarietiesViewModel(IProjectService projectService, IDialogService dialogService, IAnalysisService analysisService, VarietiesVarietyViewModel.Factory varietyFactory)
			: base("Varieties")
		{
			_projectService = projectService;
			_dialogService = dialogService;
			_analysisService = analysisService;
			_varietyFactory = varietyFactory;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			_sortPropertyName = "Sense.Gloss";
			_sortDirection = ListSortDirection.Ascending;

			Messenger.Default.Register<SwitchViewMessage>(this, HandleSwitchView);

			_findCommand = new RelayCommand(Find);

			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
					new TaskAreaCommandViewModel("Add a new variety", new RelayCommand(AddNewVariety)),
					new TaskAreaCommandViewModel("Rename this variety", new RelayCommand(RenameSelectedVariety)), 
					new TaskAreaCommandViewModel("Remove this variety", new RelayCommand(RemoveSelectedVariety)),
					new TaskAreaCommandViewModel("Find words", _findCommand),
					new TaskAreaItemsViewModel("Sort words by", new TaskAreaCommandGroupViewModel(
						new TaskAreaCommandViewModel("Sense", new RelayCommand(() => SortWordsBy("Sense.Gloss", ListSortDirection.Ascending))),
						new TaskAreaCommandViewModel("Form", new RelayCommand(() => SortWordsBy("StrRep", ListSortDirection.Ascending)))))));

			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks", 
				new TaskAreaCommandViewModel("Remove affixes from words in this variety", new RelayCommand(RunStemmer))));
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			CogProject project = _projectService.Project;
			SelectedVariety = null;
			VarietiesView = null;
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, VarietiesVarietyViewModel>(project.Varieties, variety => _varietyFactory(variety), vm => vm.DomainVariety));
		}

		private void HandleSwitchView(SwitchViewMessage msg)
		{
			if (msg.ViewModelType == GetType())
			{
				SelectedVariety = _varieties[(Variety) msg.DomainModels[0]];
				if (msg.DomainModels.Count > 1)
				{
					var sense = (Sense) msg.DomainModels[1];
					_selectedVariety.Words.SelectedWords.Clear();
					_selectedVariety.Words.SelectedWords.AddRange(_selectedVariety.Words.Words.Where(w => w.Sense.DomainSense == sense));
				}
			}
		}

		private void SortWordsBy(string propertyName, ListSortDirection sortDirection)
		{
			_sortPropertyName = propertyName;
			_sortDirection = sortDirection;

			if (_selectedVariety != null)
				_selectedVariety.Words.UpdateSort(_sortPropertyName, _sortDirection);
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

		private void Find()
		{
			if ( _findViewModel != null)
				return;

			_findViewModel = new FindViewModel(_dialogService, FindNext);
			_findViewModel.PropertyChanged += (sender, args) => _selectedVariety.Words.ResetSearch();
			_dialogService.ShowModelessDialog(this, _findViewModel, () => _findViewModel = null);
		}

		private void FindNext()
		{
			if (_selectedVariety == null)
				_findViewModel.ShowSearchEndedMessage();
			else if (!_selectedVariety.Words.FindNext(_findViewModel.Field, _findViewModel.String))
				_findViewModel.ShowSearchEndedMessage();
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_selectedVariety == null || !_varieties.Contains(_selectedVariety))
				SelectedVariety = _varieties.Count > 0 ? _varietiesView.Cast<VarietiesVarietyViewModel>().First() : null;
		}

		private void AddNewVariety()
		{
			var vm = new EditVarietyViewModel(_projectService.Project.Varieties);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var variety = new Variety(vm.Name);
				_projectService.Project.Varieties.Add(variety);
				Messenger.Default.Send(new DomainModelChangedMessage(true));
				SelectedVariety = _varieties[variety];
			}
		}

		private void RenameSelectedVariety()
		{
			if (_selectedVariety == null)
				return;

			var vm = new EditVarietyViewModel(_projectService.Project.Varieties, _selectedVariety.DomainVariety);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				_selectedVariety.DomainVariety.Name = vm.Name;
				Messenger.Default.Send(new DomainModelChangedMessage(false));
			}
		}

		private void RemoveSelectedVariety()
		{
			if (_selectedVariety == null)
				return;

			if (_dialogService.ShowYesNoQuestion(this, "Are you sure you want to remove this variety?", "Cog"))
			{
				int index = _varieties.IndexOf(_selectedVariety);
				_projectService.Project.Varieties.Remove(_selectedVariety.DomainVariety);
				Messenger.Default.Send(new DomainModelChangedMessage(true));
				if (index == _varieties.Count)
					index--;
				SelectedVariety = _varieties.Count > 0 ? _varieties[index] : null;
			}
		}

		private void RunStemmer()
		{
			if (_selectedVariety == null)
				return;

			var vm = new RunStemmerViewModel(false);
			if (_dialogService.ShowModalDialog(this, vm) == true)
				_analysisService.Stem(vm.Method, _selectedVariety.DomainVariety);
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
					if (_varietiesView != null)
					{
						_varietiesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
						_varietiesView.CollectionChanged += VarietiesChanged;
						if (_selectedVariety == null)
							SelectedVariety = _varieties.Count > 0 ? _varietiesView.Cast<VarietiesVarietyViewModel>().First() : null;
					}
				}
			}
		}

		public VarietiesVarietyViewModel SelectedVariety
		{
			get { return _selectedVariety; }
			set
			{
				if (Set(() => SelectedVariety, ref _selectedVariety, value))
				{
					if (_selectedVariety != null)
					{
						_selectedVariety.Words.UpdateSort(_sortPropertyName, _sortDirection);
						_selectedVariety.Words.ResetSearch();
					}
				}
				IsVarietySelected = _selectedVariety != null;
			}
		}

		public bool IsVarietySelected
		{
			get { return _isVarietySelected; }
			set { Set(() => IsVarietySelected, ref _isVarietySelected, value); }
		}
	}
}
