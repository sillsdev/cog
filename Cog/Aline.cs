using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class Aline
	{
		private const int Precision = 100;
		private const int MaxSubstitutionScore = 35 * Precision;
		private const int MaxExpansionScore = 0 * Precision;
		private const int IndelCost = 10 * Precision;
		private const int VowelCost = 0 * Precision;
 
		private readonly AlineConfig _config;
		private readonly Shape _shape1;
		private readonly Shape _shape2;
		private readonly int[,] _sim;
		private readonly int _bestScore;

		public Aline(AlineConfig config, Shape shape1, Shape shape2)
		{
			_config = config;
			_shape1 = shape1;
			_shape2 = shape2;
			_sim = new int[_shape1.Count + 1, _shape2.Count + 1];
			_bestScore = ComputeSimilarityMatrix();
		}

		public IEnumerable<Alignment> GetAlignments()
		{
			return GetAlignments(_bestScore, false);
		}

		public IEnumerable<Alignment> GetAlignments(double scoreMargin)
		{
			return GetAlignments((int) (scoreMargin * _bestScore), true);
		}

		private IEnumerable<Alignment> GetAlignments(int threshold, bool all)
		{
			ShapeNode node1 = _shape1.First;
			for (int i = 1; i < _shape1.Count + 1; i++)
			{
				ShapeNode node2 = _shape2.First;
				for (int j = 1; j < _shape2.Count + 1; j++)
				{
					if (_sim[i, j] >= threshold)
					{
						IEnumerable<Tuple<Shape, Shape, int>> alignments;
						if (Retrieve(node1, node2, i, j, 0, CreateEmptyShape(), CreateEmptyShape(), threshold, all, out alignments))
						{
							foreach (Tuple<Shape, Shape, int> alignment in alignments)
							{
								AddNodesToEnd(node1.Next, alignment.Item1);
								AddNodesToEnd(node2.Next, alignment.Item2);
								yield return new Alignment(alignment.Item1, alignment.Item2, CalcNormalizedScore(alignment.Item1, alignment.Item2, alignment.Item3));
							}
						}
					}
					node2 = node2.Next;
				}
				node1 = node1.Next;
			}
		}

		private int ComputeSimilarityMatrix()
		{
			int maxScore = int.MinValue;
			ShapeNode node1 = _shape1.First;
			for (int i = 1; i < _shape1.Count + 1; i++)
			{
				ShapeNode node2 = _shape2.First;
				for (int j = 1; j < _shape2.Count + 1; j++)
				{
					_sim[i, j] = new[] {
						_sim[i - 1, j] + SigmaDel(node1),
						_sim[i, j - 1] + SigmaIn(node2),
						_sim[i - 1, j - 1] + SigmaSub(node1, node2),
						j - 2 < 0 ? int.MinValue : _sim[i - 1, j - 2] + SigmaExp(node1, node2.Prev, node2),
						i - 2 < 0 ? int.MinValue : _sim[i - 2, j - 1] + SigmaExp(node2, node1.Prev, node1),
						0 }.Max();
					if (_sim[i, j] > maxScore)
						maxScore = _sim[i, j];
					node2 = node2.Next;
				}
				node1 = node1.Next;
			}
			return maxScore;
		}

		private Shape CreateEmptyShape()
		{
			var shape = new Shape(_config.SpanFactory, new ShapeNode(_config.SpanFactory, CogFeatureSystem.AnchorType, new FeatureStruct()),
				new ShapeNode(_config.SpanFactory, CogFeatureSystem.AnchorType, new FeatureStruct()));
			return shape;
		}

		private void AddNodesToEnd(ShapeNode startNode, Shape shape)
		{
			if (startNode != startNode.List.End)
			{
				foreach (ShapeNode node in startNode.GetNodes())
					shape.Add(node.Clone());
			}
		}

		private bool Retrieve(ShapeNode node1, ShapeNode node2, int i, int j, int score, Shape shape1, Shape shape2, int threshold, bool all,
			out IEnumerable<Tuple<Shape, Shape, int>> alignments)
		{
			alignments = Enumerable.Empty<Tuple<Shape, Shape, int>>();
			if (i == 0 || j == 0)
			{
				alignments = alignments.Concat(CreateAlignment(node1, node2, shape1, shape2, score));
				return true;
			}

			bool result = false;
			int opScore = SigmaSub(node1, node2);
			if (_sim[i - 1, j - 1] + opScore + score >= threshold)
			{
				Shape newShape1 = shape1.Clone();
				newShape1.Insert(newShape1.Begin, node1.Clone());
				Shape newShape2 = shape2.Clone();
				newShape2.Insert(newShape2.Begin, node2.Clone());
				IEnumerable<Tuple<Shape, Shape, int>> curAlignments;
				if (Retrieve(node1.Prev, node2.Prev, i - 1, j - 1, score + opScore, newShape1, newShape2, threshold, all, out curAlignments))
				{
					alignments = alignments.Concat(curAlignments);
					if (!all)
						return true;
					result = true;
				}
			}

			opScore = SigmaIn(node2);
			if (_sim[i, j - 1] + opScore + score >= threshold)
			{
				Shape newShape1 = shape1.Clone();
				newShape1.Insert(newShape1.Begin, CogFeatureSystem.NullType, FeatureStruct.New().Feature(CogFeatureSystem.StrRep).EqualTo("-").Value);
				Shape newShape2 = shape2.Clone();
				newShape2.Insert(newShape2.Begin, node2.Clone());
				IEnumerable<Tuple<Shape, Shape, int>> curAlignments;
				if (Retrieve(node1, node2.Prev, i, j - 1, score + opScore, newShape1, newShape2, threshold, all, out curAlignments))
				{
					alignments = alignments.Concat(curAlignments);
					if (!all)
						return true;
					result = true;
				}
			}

			if (j - 2 >= 0)
			{
				opScore = SigmaExp(node1, node2.Prev, node2);
				if (_sim[i - 1, j - 2] + opScore + score >= threshold)
				{
					Shape newShape1 = shape1.Clone();
					newShape1.Insert(newShape1.Begin, node1.Clone());
					Shape newShape2 = shape2.Clone();
					FeatureStruct fs = node2.Prev.Annotation.FeatureStruct.Clone();
					fs.Merge(node2.Annotation.FeatureStruct);
					fs.AddValue(CogFeatureSystem.StrRep, (string) node2.Prev.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)
						+ (string) node2.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep));
					newShape2.Insert(newShape2.Begin, node2.Prev.Annotation.Type, fs);
					IEnumerable<Tuple<Shape, Shape, int>> curAlignments;
					if (Retrieve(node1.Prev, node2.Prev.Prev, i - 1, j - 2, score + opScore, newShape1, newShape2, threshold, all, out curAlignments))
					{
						alignments = alignments.Concat(curAlignments);
						if (!all)
							return true;
						result = true;
					}
				}
			}

			opScore = SigmaDel(node1);
			if (_sim[i - 1, j] + opScore + score >= threshold)
			{
				Shape newShape1 = shape1.Clone();
				newShape1.Insert(newShape1.Begin, node1.Clone());
				Shape newShape2 = shape2.Clone();
				newShape2.Insert(newShape2.Begin, CogFeatureSystem.NullType, FeatureStruct.New().Feature(CogFeatureSystem.StrRep).EqualTo("-").Value);
				IEnumerable<Tuple<Shape, Shape, int>> curAlignments;
				if (Retrieve(node1.Prev, node2, i - 1, j, score + opScore, newShape1, newShape2, threshold, all, out curAlignments))
				{
					alignments = alignments.Concat(curAlignments);
					if (!all)
						return true;
					result = true;
				}
			}

			if (i - 2 >= 0)
			{
				opScore = SigmaExp(node2, node1.Prev, node1);
				if (_sim[i - 2, j - 1] + opScore + score >= threshold)
				{
					Shape newShape1 = shape1.Clone();
					FeatureStruct fs = node1.Prev.Annotation.FeatureStruct.Clone();
					fs.Merge(node1.Annotation.FeatureStruct);
					fs.AddValue(CogFeatureSystem.StrRep, (string) node1.Prev.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)
						+ (string) node1.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep));
					newShape1.Insert(newShape1.Begin, node1.Prev.Annotation.Type, fs);
					Shape newShape2 = shape2.Clone();
					newShape2.Insert(newShape2.Begin, node2.Clone());
					IEnumerable<Tuple<Shape, Shape, int>> curAlignments;
					if (Retrieve(node1.Prev.Prev, node2.Prev, i - 2, j - 1, score + opScore, newShape1, newShape2, threshold, all, out curAlignments))
					{
						alignments = alignments.Concat(curAlignments);
						if (!all)
							return true;
						result = true;
					}
				}
			}

			if (_sim[i, j] == 0)
			{
				alignments = alignments.Concat(CreateAlignment(node1, node2, shape1, shape2, score));
				result = true;
			}

			return result;
		}

		private Tuple<Shape, Shape, int> CreateAlignment(ShapeNode node1, ShapeNode node2, Shape shape1, Shape shape2, int score)
		{
			if (shape1.Count > 0)
				shape1.Annotations.Add(CogFeatureSystem.StemType, shape1.First, shape1.Last, new FeatureStruct());
			AddNodesToBeginning(node1, shape1);
			if (shape2.Count > 0)
				shape2.Annotations.Add(CogFeatureSystem.StemType, shape2.First, shape2.Last, new FeatureStruct());
			AddNodesToBeginning(node2, shape2);
			return Tuple.Create(shape1, shape2, score);
		}

		private void AddNodesToBeginning(ShapeNode startNode, Shape shape)
		{
			if (startNode != startNode.List.Begin)
			{
				foreach (ShapeNode node in startNode.GetNodes(Direction.RightToLeft))
					shape.Insert(shape.Begin, node.Clone());
			}
		}

		private int SigmaIn(ShapeNode q)
		{
			var qStrRep = (string)q.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
			SoundChange correspondence;
			if (_config.TryGetSegmentCorrespondence("-", qStrRep, out correspondence))
				return (int)(-IndelCost * (1.0 - correspondence.CorrespondenceProbability));
			return -IndelCost;
		}

		private int SigmaDel(ShapeNode p)
		{
			var pStrRep = (string)p.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
			SoundChange correspondence;
			if (_config.TryGetSegmentCorrespondence(pStrRep, "-", out correspondence))
				return (int)(-IndelCost * (1.0 - correspondence.CorrespondenceProbability));
			return -IndelCost;
		}

		private int SigmaSub(ShapeNode p, ShapeNode q)
		{
			int score = MaxSubstitutionScore - Delta(p, q) - V(p) - V(q);
			var pStrRep = (string)p.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
			var qStrRep = (string)q.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
			SoundChange correspondence;
			if (_config.TryGetSegmentCorrespondence(pStrRep, qStrRep, out correspondence))
				return (int)(score * correspondence.CorrespondenceProbability);
			return score;
		}

		private int SigmaExp(ShapeNode p, ShapeNode q1, ShapeNode q2)
		{
			return MaxExpansionScore - Delta(p, q1) - Delta(p, q2) - V(p) - Math.Max(V(q1), V(q2));
		}

		private int V(ShapeNode node)
		{
			return node.Annotation.Type == CogFeatureSystem.VowelType ? VowelCost : 0;
		}

		private int Delta(ShapeNode p, ShapeNode q)
		{
			IEnumerable<SymbolicFeature> features = p.Annotation.Type == CogFeatureSystem.VowelType && q.Annotation.Type == CogFeatureSystem.VowelType
				? _config.RelevantVowelFeatures : _config.RelevantConsonantFeatures;

			return features.Aggregate(0, (val, feat) => val + (Diff(p, q, feat) * (int) feat.Weight));
		}

		private int Diff(ShapeNode p, ShapeNode q, SymbolicFeature feature)
		{
			SymbolicFeatureValue pValue;
			IEnumerable<int> pWeights = p.Annotation.FeatureStruct.TryGetValue(feature, out pValue)
				? pValue.Values.Select(symbol => (int) (symbol.Weight * Precision)) : new[] {0};
			SymbolicFeatureValue qValue;
			IEnumerable<int> qWeights = q.Annotation.FeatureStruct.TryGetValue(feature, out qValue)
				? qValue.Values.Select(symbol => (int) (symbol.Weight * Precision)) : new[] {0};

			return (from pWeight in pWeights
			        from qWeight in qWeights
			        select Math.Abs(pWeight - qWeight)).Min();
		}

		private double CalcNormalizedScore(Shape shape1, Shape shape2, int score)
		{
			return (double) score / Math.Max(CalcMaxScore(shape1), CalcMaxScore(shape2));
		}

		private int CalcMaxScore(Shape shape)
		{
			Annotation<ShapeNode> ann = shape.Annotations.GetNodes(CogFeatureSystem.StemType).SingleOrDefault();
			return shape.Aggregate(0, (score, node) => score + (ann != null && ann.Span.Contains(node) ? SigmaSub(node, node) : (SigmaSub(node, node) / 2)));
		}
	}
}
