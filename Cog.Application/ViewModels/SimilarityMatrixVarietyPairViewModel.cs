using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public class SimilarityMatrixVarietyPairViewModel : ViewModelBase
	{
		private readonly VarietyPair _varietyPair;
		private readonly Variety _thisVariety;
		private readonly Variety _otherVariety;
		private readonly ICommand _switchToVarietyPairCommand;
		private readonly SimilarityMetric _similarityMetric;

		public SimilarityMatrixVarietyPairViewModel(Variety thisVariety, Variety otherVariety)
		{
			_thisVariety = thisVariety;
			_otherVariety = otherVariety;
		}

		public SimilarityMatrixVarietyPairViewModel(SimilarityMetric similarityMetric, Variety thisVariety, VarietyPair varietyPair)
		{
			_varietyPair = varietyPair;
			_thisVariety = thisVariety;
			_otherVariety = _varietyPair.GetOtherVariety(_thisVariety);
			_switchToVarietyPairCommand = new RelayCommand(SwitchToVarietyPair);
			_similarityMetric = similarityMetric;
		}

		private void SwitchToVarietyPair()
		{
			if (_varietyPair != null)
				Messenger.Default.Send(new SwitchViewMessage(typeof(VarietyPairsViewModel), _varietyPair));
		}

		public string ThisVarietyName
		{
			get { return _thisVariety.Name; }
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

		internal VarietyPair DomainVarietyPair
		{
			get { return _varietyPair; }
		}

		public ICommand SwitchToVarietyPairCommand
		{
			get { return _switchToVarietyPairCommand; }
		}
	}
}
