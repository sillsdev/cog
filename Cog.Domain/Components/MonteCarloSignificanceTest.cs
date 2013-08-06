using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.Cog.Domain.Components
{
	public class MonteCarloSignificanceTest : IProcessor<VarietyPair>
	{
		private readonly CogProject _project;
		private readonly string[] _processorIDs; 
		private readonly int _iterationCount;

		public MonteCarloSignificanceTest(CogProject project, IEnumerable<string> processorIDs, int iterationCount)
		{
			_project = project;
			_processorIDs = processorIDs.ToArray();
			_iterationCount = iterationCount;
		}

		public void Process(VarietyPair varietyPair)
		{
			int num = 0;

			IProcessor<VarietyPair>[] processors = _processorIDs.Select(id => _project.VarietyPairProcessors[id]).ToArray();

			for (int n = 0; n < _iterationCount; n++)
			{
				var randVariety2 = new Variety(varietyPair.Variety2.Name + "-rand" + n);
				randVariety2.Words.AddRange(varietyPair.Variety2.Words.Senses.OrderBy(gloss => Guid.NewGuid())
					.Zip(varietyPair.Variety2.Words.Senses.Select(sense => varietyPair.Variety2.Words[sense].FirstOrDefault()).Where(word => word != null),
						(sense, word) => new Word(word.StrRep, word.StemIndex, word.StemLength, sense) {Shape = word.Shape}));
				var randVarietyPair = new VarietyPair(varietyPair.Variety1, randVariety2);
				foreach (IProcessor<VarietyPair> processor in processors)
					processor.Process(randVarietyPair);
				if (randVarietyPair.LexicalSimilarityScore >= varietyPair.LexicalSimilarityScore)
					num++;
			}

			varietyPair.Significance = (double) num /_iterationCount;
		}
	}
}
