using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Statistics;
using SIL.Collections;

namespace SIL.Cog.Applications.Services
{
	[ProtoContract]
	internal class VarietyPairSurrogate
	{
		private readonly List<WordPairSurrogate> _wordPairs;
		private readonly Dictionary<SoundContextSurrogate, Tuple<string[], int>[]> _soundChanges;
		private readonly Dictionary<SyllablePosition, List<SoundCorrespondenceSurrogate>> _soundCorrespondenceCollections;

		public VarietyPairSurrogate()
		{
			_wordPairs = new List<WordPairSurrogate>();
			_soundChanges = new Dictionary<SoundContextSurrogate, Tuple<string[], int>[]>();
			_soundCorrespondenceCollections = new Dictionary<SyllablePosition, List<SoundCorrespondenceSurrogate>>();
		}

		public VarietyPairSurrogate(VarietyPair vp)
		{
			Variety1 = vp.Variety1.Name;
			Variety2 = vp.Variety2.Name;
			var wordPairSurrogates = new Dictionary<WordPair, WordPairSurrogate>();
			_wordPairs = vp.WordPairs.Select(wp => wordPairSurrogates.GetValue(wp, () => new WordPairSurrogate(wp))).ToList();
			PhoneticSimilarityScore = vp.PhoneticSimilarityScore;
			LexicalSimilarityScore = vp.LexicalSimilarityScore;
			DefaultCorrespondenceProbability = vp.DefaultCorrespondenceProbability;
			_soundChanges = new Dictionary<SoundContextSurrogate, Tuple<string[], int>[]>();
			foreach (SoundContext lhs in vp.SoundChangeFrequencyDistribution.Conditions)
			{
				FrequencyDistribution<Ngram> freqDist = vp.SoundChangeFrequencyDistribution[lhs];
				_soundChanges[new SoundContextSurrogate(lhs)] = freqDist.ObservedSamples.Select(ngram => Tuple.Create(ngram.Select(seg => seg.StrRep).ToArray(), freqDist[ngram])).ToArray();
			}
			_soundCorrespondenceCollections = new Dictionary<SyllablePosition, List<SoundCorrespondenceSurrogate>>();
			foreach (KeyValuePair<SyllablePosition, SoundCorrespondenceCollection> kvp in vp.SoundCorrespondenceCollections)
				_soundCorrespondenceCollections[kvp.Key] = kvp.Value.Select(corr => new SoundCorrespondenceSurrogate(wordPairSurrogates, corr)).ToList();
		}

		[ProtoMember(1)]
		public string Variety1 { get; set; }
		[ProtoMember(2)]
		public string Variety2 { get; set; }

		[ProtoMember(3, AsReference = true)]
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
		public Dictionary<SoundContextSurrogate, Tuple<string[], int>[]> SoundChangeFrequencyDistribution
		{
			get { return _soundChanges; }
		}

		[ProtoMember(8)]
		public Dictionary<SyllablePosition, List<SoundCorrespondenceSurrogate>> SoundCorrespondenceCollections
		{
			get { return _soundCorrespondenceCollections; }
		}

		public VarietyPair ToVarietyPair(SegmentPool segmentPool, CogProject project)
		{
			var vp = new VarietyPair(project.Varieties[Variety1], project.Varieties[Variety2])
				{
					PhoneticSimilarityScore = PhoneticSimilarityScore,
					LexicalSimilarityScore = LexicalSimilarityScore,
					DefaultCorrespondenceProbability = DefaultCorrespondenceProbability
				};
			var wordPairs = new Dictionary<WordPairSurrogate, WordPair>();
			vp.WordPairs.AddRange(_wordPairs.Select(surrogate => wordPairs.GetValue(surrogate, () => surrogate.ToWordPair(project, vp))));
			var soundChanges = new ConditionalFrequencyDistribution<SoundContext, Ngram>();
			foreach (KeyValuePair<SoundContextSurrogate, Tuple<string[], int>[]> fd in _soundChanges)
			{
				SoundContext ctxt = fd.Key.ToSoundContext(project, segmentPool);
				FrequencyDistribution<Ngram> freqDist = soundChanges[ctxt];
				foreach (Tuple<string[], int> sample in fd.Value)
				{
					Ngram corr = sample.Item1 == null ? new Ngram() : new Ngram(sample.Item1.Select(segmentPool.GetExisting));
					freqDist.Increment(corr, sample.Item2);
				}
			}
			vp.SoundChangeFrequencyDistribution = soundChanges;
			IWordAligner aligner = project.WordAligners["primary"];
			int segmentCount = vp.Variety2.SegmentFrequencyDistributions[SyllablePosition.Anywhere].ObservedSamples.Count;
			int possCorrCount = aligner.ExpansionCompressionEnabled ? (segmentCount * segmentCount) + segmentCount + 1 : segmentCount + 1;
			vp.SoundChangeProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram>(soundChanges,
				freqDist => new WittenBellProbabilityDistribution<Ngram>(freqDist, possCorrCount));

			foreach (KeyValuePair<SyllablePosition, List<SoundCorrespondenceSurrogate>> kvp in _soundCorrespondenceCollections)
			{
				if (kvp.Value != null)
					vp.SoundCorrespondenceCollections[kvp.Key].AddRange(kvp.Value.Select(surrogate => surrogate.ToSoundCorrespondence(segmentPool, wordPairs)));
			}
			return vp;
		}
	}
}
