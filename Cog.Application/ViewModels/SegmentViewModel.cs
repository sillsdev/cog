using GalaSoft.MvvmLight;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public class SegmentViewModel : ViewModelBase
	{
		private readonly Segment _segment;

		public SegmentViewModel(Segment segment)
		{
			_segment = segment;
		}

		public string StrRep
		{
			get { return _segment.StrRep; }
		}

		public SoundType Type
		{
			get
			{
				if (_segment.Type == CogFeatureSystem.ConsonantType)
					return SoundType.Consonant;
				return SoundType.Vowel;
			}
		}

		public string FeatureStructure
		{
			get { return _segment.FeatureStruct.GetString(); }
		}

		internal Segment DomainSegment
		{
			get { return _segment; }
		}
	}
}
