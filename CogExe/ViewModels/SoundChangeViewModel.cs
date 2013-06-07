using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class SoundChangeViewModel : ViewModelBase
	{
		private readonly SoundContext _modelLhs;
		private readonly Ngram _correspondence;
		private readonly SoundChangeLhsViewModel _lhs;
		private readonly double _prob;
		private readonly int _frequency;

		public SoundChangeViewModel(SoundContext lhs, Ngram correspondence, double probability, int frequency)
		{
			_modelLhs = lhs;
			_correspondence = correspondence;
			_lhs = new SoundChangeLhsViewModel(lhs);
			_prob = probability;
			_frequency = frequency;
		}

		public SoundChangeLhsViewModel Lhs
		{
			get { return _lhs; }
		}

		public string Correspondence
		{
			get { return _correspondence.ToString(); }
		}

		public double Probability
		{
			get { return _prob; }
		}

		public int Frequency
		{
			get { return _frequency; }
		}

		public SoundContext ModelSoundChangeLhs
		{
			get { return _modelLhs; }
		}

		public Ngram ModelCorrespondence
		{
			get { return _correspondence; }
		}
	}
}
