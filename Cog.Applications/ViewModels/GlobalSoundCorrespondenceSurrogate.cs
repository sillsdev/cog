using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.ViewModels
{
	[ProtoContract]
	internal class GlobalSoundCorrespondenceSurrogate
	{
		private readonly List<WordPairSurrogate> _wordPairs;

		public GlobalSoundCorrespondenceSurrogate()
		{
			_wordPairs = new List<WordPairSurrogate>();
		}

		public GlobalSoundCorrespondenceSurrogate(Dictionary<WordPair, WordPairSurrogate> wordPairSurrogates, GlobalSoundCorrespondence corr)
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

		public GlobalSoundCorrespondence ToGlobalSoundCorrespondence(SegmentPool segmentPool, Dictionary<WordPairSurrogate, WordPair> wordPairs)
		{
			var corr = new GlobalSoundCorrespondence(segmentPool.GetExisting(Segment1), segmentPool.GetExisting(Segment2)) {Frequency = Frequency};
			corr.WordPairs.AddRange(_wordPairs.Select(wps => wordPairs[wps]));
			return corr;
		}
	}
}
