using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.NgramModeling;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Tests.Components
{
	[TestFixture]
	public class AlineScorerTests
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();
		private FeatureSystem _featSys;
		private SegmentPool _segmentPool;
		private Segmenter _segmenter;
		private Word _word1;
		private Word _word2;

		[SetUp]
		public void SetUp()
		{
			_featSys = new FeatureSystem
			{
				new SymbolicFeature("place",
					new FeatureSymbol("bilabial"),
					new FeatureSymbol("labiodental"),
					new FeatureSymbol("dental"),
					new FeatureSymbol("alveolar"),
					new FeatureSymbol("retroflex"),
					new FeatureSymbol("palato-alveolar"),
					new FeatureSymbol("palatal"),
					new FeatureSymbol("velar"),
					new FeatureSymbol("uvular"),
					new FeatureSymbol("pharyngeal"),
					new FeatureSymbol("glottal")),
				new SymbolicFeature("manner",
					new FeatureSymbol("stop"),
					new FeatureSymbol("affricate"),
					new FeatureSymbol("fricative"),
					new FeatureSymbol("approximant"),
					new FeatureSymbol("trill"),
					new FeatureSymbol("flap"),
					new FeatureSymbol("close-vowel"),
					new FeatureSymbol("mid-vowel"),
					new FeatureSymbol("open-vowel")),
				new SymbolicFeature("voice",
					new FeatureSymbol("voice+"),
					new FeatureSymbol("voice-")),
				new SymbolicFeature("height",
					new FeatureSymbol("close"),
					new FeatureSymbol("near-close"),
					new FeatureSymbol("close-mid"),
					new FeatureSymbol("mid"),
					new FeatureSymbol("open-mid"),
					new FeatureSymbol("near-open"),
					new FeatureSymbol("open")),
				new SymbolicFeature("backness",
					new FeatureSymbol("front"),
					new FeatureSymbol("near-front"),
					new FeatureSymbol("central"),
					new FeatureSymbol("near-back"),
					new FeatureSymbol("back")),
				new SymbolicFeature("round",
					new FeatureSymbol("round+"),
					new FeatureSymbol("round-"))
			};

			_segmentPool = new SegmentPool();
			_segmenter = new Segmenter(_spanFactory)
				{
					Consonants =
					{
						{"c", FeatureStruct.New(_featSys).Symbol("palatal").Symbol("stop").Symbol("voice-").Value},
						{"b", FeatureStruct.New(_featSys).Symbol("bilabial").Symbol("stop").Symbol("voice+").Value},
						{"r", FeatureStruct.New(_featSys).Symbol("alveolar").Symbol("trill").Symbol("voice+").Value}
					},
					Vowels =
					{
						{"a", FeatureStruct.New(_featSys).Symbol("open").Symbol("front").Symbol("round-").Symbol("open-vowel").Symbol("voice+").Value}
					},
					Boundaries = {"-"},
					Modifiers = {"\u0303", "\u0308"},
					Joiners = {"\u0361"}
				};

			var syllabifier = new SimpleSyllabifier(false, false);

			var meaning = new Meaning("test", null);
			var v1 = new Variety("variety1");
			_word1 = new Word("car", meaning);
			_segmenter.Segment(_word1);
			v1.Words.Add(_word1);

			syllabifier.Process(v1);
			
			var v2 = new Variety("variety2");
			_word2 = new Word("bar", meaning);
			_segmenter.Segment(_word2);
			v2.Words.Add(_word2);

			syllabifier.Process(v2);

			var vp = new VarietyPair(v1, v2);
			vp.CognateSoundCorrespondenceFrequencyDistribution = new ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>>();
			vp.CognateSoundCorrespondenceFrequencyDistribution[_word1.Shape.First.ToSoundContext(_segmentPool, Enumerable.Empty<SoundClass>())].Increment(_segmentPool.Get(_word2.Shape.First));
			vp.CognateSoundCorrespondenceProbabilityDistribution = new ConditionalProbabilityDistribution<SoundContext, Ngram<Segment>>(vp.CognateSoundCorrespondenceFrequencyDistribution,
				(sc, fd) => new MaxLikelihoodProbabilityDistribution<Ngram<Segment>>(fd));
			v1.VarietyPairs.VarietyPairAdded(vp);
			v2.VarietyPairs.VarietyPairAdded(vp);
		}

		private AlineScorer CreateScorer(bool soundChangeScoringEnabled, bool syllablePositionCostEnabled)
		{
			return new AlineScorer(_segmentPool, new[] {_featSys.GetFeature<SymbolicFeature>("height"), _featSys.GetFeature<SymbolicFeature>("backness"), _featSys.GetFeature<SymbolicFeature>("round")},
				new[] {_featSys.GetFeature<SymbolicFeature>("place"), _featSys.GetFeature<SymbolicFeature>("manner"), _featSys.GetFeature<SymbolicFeature>("voice")},
				new Dictionary<SymbolicFeature, int>
				{
					{_featSys.GetFeature<SymbolicFeature>("height"), 10},
					{_featSys.GetFeature<SymbolicFeature>("backness"), 10},
					{_featSys.GetFeature<SymbolicFeature>("round"), 2},
					{_featSys.GetFeature<SymbolicFeature>("place"), 40},
					{_featSys.GetFeature<SymbolicFeature>("manner"), 50},
					{_featSys.GetFeature<SymbolicFeature>("voice"), 5}
				}, new Dictionary<FeatureSymbol, int>
				{
					{_featSys.GetSymbol("bilabial"), 100},
					{_featSys.GetSymbol("labiodental"), 90},
					{_featSys.GetSymbol("dental"), 80},
					{_featSys.GetSymbol("alveolar"), 70},
					{_featSys.GetSymbol("retroflex"), 60},
					{_featSys.GetSymbol("palato-alveolar"), 50},
					{_featSys.GetSymbol("palatal"), 40},
					{_featSys.GetSymbol("velar"), 30},
					{_featSys.GetSymbol("uvular"), 20},
					{_featSys.GetSymbol("pharyngeal"), 10},
					{_featSys.GetSymbol("glottal"), 0},

					{_featSys.GetSymbol("stop"), 100},
					{_featSys.GetSymbol("affricate"), 95},
					{_featSys.GetSymbol("fricative"), 90},
					{_featSys.GetSymbol("approximant"), 80},
					{_featSys.GetSymbol("trill"), 60},
					{_featSys.GetSymbol("flap"), 50},
					{_featSys.GetSymbol("close-vowel"), 30},
					{_featSys.GetSymbol("mid-vowel"), 15},
					{_featSys.GetSymbol("open-vowel"), 0},

					{_featSys.GetSymbol("voice+"), 100},
					{_featSys.GetSymbol("voice-"), 0},

					{_featSys.GetSymbol("close"), 100},
					{_featSys.GetSymbol("near-close"), 85},
					{_featSys.GetSymbol("close-mid"), 65},
					{_featSys.GetSymbol("mid"), 50},
					{_featSys.GetSymbol("open-mid"), 35},
					{_featSys.GetSymbol("near-open"), 15},
					{_featSys.GetSymbol("open"), 0},

					{_featSys.GetSymbol("front"), 100},
					{_featSys.GetSymbol("near-front"), 80},
					{_featSys.GetSymbol("central"), 50},
					{_featSys.GetSymbol("near-back"), 20},
					{_featSys.GetSymbol("back"), 0},

					{_featSys.GetSymbol("round+"), 100},
					{_featSys.GetSymbol("round-"), 0}
				}, Enumerable.Empty<SoundClass>(), soundChangeScoringEnabled, syllablePositionCostEnabled);
		}

		[Test]
		public void Delta_SameConsonants_ReturnsZero()
		{
			AlineScorer scorer = CreateScorer(false, false);
			var fs1 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("bilabial").Symbol("stop").Symbol("voice+").Value;
			var fs2 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("bilabial").Symbol("stop").Symbol("voice+").Value;
			Assert.That(scorer.Delta(fs1, fs2), Is.EqualTo(0));
		}

		[Test]
		public void Delta_ConsonantsDifferByOneFeature_ReturnsCorrectDelta()
		{
			AlineScorer scorer = CreateScorer(false, false);
			var fs1 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("bilabial").Symbol("stop").Symbol("voice+").Value;
			var fs2 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("labiodental").Symbol("stop").Symbol("voice+").Value;
			Assert.That(scorer.Delta(fs1, fs2), Is.EqualTo(400));
		}

		[Test]
		public void Delta_ConsonantsDifferByTwoFeatures_ReturnsCorrectDelta()
		{
			AlineScorer scorer = CreateScorer(false, false);
			var fs1 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("bilabial").Symbol("stop").Symbol("voice+").Value;
			var fs2 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("labiodental").Symbol("fricative").Symbol("voice+").Value;
			Assert.That(scorer.Delta(fs1, fs2), Is.EqualTo(900));
		}

		[Test]
		public void Delta_SameConsonantClusters_ReturnsZero()
		{
			AlineScorer scorer = CreateScorer(false, false);
			var fs1 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("bilabial", "alveolar").Symbol("stop", "trill").Symbol("voice+").Value;
			var fs2 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("bilabial", "alveolar").Symbol("stop", "trill").Symbol("voice+").Value;
			Assert.That(scorer.Delta(fs1, fs2), Is.EqualTo(0));
		}

		[Test]
		public void Delta_ConsonantClustersEmptyFeature_ReturnsCorrectDelta()
		{
			AlineScorer scorer = CreateScorer(false, false);
			var fs1 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("bilabial", "alveolar").Symbol("stop", "trill").Value;
			var fs2 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("bilabial", "alveolar").Symbol("stop", "trill").Symbol("voice-").Value;
			Assert.That(scorer.Delta(fs1, fs2), Is.EqualTo(250));
		}

		[Test]
		public void Delta_ConsonantClustersDifferByOneFeature_ReturnsCorrectDelta()
		{
			AlineScorer scorer = CreateScorer(false, false);
			var fs1 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("bilabial", "alveolar").Symbol("stop", "trill").Symbol("voice-").Value;
			var fs2 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("bilabial", "palatal").Symbol("stop", "trill").Symbol("voice-").Value;
			Assert.That(scorer.Delta(fs1, fs2), Is.EqualTo(600));
		}

		[Test]
		public void Delta_SameVowels_ReturnsZero()
		{
			AlineScorer scorer = CreateScorer(false, false);
			var fs1 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.VowelType).Symbol("close").Symbol("front").Symbol("round+").Symbol("voice+").Symbol("velar").Symbol("close-vowel").Value;
			var fs2 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.VowelType).Symbol("close").Symbol("front").Symbol("round+").Symbol("voice+").Symbol("velar").Symbol("close-vowel").Value;
			Assert.That(scorer.Delta(fs1, fs2), Is.EqualTo(0));
		}

		[Test]
		public void Delta_VowelsDifferByOneFeature_ReturnsCorrectDelta()
		{
			AlineScorer scorer = CreateScorer(false, false);
			var fs1 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.VowelType).Symbol("close").Symbol("front").Symbol("round+").Symbol("voice+").Symbol("velar").Symbol("close-vowel").Value;
			var fs2 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.VowelType).Symbol("close").Symbol("front").Symbol("round-").Symbol("voice+").Symbol("velar").Symbol("close-vowel").Value;
			Assert.That(scorer.Delta(fs1, fs2), Is.EqualTo(200));
		}

		[Test]
		public void Delta_VowelAndConsonant_ReturnsCorrectDelta()
		{
			AlineScorer scorer = CreateScorer(false, false);
			var fs1 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.VowelType).Symbol("close").Symbol("front").Symbol("round+").Symbol("voice+").Symbol("velar").Symbol("close-vowel").Value;
			var fs2 = FeatureStruct.New(_featSys).Symbol(CogFeatureSystem.ConsonantType).Symbol("velar").Symbol("stop").Symbol("voice+").Value;
			Assert.That(scorer.Delta(fs1, fs2), Is.EqualTo(3500));
		}

		[Test]
		public void GetSubstitutionScore_SoundChangeScoringEnabled_ReturnsCorrectScore()
		{
			AlineScorer scorer = CreateScorer(true, false);
			Assert.That(scorer.GetSubstitutionScore(_word1, _word1.Shape.First, _word2, _word2.Shape.First), Is.EqualTo(1400));
		}

		[Test]
		public void GetSubstitutionScore_SoundChangeScoringDisabled_ReturnsCorrectScore()
		{
			AlineScorer scorer = CreateScorer(false, false);
			Assert.That(scorer.GetSubstitutionScore(_word1, _word1.Shape.First, _word2, _word2.Shape.First), Is.EqualTo(600));
		}

		[Test]
		public void GetSubstitutionScore_SyllablePositionCostEnabled_ReturnsCorrectScore()
		{
			AlineScorer scorer = CreateScorer(false, true);
			Assert.That(scorer.GetSubstitutionScore(_word1, _word1.Shape.First, _word2, _word2.Shape.Last), Is.EqualTo(-700));
		}

		[Test]
		public void GetSubstitutionScore_SyllablePositionCostDisabled_ReturnsCorrectScore()
		{
			AlineScorer scorer = CreateScorer(false, false);
			Assert.That(scorer.GetSubstitutionScore(_word1, _word1.Shape.First, _word2, _word2.Shape.Last), Is.EqualTo(-200));
		}

		[Test]
		public void GetMaxScore1_SoundChangeScoringEnabled_ReturnsCorrectScore()
		{
			AlineScorer scorer = CreateScorer(true, false);
			Assert.That(scorer.GetMaxScore1(_word1, _word1.Shape.First, _word2), Is.EqualTo(4300));
		}

		[Test]
		public void GetMaxScore1_SoundChangeScoringDisabled_ReturnsCorrectScore()
		{
			AlineScorer scorer = CreateScorer(false, false);
			Assert.That(scorer.GetMaxScore1(_word1, _word1.Shape.First, _word2), Is.EqualTo(3500));
		}
		
		[Test]
		public void GetMaxScore2_SoundChangeScoringEnabled_ReturnsCorrectScore()
		{
			AlineScorer scorer = CreateScorer(true, false);
			Assert.That(scorer.GetMaxScore2(_word1, _word2, _word2.Shape.First), Is.EqualTo(4300));
		}

		[Test]
		public void GetMaxScore2_SoundChangeScoringDisabled_ReturnsCorrectScore()
		{
			AlineScorer scorer = CreateScorer(false, false);
			Assert.That(scorer.GetMaxScore2(_word1, _word2, _word2.Shape.First), Is.EqualTo(3500));
		}
	}
}
