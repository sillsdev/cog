using System;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Collections;
using SIL.Cog.Components;
using SIL.Cog.Services;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public enum CurrentVarietyPairState
	{
		SelectedAndCompared,
		SelectedAndNotCompared,
		NotSelected,
	}

	public class VarietyPairsViewModel : WorkspaceViewModelBase
	{
		private CogProject _project;
		private readonly IProgressService _progressService;
		private ReadOnlyMirroredList<Variety, VarietyViewModel> _varieties;
		private VarietyViewModel _currentVariety1;
		private VarietyViewModel _currentVariety2;
		private VarietyPairViewModel _currentVarietyPair;
		private CurrentVarietyPairState _currentVarietyPairState;
		private readonly IExportService _exportService;

		public VarietyPairsViewModel(IProgressService progressService, IExportService exportService)
			: base("Variety Pairs")
		{
			_progressService = progressService;
			_exportService = exportService;
			Messenger.Default.Register<NotificationMessage>(this, HandleNotificationMessage);
			_currentVarietyPairState = CurrentVarietyPairState.NotSelected;
			TaskAreas.Add(new TaskAreaCommandsViewModel("Common tasks", 
				new CommandViewModel("Perform comparison on this variety pair", new RelayCommand(PerformComparison))));
			TaskAreas.Add(new TaskAreaCommandsViewModel("Other tasks",
				new CommandViewModel("Export results for this variety pair", new RelayCommand(ExportVarietyPair))));
		}

		private void PerformComparison()
		{
			if (_currentVarietyPairState == CurrentVarietyPairState.NotSelected)
				return;

			Messenger.Default.Send(new NotificationMessage(Notifications.PerformingComparison));
			if (_currentVarietyPair != null)
				_project.VarietyPairs.Remove(_currentVarietyPair.ModelVarietyPair);

			var pair = new VarietyPair(_currentVariety1.ModelVariety, _currentVariety2.ModelVariety);
			_project.VarietyPairs.Add(pair);

			var pipeline = new Pipeline<VarietyPair>(_project.GetComparisonProcessors());
			_progressService.ShowProgress(() =>
				{
					pipeline.Process(pair.ToEnumerable());
					Messenger.Default.Send(new NotificationMessage(Notifications.ComparisonPerformed));
				});
		}

		private void ExportVarietyPair()
		{
			if (_currentVarietyPairState == CurrentVarietyPairState.NotSelected)
				return;

			_exportService.ExportVarietyPair(this, _project, _currentVarietyPair.ModelVarietyPair);
		}

		private void HandleNotificationMessage(NotificationMessage msg)
		{
			switch (msg.Notification)
			{
				case Notifications.ComparisonPerformed:
					SetCurrentVarietyPair();
					break;
			}
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			_project.VarietyPairs.CollectionChanged += VarietyPairsChanged;
			Set("Varieties", ref _varieties, new ReadOnlyMirroredList<Variety, VarietyViewModel>(_project.Varieties, variety => new VarietyViewModel(variety), vm => vm.ModelVariety));
			ResetCurrentVarietyPair();
			((INotifyCollectionChanged) _varieties).CollectionChanged += VarietiesChanged;
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ResetCurrentVarietyPair();
		}

		private void ResetCurrentVarietyPair()
		{
			if (_varieties.Count > 0)
			{
				VarietyViewModel[] varieties = _varieties.OrderBy(v => v.Name).ToArray();
				Set(() => CurrentVariety1, ref _currentVariety1, varieties[0]);
				if (_varieties.Count > 1)
					Set(() => CurrentVariety2, ref _currentVariety2, varieties[1]);
				else
					Set(() => CurrentVariety2, ref _currentVariety2, varieties[0]);
				SetCurrentVarietyPair();
			}
		}

		private void VarietyPairsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Remove:
					if (_currentVarietyPair != null && e.OldItems.Cast<VarietyPair>().Any(vp => vp == _currentVarietyPair.ModelVarietyPair))
						CurrentVarietyPairState = CurrentVarietyPairState.SelectedAndNotCompared;
					break;

				case NotifyCollectionChangedAction.Reset:
					ResetCurrentVarietyPair();
					break;
			}
		}

		public ReadOnlyObservableList<VarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public VarietyViewModel CurrentVariety1
		{
			get { return _currentVariety1; }
			set
			{
				if (Set(() => CurrentVariety1, ref _currentVariety1, value))
					SetCurrentVarietyPair();
			}
		}

		public VarietyViewModel CurrentVariety2
		{
			get { return _currentVariety2; }
			set
			{
				if (Set(() => CurrentVariety2, ref _currentVariety2, value))
					SetCurrentVarietyPair();
			}
		}

		public VarietyPairViewModel CurrentVarietyPair
		{
			get { return _currentVarietyPair; }
			set
			{
				if (Set(() => CurrentVarietyPair, ref _currentVarietyPair, value))
				{
					if (_currentVarietyPair == null)
					{
						Set(() => CurrentVariety1, ref _currentVariety1, _varieties.Count > 0 ? _varieties[0] : null);
						Set(() => CurrentVariety2, ref _currentVariety2, _varieties.Count > 1 ? _varieties[1] : null);
						CurrentVarietyPairState = CurrentVarietyPairState.NotSelected;
					}
					else
					{
						Set(() => CurrentVariety1, ref _currentVariety1, _varieties.First(v => v.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety1));
						Set(() => CurrentVariety2, ref _currentVariety2, _varieties.First(v => v.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety2));
						CurrentVarietyPairState = _currentVariety1.ModelVariety.VarietyPairs.Contains(_currentVariety2.ModelVariety)
							                          ? CurrentVarietyPairState.SelectedAndCompared : CurrentVarietyPairState.SelectedAndNotCompared;
					}
				}
			}
		}

		public CurrentVarietyPairState CurrentVarietyPairState
		{
			get { return _currentVarietyPairState; }
			set { Set(() => CurrentVarietyPairState, ref _currentVarietyPairState, value); }
		}

		private void SetCurrentVarietyPair()
		{
			VarietyPairViewModel vm = null;
			var state = CurrentVarietyPairState.NotSelected;
			if (_currentVariety1 != null && _currentVariety2 != null && _currentVariety1 != _currentVariety2)
			{
				VarietyPair pair;
				if (_currentVariety1.ModelVariety.VarietyPairs.TryGetValue(_currentVariety2.ModelVariety, out pair))
				{
					vm = new VarietyPairViewModel(_project, pair, _currentVariety1.ModelVariety == pair.Variety1);
					state = CurrentVarietyPairState.SelectedAndCompared;
				}
				else
				{
					state = CurrentVarietyPairState.SelectedAndNotCompared;
				}
			}
			Set(() => CurrentVarietyPair, ref _currentVarietyPair, vm);
			CurrentVarietyPairState = state;
		}

		public override bool SwitchView(Type viewType, object model)
		{
			if (base.SwitchView(viewType, model))
			{
				var pair = (VarietyPair) model;
				CurrentVarietyPair = new VarietyPairViewModel(_project, pair, true);
				return true;
			}

			return false;
		}
	}
}
