using System;
using System.Collections.Generic;
using SIL.Collections;

namespace SIL.Cog
{
	public class VarietyPair
	{
		public static VarietyPair Create(Variety variety1, Variety variety2)
		{
			var varietyPair = new VarietyPair(variety1, variety2);
			variety1.VarietyPairs.Add(varietyPair);
			variety2.VarietyPairs.Add(varietyPair);
			return varietyPair;
		}

		private readonly Variety _variety1;
		private readonly Variety _variety2;
		private readonly WordPairCollection _wordPairs; 
		private readonly Dictionary<Tuple<NaturalClass, NSegment, NaturalClass>, SoundChange> _soundChanges;
		private readonly double _defaultCorrespondenceProbability;
		private readonly int _possibleCorrespondenceCount;
		private readonly Dictionary<Segment, HashSet<Segment>> _similarSegments; 

		internal VarietyPair(Variety variety1, Variety variety2)
		{
			_variety1 = variety1;
			_variety2 = variety2;
			_wordPairs = new WordPairCollection(this);
			_soundChanges = new Dictionary<Tuple<NaturalClass, NSegment, NaturalClass>, SoundChange>();

			int phonemeCount = _variety2.Segments.Count;
			_possibleCorrespondenceCount = (phonemeCount * phonemeCount) + phonemeCount + 1;
			_defaultCorrespondenceProbability = 1.0 / _possibleCorrespondenceCount;
			_similarSegments = new Dictionary<Segment, HashSet<Segment>>();
		}

		public Variety Variety1
		{
			get { return _variety1; }
		}

		public Variety Variety2
		{
			get { return _variety2; }
		}

		public WordPairCollection WordPairs
		{
			get { return _wordPairs; }
		}

		public double PhoneticSimilarityScore { get; set; }

		public double LexicalSimilarityScore { get; set; }

		public double Significance { get; set; }

		public double Precision { get; set; }

		public double Recall { get; set; }

		public double DefaultCorrespondenceProbability
		{
			get { return _defaultCorrespondenceProbability; }
		}

		public int PossibleCorrespondenceCount
		{
			get { return _possibleCorrespondenceCount; }
		}

		public IReadOnlyCollection<SoundChange> SoundChanges
		{
			get { return _soundChanges.Values.AsReadOnlyCollection(); }
		}

		public SoundChange GetSoundChange(NaturalClass leftEnv, NSegment target, NaturalClass rightEnv)
		{
			Tuple<NaturalClass, NSegment, NaturalClass> key = Tuple.Create(leftEnv, target, rightEnv);
			return _soundChanges.GetValue(key, () => new SoundChange(_possibleCorrespondenceCount, leftEnv, target, rightEnv));
		}

		public bool TryGetSoundChange(NaturalClass leftEnv, NSegment target, NaturalClass rightEnv, out SoundChange soundChange)
		{
			Tuple<NaturalClass, NSegment, NaturalClass> key = Tuple.Create(leftEnv, target, rightEnv);
			return _soundChanges.TryGetValue(key, out soundChange);
		}

		public void AddSimilarSegment(Segment seg1, Segment seg2)
		{
			HashSet<Segment> segments = _similarSegments.GetValue(seg1, () => new HashSet<Segment>());
			segments.Add(seg2);
		}

		public IReadOnlySet<Segment> GetSimilarSegments(Segment seg)
		{
			HashSet<Segment> segments;
			if (_similarSegments.TryGetValue(seg, out  segments))
				return segments.AsReadOnlySet();

			return new ReadOnlySet<Segment>(new HashSet<Segment>());
		}
	}
}
