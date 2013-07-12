using SIL.Cog.Components;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class UnsupervisedAffixIdentifierViewModel : ComponentSettingsViewModelBase 
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private double _threshold;
		private int _maxAffixLength;
		private bool _categoryRequired;

		public UnsupervisedAffixIdentifierViewModel(SpanFactory<ShapeNode> spanFactory, CogProject project, UnsupervisedAffixIdentifier identifier)
			: base("Automatic stemmer", project)
		{
			_spanFactory = spanFactory;
			_threshold = identifier.Threshold;
			_maxAffixLength = identifier.MaxAffixLength;
			_categoryRequired = identifier.CategoryRequired;
		}

		public double Threshold
		{
			get { return _threshold; }
			set { SetChanged(() => Threshold, ref _threshold, value); }
		}

		public int MaxAffixLength
		{
			get { return _maxAffixLength; }
			set { SetChanged(() => MaxAffixLength, ref _maxAffixLength, value); }
		}

		public bool CategoryRequired
		{
			get { return _categoryRequired; }
			set { SetChanged(() => CategoryRequired, ref _categoryRequired, value); }
		}

		public override object UpdateComponent()
		{
			var affixIdentifier = new UnsupervisedAffixIdentifier(_spanFactory, _threshold, _maxAffixLength, _categoryRequired);
			Project.VarietyProcessors["affixIdentifier"] = affixIdentifier;
			return affixIdentifier;
		}
	}
}
