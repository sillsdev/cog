using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using SIL.Cog.Domain;
using SIL.Extensions;
using SIL.Machine.FeatureModel;
using SIL.Machine.NgramModeling;
using SIL.Machine.Statistics;

namespace SIL.Cog.Application.Services
{
	[ProtoContract]
	internal class VarietyPairSurrogate
	{
		private readonly List<WordPairSurrogate> _wordPairs;
		private readonly Dictionary<SoundContextSurrogate, Tuple<string[], int>[]> _cognateSoundCorrespondenceFrequencyDistribution;
		private readonly Dictionary<string, List<SoundCorrespondenceSurrogate>> _cognateSoundCorrespondenceByPosition;

		public VarietyPairSurrogate()
		{
			_wordPairs = new List<WordPairSurrogate>();
			_cognateSoundCorrespondenceFrequencyDistribution = new Dictionary<SoundContextSurrogate, Tuple<string[], int>[]>();
			_cognateSoundCorrespondenceByPosition = new Dictionary<string, List<SoundCorrespondenceSurrogate>>();
		}

		public VarietyPairSurrogate(VarietyPair vp)
		{
			Variety1 = vp.Variety1.Name;
			Variety2 = vp.Variety2.Name;
			var wordPairSurrogates = new Dictionary<WordPair, WordPairSurrogate>();
			_wordPairs = vp.WordPairs.Select(wp => wordPairSurrogates.GetOrCreate(wp, () => new WordPairSurrogate(wp))).ToList();
			PhoneticSimilarityScore = vp.PhoneticSimilarityScore;
			LexicalSimilarityScore = vp.LexicalSimilarityScore;
			DefaultSoundCorrespondenceProbability = vp.DefaultSoundCorrespondenceProbability;
			_cognateSoundCorrespondenceFrequencyDistribution = new Dictionary<SoundContextSurrogate, Tuple<string[], int>[]>();
			foreach (SoundContext lhs in vp.CognateSoundCorrespondenceFrequencyDistribution.Conditions)
			{
				FrequencyDistribution<Ngram<Segment>> freqDist = vp.CognateSoundCorrespondenceFrequencyDistribution[lhs];
				_cognateSoundCorrespondenceFrequencyDistribution[new SoundContextSurrogate(lhs)] = freqDist.ObservedSamples.Select(ngram => Tuple.Create(ngram.Select(seg => seg.StrRep).ToArray(), freqDist[ngram])).ToArray();
			}
			_cognateSoundCorrespondenceByPosition = new Dictionary<string, List<SoundCorrespondenceSurrogate>>();
			foreach (KeyValuePair<FeatureSymbol, SoundCorrespondenceCollection> kvp in vp.CognateSoundCorrespondencesByPosition)
			{
				string pos;
				if (kvp.Key == CogFeatureSystem.Onset)
					pos = "onset";
				else if (kvp.Key == CogFeatureSystem.Nucleus)
					pos = "nucleus";
				else
					pos = "coda";
				_cognateSoundCorrespondenceByPosition[pos] = kvp.Value.Select(corr => new SoundCorrespondenceSurrogate(wordPairSurrogates, corr)).ToList();
			}
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
		public double DefaultSoundCorrespondenceProbability { get; set; }
		[ProtoMember(7)]
		public Dictionary<SoundContextSurrogate, Tuple<string[], int>[]> CognateSoundCorrespondenceFrequencyDistribution
		{
			get { return _cognateSoundCorrespondenceFrequencyDistribution; }
		}

		[ProtoMember(8)]
		public Dictionary<string, List<SoundCorrespondenceSurrogate>> CognateSoundCorrespondenceByPosition
		{
			get { return _cognateSoundCorrespondenceByPosition; }
		}

		public VarietyPair ToVarietyPair(SegmentPool segmentPool, CogProject project)
		{
			var vp = new VarietyPair(project.Varieties[Variety1], project.Varieties[Variety2])
				{
					PhoneticSimilarityScore = PhoneticSimilarityScore,
					LexicalSimilarityScore = LexicalSimilarityScore,
					DefaultSoundCorrespondenceProbability = DefaultSoundCorrespondenceProbability
				};
			var wordPairs = new Dictionary<WordPairSurrogate, WordPair>();
			foreach (WordPairSurrogate wpSurrogate in _wordPairs)
			{
				WordPair wp = wpSurrogate.ToWordPair(project, vp);
				vp.WordPairs.Add(wp);
				project.CognacyDecisions.UpdateActualCognacy(wp);
				wordPairs[wpSurrogate] = wp;
			}
			var cognateCorrCounts = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
			foreach (KeyValuePair<SoundContextSurrogate, Tuple<string[], int>[]> fd in _cognateSoundCorrespondenceFrequencyDistribution)
			{
				SoundContext ctxt = fd.Key.ToSoundContext(project, segmentPool);
				FrequencyDistribution<Ngram<Segment>> freqDist = cognateCorrCounts[ctxt];
				foreach (Tuple<string[], int> sample in fd.Value)
				{
					Ngram<Segment> corr = sample.Item1 == null ? new Ngram<Segment>() : new Ngram<Segment>(sample.Item1.Select(segmentPool.GetExisting));
					freqDist.Increment(corr, sample.Item2);
				}
			}
			vp.CognateSoundCorrespondenceFrequencyDistribution = cognateCorrCounts;
			IWordAligner aligner = project.WordAligners[ComponentIdentifiers.PrimaryWordAligner];
			int segmentCount = vp.Variety2.SegmentFrequencyDistribution.ObservedSamples.Count;
			int possCorrCount = aligner.ExpansionCompressionEnabled ? (segmentCount * segmentCount) + segmentCount + 1 : segmentCount + 1;
			vp.CognateSoundCorrespondenceProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(cognateCorrCounts,
				(sc, freqDist) => new WittenBellProbabilityDistribution<Ngram<Segment>>(freqDist, possCorrCount));

			foreach (KeyValuePair<string, List<SoundCorrespondenceSurrogate>> kvp in _cognateSoundCorrespondenceByPosition)
			{
				if (kvp.Value != null)
				{
					FeatureSymbol pos = null;
					switch (kvp.Key)
					{
						case "onset":
							pos = CogFeatureSystem.Onset;
							break;
						case "nucleus":
							pos = CogFeatureSystem.Nucleus;
							break;
						case "coda":
							pos = CogFeatureSystem.Coda;
							break;
					}
					vp.CognateSoundCorrespondencesByPosition[pos].AddRange(kvp.Value.Select(surrogate => surrogate.ToSoundCorrespondence(segmentPool, wordPairs)));
				}
			}
			return vp;
		}
	}
}
