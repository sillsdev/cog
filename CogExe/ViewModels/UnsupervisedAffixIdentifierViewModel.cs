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
			set
			{
				Set(() => Threshold, ref _threshold, value);
				IsChanged = true;
			}
		}

		public int MaxAffixLength
		{
			get { return _maxAffixLength; }
			set
			{
				Set(() => MaxAffixLength, ref _maxAffixLength, value);
				IsChanged = true;
			}
		}

		public bool CategoryRequired
		{
			get { return _categoryRequired; }
			set
			{
				Set(() => CategoryRequired, ref _categoryRequired, value);
				IsChanged = true;
			}
		}

		public override object UpdateComponent()
		{
			var affixIdentifier = new UnsupervisedAffixIdentifier(_spanFactory, _threshold, _maxAffixLength, _categoryRequired);
			Project.VarietyProcessors["affixIdentifier"] = affixIdentifier;
			return affixIdentifier;
		}
	}
}
