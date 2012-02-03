using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.Cog
{
	public class MonteCarloSignificanceTest : IProcessor<VarietyPair>
	{
		private readonly IList<IProcessor<VarietyPair>> _analyzers; 
		private readonly int _iterationCount;
		private readonly bool _useSoundChanges;

		public MonteCarloSignificanceTest(IList<IProcessor<VarietyPair>> analyzers, int iterationCount, bool useSoundChanges)
		{
			_analyzers = analyzers;
			_iterationCount = iterationCount;
			_useSoundChanges = useSoundChanges;
		}

		public void Process(VarietyPair varietyPair)
		{
			int num = 0;
			for (int n = 0; n < _iterationCount; n++)
			{
				var randVariety2 = new Variety(varietyPair.Variety2.ID + "-rand" + n, varietyPair.Variety2.Words.Select(word => new { word.Gloss, word.Category }).OrderBy(gloss => Guid.NewGuid())
					.Zip(varietyPair.Variety2.Words.Select(word => word.Shape), (gi, shape) => new Word(shape, gi.Gloss, gi.Category)));
				var randVarietyPair = new VarietyPair(varietyPair.Variety1, randVariety2);
				if (_useSoundChanges)
				{
					foreach (SoundChange soundChange in varietyPair.SoundChanges)
					{
						SoundChange newSoundChange = randVarietyPair.GetSoundChange(soundChange.LeftEnvironment, soundChange.Target, soundChange.RightEnvironment);
						foreach (NPhone corr in soundChange.ObservedCorrespondences)
							newSoundChange[corr] = soundChange[corr];
					}
				}
				foreach (IProcessor<VarietyPair> analyzer in _analyzers)
					analyzer.Process(randVarietyPair);
				if (randVarietyPair.LexicalSimilarityScore >= varietyPair.LexicalSimilarityScore)
					num++;
			}

			varietyPair.Significance = (double) num /_iterationCount;
		}
	}
}
