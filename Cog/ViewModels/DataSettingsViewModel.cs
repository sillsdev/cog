using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class DataSettingsViewModel : WorkspaceViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private CogProject _project;
		private UnsupervisedAffixIdentifierViewModel _unsupervisedAffixIdentifier;
		private ReadOnlyCollection<CogViewModelBase> _components;
		private readonly ICommand _applyCommand;
		private readonly ICommand _resetCommand;
		private bool _isDirty;

		public DataSettingsViewModel(SpanFactory<ShapeNode> spanFactory)
			: base("Settings")
		{
			_spanFactory = spanFactory;
			_applyCommand = new RelayCommand(Apply, CanApply);
			_resetCommand = new RelayCommand(Reset);
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			_isDirty = true;
			Reset();
		}

		private bool CanApply()
		{
			return _isDirty;
		}

		private void Apply()
		{
			var identifier = new UnsupervisedAffixIdentifier(_spanFactory, _unsupervisedAffixIdentifier.Threshold,
				_unsupervisedAffixIdentifier.MaxAffixLength, _unsupervisedAffixIdentifier.CategoryRequired);
			_project.VarietyProcessors["affixIdentifier"] = identifier;
			_isDirty = false;
		}

		private void Reset()
		{
			if (!_isDirty)
				return;

			if (_unsupervisedAffixIdentifier != null)
				_unsupervisedAffixIdentifier.PropertyChanged -= Component_PropertyChanged;
			_unsupervisedAffixIdentifier = new UnsupervisedAffixIdentifierViewModel((UnsupervisedAffixIdentifier) _project.VarietyProcessors["affixIdentifier"]);
			_unsupervisedAffixIdentifier.PropertyChanged += Component_PropertyChanged;
			Set("Components", ref _components, new ReadOnlyCollection<CogViewModelBase>(new CogViewModelBase[]
				{
					_unsupervisedAffixIdentifier
				}));
			_isDirty = false;
		}

		private void Component_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			_isDirty = true;
		}

		public ReadOnlyCollection<CogViewModelBase> Components
		{
			get { return _components; }
		}

		public ICommand ApplyCommand
		{
			get { return _applyCommand; }
		}

		public ICommand ResetCommand
		{
			get { return _resetCommand; }
		}
	}
}
