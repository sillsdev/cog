using SIL.Cog.Processors;

namespace SIL.Cog.ViewModels
{
	public class UnsupervisedAffixIdentifierViewModel : CogViewModelBase
	{
		private double _threshold;
		private int _maxAffixLength;
		private bool _categoryRequired;

		public UnsupervisedAffixIdentifierViewModel(UnsupervisedAffixIdentifier identifier)
			: base("Automatic stemmer")
		{
			_threshold = identifier.Threshold;
			_maxAffixLength = identifier.MaxAffixLength;
			_categoryRequired = identifier.CategoryRequired;
		}

		public double Threshold
		{
			get { return _threshold; }
			set { Set("Threshold", ref _threshold, value); }
		}

		public int MaxAffixLength
		{
			get { return _maxAffixLength; }
			set { Set("MaxAffixLength", ref _maxAffixLength, value); }
		}

		public bool CategoryRequired
		{
			get { return _categoryRequired; }
			set { Set("CategoryRequired", ref _categoryRequired, value); }
		}
	}
}
