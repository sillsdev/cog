using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;

namespace SIL.Cog.ViewModels
{
	public class VarietyPairsViewModel : WorkspaceViewModelBase
	{
		private CogProject _project;
		private ViewModelCollection<VarietyViewModel, Variety> _varieties;
		private VarietyViewModel _currentVariety1;
		private VarietyViewModel _currentVariety2;
		private VarietyPairViewModel _currentVarietyPair;

		public VarietyPairsViewModel()
			: base("Variety Pairs")
		{
			Messenger.Default.Register<NotificationMessage>(this, HandleNotificationMessage);
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
			if (_project != null)
			{
				_project.VarietyPairs.CollectionChanged -= VarietyPairsChanged;
				_varieties.CollectionChanged -= VarietiesChanged;
			}

			_project = project;
			_project.VarietyPairs.CollectionChanged += VarietyPairsChanged;
			Set("Varieties", ref _varieties, new ViewModelCollection<VarietyViewModel, Variety>(_project.Varieties, variety => new VarietyViewModel(variety)));
			if (_varieties.Count > 0)
			{
				Set("CurrentVariety1", ref _currentVariety1, _varieties[0]);
				if (_varieties.Count > 1)
					Set("CurrentVariety2", ref _currentVariety2, _varieties[1]);
			}
			_varieties.CollectionChanged += VarietiesChanged;
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_varieties.Count > 0 && _currentVariety1 == null)
			{
				Set("CurrentVariety1", ref _currentVariety1, _varieties[0]);
			}
			if (_varieties.Count > 1 && _currentVariety2 == null)
			{
				Set("CurrentVariety2", ref _currentVariety2, _varieties[1]);
				SetCurrentVarietyPair();
			}
		}

		private void VarietyPairsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					VarietyPairsAdded(e.NewItems.Cast<VarietyPair>());
					break;

				case NotifyCollectionChangedAction.Remove:
					VarietyPairsRemoved(e.OldItems.Cast<VarietyPair>());
					break;

				case NotifyCollectionChangedAction.Replace:
					VarietyPairsRemoved(e.OldItems.Cast<VarietyPair>());
					VarietyPairsAdded(e.NewItems.Cast<VarietyPair>());
					break;

				case NotifyCollectionChangedAction.Reset:
					Set("CurrentVarietyPair", ref _currentVarietyPair, null);
					break;
			}
		}

		private void VarietyPairsRemoved(IEnumerable<VarietyPair> pairs)
		{
			if (_currentVarietyPair != null && pairs.Any(vp => vp == _currentVarietyPair.ModelVarietyPair))
				CurrentVarietyPair = null;
		}

		private void VarietyPairsAdded(IEnumerable<VarietyPair> pairs)
		{
			foreach (VarietyPair pair in pairs)
			{
				if (_currentVarietyPair == null && _currentVariety1 != null && _currentVariety2 != null &&
					((pair.Variety1 == _currentVariety1.ModelVariety && pair.Variety2 == _currentVariety2.ModelVariety)
					|| (pair.Variety1 == _currentVariety2.ModelVariety && pair.Variety2 == _currentVariety1.ModelVariety)))
				{
					Set("CurrentVarietyPair", ref _currentVarietyPair, new VarietyPairViewModel(_project, pair));
					break;
				}
			}
		}

		public ObservableCollection<VarietyViewModel> Varieties
		{
			get { return _varieties; }
		}

		public VarietyViewModel CurrentVariety1
		{
			get { return _currentVariety1; }
			set
			{
				if (Set("CurrentVariety1", ref _currentVariety1, value))
					SetCurrentVarietyPair();
			}
		}

		public VarietyViewModel CurrentVariety2
		{
			get { return _currentVariety2; }
			set
			{
				if (Set("CurrentVariety2", ref _currentVariety2, value))
					SetCurrentVarietyPair();
			}
		}

		public VarietyPairViewModel CurrentVarietyPair
		{
			get { return _currentVarietyPair; }
			set
			{
				Set("CurrentVarietyPair", ref _currentVarietyPair, value);
				if (_currentVarietyPair == null)
				{
					Set("CurrentVariety1", ref _currentVariety1, _varieties.Count > 0 ? _varieties[0] : null);
					Set("CurrentVariety2", ref _currentVariety2, _varieties.Count > 1 ? _varieties[1] : null);
				}
				else if (_currentVariety1.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety1)
				{
					if (_currentVariety2.ModelVariety != _currentVarietyPair.ModelVarietyPair.Variety2)
						Set("CurrentVariety2", ref _currentVariety2, _varieties.First(v => v.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety2));
				}
				else if (_currentVariety2.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety1)
				{
					if (_currentVariety1.ModelVariety != _currentVarietyPair.ModelVarietyPair.Variety2)
						Set("CurrentVariety1", ref _currentVariety1, _varieties.First(v => v.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety2));
				}
				else if (_currentVariety1.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety2)
				{
					if (_currentVariety2.ModelVariety != _currentVarietyPair.ModelVarietyPair.Variety1)
						Set("CurrentVariety2", ref _currentVariety2, _varieties.First(v => v.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety1));
				}
				else if (_currentVariety2.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety2)
				{
					if (_currentVariety1.ModelVariety != _currentVarietyPair.ModelVarietyPair.Variety1)
						Set("CurrentVariety1", ref _currentVariety1, _varieties.First(v => v.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety1));
				}
				else
				{
					Set("CurrentVariety1", ref _currentVariety1, _varieties.First(v => v.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety1));
					Set("CurrentVariety2", ref _currentVariety2, _varieties.First(v => v.ModelVariety == _currentVarietyPair.ModelVarietyPair.Variety2));
				}
			}
		}

		private void SetCurrentVarietyPair()
		{
			VarietyPairViewModel vm = null;
			if (_currentVariety1 != null && _currentVariety2 != null && _currentVariety1 != _currentVariety2)
			{
				VarietyPair pair;
				if (_currentVariety1.ModelVariety.VarietyPairs.TryGetValue(_currentVariety2.ModelVariety, out pair))
					vm = new VarietyPairViewModel(_project, pair);
			}
			Set("CurrentVarietyPair", ref _currentVarietyPair, vm);
		}

		public override bool SwitchView(Type viewType, object model)
		{
			if (base.SwitchView(viewType, model))
			{
				var pair = (VarietyPair) model;
				CurrentVarietyPair = new VarietyPairViewModel(_project, pair);
				return true;
			}

			return false;
		}
	}
}
