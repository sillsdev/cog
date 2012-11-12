using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class SoundCorrespondenceViewModel : ViewModelBase
	{
		private readonly SoundChangeLhs _modelLhs;
		private readonly Ngram _correspondence;
		private readonly SoundCorrespondenceLhsViewModel _lhs;
		private readonly double _prob;

		public SoundCorrespondenceViewModel(SoundChangeLhs lhs, Ngram correspondence, double probability)
		{
			_modelLhs = lhs;
			_correspondence = correspondence;
			_lhs = new SoundCorrespondenceLhsViewModel(lhs);
			_prob = probability;
		}

		public SoundCorrespondenceLhsViewModel Lhs
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

		public SoundChangeLhs ModelSoundChangeLhs
		{
			get { return _modelLhs; }
		}

		public Ngram ModelCorrespondence
		{
			get { return _correspondence; }
		}
	}
}
