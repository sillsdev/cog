using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Components
{
	public class SegmentFrequencyDistributionCalculator : IProcessor<Variety>
	{
		private readonly SegmentPool _segmentPool;

		public SegmentFrequencyDistributionCalculator(SegmentPool segmentPool)
		{
			_segmentPool = segmentPool;
		}

		private bool IsOnset(Word word, ShapeNode node)
		{
			return node.Type() == CogFeatureSystem.ConsonantType && node.Annotation.Parent.Children.First == node.Annotation;
		}

		private bool IsNucleus(Word word, ShapeNode node)
		{
			return node.Type() == CogFeatureSystem.VowelType;
		}

		private bool IsCoda(Word word, ShapeNode node)
		{
			return node.Type() == CogFeatureSystem.ConsonantType && node.Annotation.Parent.Children.Last == node.Annotation;
		}

		public void Process(Variety data)
		{
			var calculators = new Dictionary<SyllablePosition, Calculator>
				{
					{SyllablePosition.Anywhere, new Calculator((word, node) => true)},
					{SyllablePosition.Onset, new Calculator(IsOnset)},
					{SyllablePosition.Nucleus, new Calculator(IsNucleus)},
					{SyllablePosition.Coda, new Calculator(IsCoda)}
				};

			foreach (Word word in data.Words)
			{
				foreach (ShapeNode node in word.Shape.Where(n => n.Type().IsOneOf(CogFeatureSystem.VowelType, CogFeatureSystem.ConsonantType)))
				{
					foreach (Calculator identifier in calculators.Values)
						identifier.ProcessNode(_segmentPool, word, node);
				}
			}

			foreach (KeyValuePair<SyllablePosition, Calculator> kvp in calculators)
				data.SegmentFrequencyDistributions[kvp.Key] = kvp.Value.FrequencyDistribution;
		}

		private class Calculator
		{
			private readonly FrequencyDistribution<Segment> _freqDist;
			private readonly Func<Word, ShapeNode, bool> _filter;

			public Calculator(Func<Word, ShapeNode, bool> filter)
			{
				_freqDist = new FrequencyDistribution<Segment>();
				_filter = filter;
			}

			public FrequencyDistribution<Segment> FrequencyDistribution
			{
				get { return _freqDist; }
			}

			public void ProcessNode(SegmentPool segmentPool, Word word, ShapeNode node)
			{
				if (_filter(word, node))
				{
					Segment seg = segmentPool.Get(node);
					_freqDist.Increment(seg);
				}
			}
		}
	}
}
