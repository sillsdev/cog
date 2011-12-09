using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;
using SIL.Machine.Matching;
using dnAnalytics.Statistics.Distributions;

namespace SIL.Cog
{
	public class SoundChangeInducer
	{
		private readonly AlineConfig _alineConfig;
		private readonly List<FeatureStruct> _naturalClasses; 

		public SoundChangeInducer(AlineConfig alineConfig, IEnumerable<FeatureStruct> naturalClasses)
		{
			_alineConfig = alineConfig;
			_naturalClasses = new List<FeatureStruct>(naturalClasses);
		}

		public void InduceSoundChanges(IEnumerable<Word> words1, IEnumerable<Word> words2)
		{
			var x = new List<Word>(words1);
			var y = new List<Word>(words2);

			List<FeatureStruct> phonemes = x.SelectMany(word => word.Shape).Union(y.SelectMany(word => word.Shape))
				.Select(node => node.Annotation.FeatureStruct).Distinct().Clone().ToList();

			List<Pattern<Shape, ShapeNode>> changes = phonemes.Select(phoneme => Pattern<Shape, ShapeNode>.New().Annotation(phoneme.Clone()).Value)
				.Concat(Pattern<Shape, ShapeNode>.New().Value).ToList();

			var random = new Random();
			var contexts = new Dictionary<Pattern<Shape, ShapeNode>, Multinomial>();
			foreach (FeatureStruct target in phonemes.Concat((FeatureStruct) null))
			{
				foreach (FeatureStruct leftEnv in _naturalClasses)
				{
					foreach (FeatureStruct rightEnv in _naturalClasses)
					{
						var pattern = new Pattern<Shape, ShapeNode>();
						pattern.Children.Add(new Constraint<Shape, ShapeNode>(leftEnv.Clone()));
						if (target != null)
							pattern.Children.Add(new Constraint<Shape, ShapeNode>(target.Clone()));
						pattern.Children.Add(new Constraint<Shape, ShapeNode>(rightEnv.Clone()));
						double[] alpha = changes.Select(rhs => target != null && rhs.Children.Count == 1
							&& target.Equals(((Constraint<Shape, ShapeNode>) rhs.Children.Single()).FeatureStruct) ? 6.0 : 1.1 / changes.Count - 1).ToArray();
						contexts[pattern] = new Multinomial(Dirichlet.Sample(random, alpha));
					}
				}
			}

			bool converged = false;
			while (!converged)
			{
				E(x, y);
			}
		}

		private void E(IEnumerable<Word> x, IEnumerable<Word> y)
		{
			foreach (Tuple<Word, Word> words in x.Zip(y))
			{
				var aline = new Aline(_alineConfig, words.Item1.Shape, words.Item2.Shape);
				Alignment alignment = aline.GetAlignments().First();
				Annotation<ShapeNode> ann1 = alignment.Shape1.Annotations.GetNodes(CogFeatureSystem.StemType).SingleOrDefault();
				Annotation<ShapeNode> ann2 = alignment.Shape2.Annotations.GetNodes(CogFeatureSystem.StemType).SingleOrDefault();
				if (ann1 != null && ann2 != null)
				{
					foreach (Tuple<ShapeNode, ShapeNode> possibleLink in alignment.Shape1.GetNodes(ann1.Span).Zip(alignment.Shape2.GetNodes(ann2.Span)))
					{
						var u = (string) possibleLink.Item1.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
						var v = (string) possibleLink.Item2.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
						if (possibleLink.Item1.Annotation.Type != CogFeatureSystem.NullType &&
						    possibleLink.Item2.Annotation.Type != CogFeatureSystem.NullType)
						{
							Tuple<string, string> key = Tuple.Create(u, v);
							if (!pairLinkCounts.ContainsKey(key))
								pairLinkCounts[key] = 0;
							pairLinkCounts[key]++;
						}

						if (possibleLink.Item1.Annotation.Type != CogFeatureSystem.NullType)
						{
							if (!lang1LinkCounts.ContainsKey(u))
								lang1LinkCounts[u] = 0;
							lang1LinkCounts[u]++;
						}

						if (possibleLink.Item2.Annotation.Type != CogFeatureSystem.NullType)
						{
							if (!lang2LinkCounts.ContainsKey(v))
								lang2LinkCounts[v] = 0;
							lang2LinkCounts[v]++;
						}
					}
				}
			}
		}
	}
}
