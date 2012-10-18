using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class RelevantFeatureViewModel : CogViewModelBase
	{
		private readonly SymbolicFeature _feature;
		private bool _isSelected;

		public RelevantFeatureViewModel(SymbolicFeature feature, bool isSelected)
			: base(feature.Description)
		{
			_feature = feature;
			_isSelected = isSelected;
		}

		public SymbolicFeature ModelFeature
		{
			get { return _feature; }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				Set(() => IsSelected, ref _isSelected, value);
				IsDirty = true;
			}
		}

		public bool IsDirty { get; private set; }
	}
}
