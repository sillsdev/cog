using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace SIL.Cog.ViewModels
{
	public class VarietyPairSimilarityMatrixViewModel : WrapperViewModel
	{
		private VarietyPair _varietyPair;
		private readonly Variety _otherVariety;
		private readonly ICommand _switchToVarietyPairCommand;

		public VarietyPairSimilarityMatrixViewModel(Variety otherVariety)
			: this(otherVariety, null)
		{
		}

		public VarietyPairSimilarityMatrixViewModel(Variety otherVariety, VarietyPair varietyPair)
			: base(varietyPair)
		{
			ModelVarietyPair = varietyPair;
			_otherVariety = otherVariety;
			_switchToVarietyPairCommand = new RelayCommand(SwitchToVarietyPair);
		}

		private void SwitchToVarietyPair()
		{
			if (_varietyPair != null)
				Messenger.Default.Send(new SwitchViewMessage(typeof(VarietyPairsViewModel), _varietyPair));
		}

		public string OtherVarietyName
		{
			get { return _otherVariety.Name; }
		}

		public double LexicalSimilarityScore
		{
			get
			{
				if (_varietyPair == null)
					return -1;
				return _varietyPair.LexicalSimilarityScore * 100;
			}
		}

		public double PhoneticSimilarityScore
		{
			get
			{
				if (_varietyPair == null)
					return -1;
				return _varietyPair.PhoneticSimilarityScore * 100;
			}
		}

		public VarietyPair ModelVarietyPair
		{
			get { return _varietyPair; }
			set
			{
				_varietyPair = value;
				WrappedObject = value;
				RaisePropertyChanged("LexicalSimilarityScore");
				RaisePropertyChanged("PhoneticSimilarityScore");
			}
		}

		public Variety ModelOtherVariety
		{
			get { return _otherVariety; }
		}

		public ICommand SwitchToVarietyPairCommand
		{
			get { return _switchToVarietyPairCommand; }
		}
	}
}
