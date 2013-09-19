using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Services
{
	[ProtoContract]
	internal class SoundCorrespondenceSurrogate
	{
		private readonly List<WordPairSurrogate> _wordPairs;

		public SoundCorrespondenceSurrogate()
		{
			_wordPairs = new List<WordPairSurrogate>();
		}

		public SoundCorrespondenceSurrogate(Dictionary<WordPair, WordPairSurrogate> wordPairSurrogates, SoundCorrespondence corr)
		{
			Segment1 = corr.Segment1.StrRep;
			Segment2 = corr.Segment2.StrRep;
			Frequency = corr.Frequency;
			_wordPairs = corr.WordPairs.Select(wp => wordPairSurrogates[wp]).ToList();
		}

		[ProtoMember(1)]
		public string Segment1 { get; set; }
		[ProtoMember(2)]
		public string Segment2 { get; set; }
		[ProtoMember(3)]
		public int Frequency { get; set; }

		[ProtoMember(4, AsReference = true)]
		public List<WordPairSurrogate> WordPairs
		{
			get { return _wordPairs; }
		}

		public SoundCorrespondence ToSoundCorrespondence(SegmentPool segmentPool, Dictionary<WordPairSurrogate, WordPair> wordPairs)
		{
			var corr = new SoundCorrespondence(segmentPool.GetExisting(Segment1), segmentPool.GetExisting(Segment2)) {Frequency = Frequency};
			corr.WordPairs.AddRange(_wordPairs.Select(wps => wordPairs[wps]));
			return corr;
		}
	}
}
