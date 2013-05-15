using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace SIL.Cog.ViewModels
{
	public class SimilarityMatrixVarietyPairViewModel : ViewModelBase
	{
		private readonly VarietyPair _varietyPair;
		private readonly Variety _otherVariety;
		private readonly ICommand _switchToVarietyPairCommand;
		private readonly SimilarityMetric _similarityMetric;

		public SimilarityMatrixVarietyPairViewModel(Variety otherVariety)
		{
			_otherVariety = otherVariety;
		}

		public SimilarityMatrixVarietyPairViewModel(SimilarityMetric similarityMetric, Variety otherVariety, VarietyPair varietyPair)
		{
			_varietyPair = varietyPair;
			_otherVariety = otherVariety;
			_switchToVarietyPairCommand = new RelayCommand(SwitchToVarietyPair);
			_similarityMetric = similarityMetric;
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

		public double SimilarityScore
		{
			get
			{
				if (_varietyPair != null)
				{
					switch (_similarityMetric)
					{
						case SimilarityMetric.Lexical:
							return _varietyPair.LexicalSimilarityScore * 100;
						case SimilarityMetric.Phonetic:
							return _varietyPair.PhoneticSimilarityScore * 100;
					}
				}
				return -1;
			}
		}

		public VarietyPair ModelVarietyPair
		{
			get { return _varietyPair; }
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
