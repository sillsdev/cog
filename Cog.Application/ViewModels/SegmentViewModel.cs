using GalaSoft.MvvmLight;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public class SegmentViewModel : ViewModelBase
	{
		private readonly Segment _segment;
		private readonly SoundType _type;

		public SegmentViewModel(Segment segment)
		{
			_segment = segment;
			_type = _segment.Type == CogFeatureSystem.ConsonantType ? SoundType.Consonant : SoundType.Vowel;
		}

		public SegmentViewModel(Segment segment, SoundType type)
		{
			_segment = segment;
			_type = type;
		}

		public string StrRep
		{
			get { return _segment == null ? "-" : _segment.StrRep; }
		}

		public SoundType Type
		{
			get { return _type; }
		}

		public string FeatureStructure
		{
			get { return _segment == null ? string.Empty : _segment.FeatureStruct.GetString(); }
		}

		internal Segment DomainSegment
		{
			get { return _segment; }
		}
	}
}
