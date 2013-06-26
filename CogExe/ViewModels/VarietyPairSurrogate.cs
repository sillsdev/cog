using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using SIL.Cog.Statistics;

namespace SIL.Cog.ViewModels
{
	[ProtoContract]
	public class VarietyPairSurrogate
	{
		private readonly List<WordPairSurrogate> _wordPairs;
		private readonly Dictionary<SoundContextSurrogate, Dictionary<List<string>, int>> _soundChanges; 

		public VarietyPairSurrogate()
		{
			_wordPairs = new List<WordPairSurrogate>();
			_soundChanges = new Dictionary<SoundContextSurrogate, Dictionary<List<string>, int>>();
		}

		public VarietyPairSurrogate(VarietyPair vp)
		{
			Variety1 = vp.Variety1.Name;
			Variety2 = vp.Variety2.Name;
			_wordPairs = vp.WordPairs.Select(wp => new WordPairSurrogate(wp)).ToList();
			PhoneticSimilarityScore = vp.PhoneticSimilarityScore;
			LexicalSimilarityScore = vp.LexicalSimilarityScore;
			DefaultCorrespondenceProbability = vp.DefaultCorrespondenceProbability;
			_soundChanges = new Dictionary<SoundContextSurrogate, Dictionary<List<string>, int>>();
			foreach (SoundContext lhs in vp.SoundChangeFrequencyDistribution.Conditions)
			{
				FrequencyDistribution<Ngram> freqDist = vp.SoundChangeFrequencyDistribution[lhs];
				Dictionary<List<string>, int> dict = freqDist.ObservedSamples.ToDictionary(ngram => ngram.Select(seg => seg.StrRep).ToList(), ngram => freqDist[ngram]);
				_soundChanges[new SoundContextSurrogate(lhs)] = dict;
			}
		}

		[ProtoMember(1)]
		public string Variety1 { get; set; }
		[ProtoMember(2)]
		public string Variety2 { get; set; }

		[ProtoMember(3)]
		public List<WordPairSurrogate> WordPairs
		{
			get { return _wordPairs; }
		}

		[ProtoMember(4)]
		public double PhoneticSimilarityScore { get; set; }
		[ProtoMember(5)]
		public double LexicalSimilarityScore { get; set; }
		[ProtoMember(6)]
		public double DefaultCorrespondenceProbability { get; set; }
		[ProtoMember(7)]
		public Dictionary<SoundContextSurrogate, Dictionary<List<string>, int>> SoundChangeFrequencyDistribution
		{
			get { return _soundChanges; }
		}

		public VarietyPair ToVarietyPair(CogProject project)
		{
			var vp = new VarietyPair(project.Varieties[Variety1], project.Varieties[Variety2])
				{
					PhoneticSimilarityScore = PhoneticSimilarityScore,
					LexicalSimilarityScore = LexicalSimilarityScore,
					DefaultCorrespondenceProbability = DefaultCorrespondenceProbability
				};
			vp.WordPairs.AddRange(_wordPairs.Select(surrogate => surrogate.ToWordPair(project, vp)));
			var soundChanges = new ConditionalFrequencyDistribution<SoundContext, Ngram>();
			foreach (KeyValuePair<SoundContextSurrogate, Dictionary<List<string>, int>> fd in _soundChanges)
			{
				foreach (KeyValuePair<List<string>, int> sample in fd.Value)
					soundChanges[fd.Key.ToSoundContext(project, vp.Variety1)].Increment(new Ngram(sample.Key.Select(s => vp.Variety2.Segments[s])), sample.Value);
			}
			vp.SoundChangeFrequencyDistribution = soundChanges;
			IWordAligner aligner = project.WordAligners["primary"];
			int segmentCount = vp.Variety2.Segments.Count;
			int possCorrCount = aligner.ExpansionCompressionEnabled ? (segmentCount * segmentCount) + segmentCount + 1 : segmentCount + 1;
			vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram>(soundChanges,
				freqDist => new WittenBellProbabilityDistribution<Ngram>(freqDist, possCorrCount));
			return vp;
		}
	}
}
