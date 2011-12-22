using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class EditDistanceMatrix
	{
		public static int Delta(IEnumerable<SymbolicFeature> relevantFeatures, ShapeNode p, ShapeNode q)
		{
			return relevantFeatures.Aggregate(0, (val, feat) => val + (Diff(p, q, feat) * (int)feat.Weight));
		}

		private static int Diff(ShapeNode p, ShapeNode q, SymbolicFeature feature)
		{
			SymbolicFeatureValue pValue;
			if (!p.Annotation.FeatureStruct.TryGetValue(feature, out pValue))
				pValue = null;
			SymbolicFeatureValue qValue;
			if (!q.Annotation.FeatureStruct.TryGetValue(feature, out qValue))
				qValue = null;

			if (pValue == null && qValue == null)
				return 0;

			if (pValue == null)
				return (int)qValue.Values.MinBy(symbol => symbol.Weight).Weight;

			if (qValue == null)
				return (int)pValue.Values.MinBy(symbol => symbol.Weight).Weight;

			int min = -1;
			foreach (FeatureSymbol pSymbol in pValue.Values)
			{
				foreach (FeatureSymbol qSymbol in qValue.Values)
				{
					int diff = Math.Abs((int)pSymbol.Weight - (int)qSymbol.Weight);
					if (min == -1 || diff < min)
						min = diff;
				}
			}

			return min;
		}

		private readonly EditDistance _editDistance;
		private readonly WordPair _wordPair;
		private readonly int[,] _sim;
		private int _bestScore = -1;

		internal EditDistanceMatrix(EditDistance editDistance, WordPair wordPair)
		{
			_editDistance = editDistance;
			_wordPair = wordPair;
			_sim = new int[_wordPair.Word1.Shape.Count + 1, _wordPair.Word2.Shape.Count + 1];
		}

		public IEnumerable<Alignment> GetAlignments()
		{
			if (_bestScore == -1)
				_bestScore = ComputeSimilarityMatrix();
			return GetAlignments(_bestScore, false);
		}

		public IEnumerable<Alignment> GetAlignments(double scoreMargin)
		{
			if (_bestScore == -1)
				_bestScore = ComputeSimilarityMatrix();
			return GetAlignments((int) (scoreMargin * _bestScore), true);
		}

		private IEnumerable<Alignment> GetAlignments(int threshold, bool all)
		{
			ShapeNode node1 = _wordPair.Word1.Shape.First;
			for (int i = 1; i < _wordPair.Word1.Shape.Count + 1; i++)
			{
				ShapeNode node2 = _wordPair.Word2.Shape.First;
				for (int j = 1; j < _wordPair.Word2.Shape.Count + 1; j++)
				{
					if (_sim[i, j] >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1, node2, i, j, 0, threshold, all))
						{
							alignment.Item1.Annotations.Add(CogFeatureSystem.StemType, alignment.Item3.Next, alignment.Item1.Last, FeatureStruct.New().Value);
							AddNodesToEnd(node1.Next, alignment.Item1);
							alignment.Item2.Annotations.Add(CogFeatureSystem.StemType, alignment.Item4.Next, alignment.Item2.Last, FeatureStruct.New().Value);
							AddNodesToEnd(node2.Next, alignment.Item2);
							yield return new Alignment(alignment.Item1, alignment.Item2, CalcNormalizedScore(alignment.Item1, alignment.Item2, alignment.Item5));
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
			ShapeNode node1 = _wordPair.Word1.Shape.First;
			for (int i = 1; i < _wordPair.Word1.Shape.Count + 1; i++)
			{
				ShapeNode node2 = _wordPair.Word2.Shape.First;
				for (int j = 1; j < _wordPair.Word2.Shape.Count + 1; j++)
				{
					_sim[i, j] = new[] {
						_sim[i - 1, j] + _editDistance.SigmaDeletion(_wordPair, node1),
						_sim[i, j - 1] + _editDistance.SigmaInsertion(_wordPair, node2),
						_sim[i - 1, j - 1] + _editDistance.SigmaSubstitution(_wordPair, node1, node2),
						j - 2 < 0 ? int.MinValue : _sim[i - 1, j - 2] + _editDistance.SigmaExpansion(_wordPair, node1, node2.Prev, node2),
						i - 2 < 0 ? int.MinValue : _sim[i - 2, j - 1] + _editDistance.SigmaCompression(_wordPair, node1.Prev, node1, node2),
						0 }.Max();
					if (_sim[i, j] > maxScore)
						maxScore = _sim[i, j];
					node2 = node2.Next;
				}
				node1 = node1.Next;
			}
			return maxScore;
		}

		private void AddNodesToEnd(ShapeNode startNode, Shape shape)
		{
			if (startNode != startNode.List.End)
			{
				foreach (ShapeNode node in startNode.GetNodes())
					shape.Add(node.Clone());
			}
		}

		private IEnumerable<Tuple<Shape, Shape, ShapeNode, ShapeNode, int>> Retrieve(ShapeNode node1, ShapeNode node2, int i, int j, int score, int threshold, bool all)
		{
			if (i == 0 || j == 0)
			{
				yield return CreateAlignment(node1, node2, score);
			}
			else
			{
				int opScore = _editDistance.SigmaSubstitution(_wordPair, node1, node2);
				if (_sim[i - 1, j - 1] + opScore + score >= threshold)
				{
					foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1.Prev, node2.Prev, i - 1, j - 1, score + opScore, threshold, all))
					{
						alignment.Item1.Add(node1.Clone());
						alignment.Item2.Add(node2.Clone());
						yield return alignment;
						if (!all)
							yield break;
					}
				}

				opScore = _editDistance.SigmaInsertion(_wordPair, node2);
				if (_sim[i, j - 1] + opScore + score >= threshold)
				{
					foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1, node2.Prev, i, j - 1, score + opScore, threshold, all))
					{
						alignment.Item1.Add(CogFeatureSystem.NullType, FeatureStruct.New().Feature(CogFeatureSystem.StrRep).EqualTo("-").Value);
						alignment.Item2.Add(node2.Clone());
						yield return alignment;
						if (!all)
							yield break;
					}
				}

				if (j - 2 >= 0)
				{
					opScore = _editDistance.SigmaExpansion(_wordPair, node1, node2.Prev, node2);
					if (_sim[i - 1, j - 2] + opScore + score >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1.Prev, node2.Prev.Prev, i - 1, j - 2, score + opScore, threshold, all))
						{
							alignment.Item1.Add(node1.Clone());
							FeatureStruct fs = node2.Prev.Annotation.FeatureStruct.Clone();
							fs.Union(node2.Annotation.FeatureStruct);
							fs.AddValue(CogFeatureSystem.StrRep, (string) node2.Prev.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)
								+ (string) node2.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep));
							alignment.Item2.Add(node2.Prev.Annotation.Type, fs);
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				opScore = _editDistance.SigmaDeletion(_wordPair, node1);
				if (_sim[i - 1, j] + opScore + score >= threshold)
				{
					foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1.Prev, node2, i - 1, j, score + opScore, threshold, all))
					{
						alignment.Item1.Add(node1.Clone());
						alignment.Item2.Add(CogFeatureSystem.NullType, FeatureStruct.New().Feature(CogFeatureSystem.StrRep).EqualTo("-").Value);
						yield return alignment;
						if (!all)
							yield break;
					}
				}

				if (i - 2 >= 0)
				{
					opScore = _editDistance.SigmaCompression(_wordPair, node1.Prev, node1, node2);
					if (_sim[i - 2, j - 1] + opScore + score >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1.Prev.Prev, node2.Prev, i - 2, j - 1, score + opScore, threshold, all))
						{
							FeatureStruct fs = node1.Prev.Annotation.FeatureStruct.Clone();
							fs.Union(node1.Annotation.FeatureStruct);
							fs.AddValue(CogFeatureSystem.StrRep, (string) node1.Prev.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)
								+ (string) node1.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep));
							alignment.Item1.Add(node1.Prev.Annotation.Type, fs);
							alignment.Item2.Add(node2.Clone());
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if (_sim[i, j] == 0)
					yield return CreateAlignment(node1, node2, score);
			}
		}

		private Tuple<Shape, Shape, ShapeNode, ShapeNode, int> CreateAlignment(ShapeNode node1, ShapeNode node2, int score)
		{
			Shape shape1 = CreateEmptyShape();
			AddNodesToBeginning(node1, shape1);

			Shape shape2 = CreateEmptyShape();
			AddNodesToBeginning(node2, shape2);
			return Tuple.Create(shape1, shape2, shape1.Count == 0 ? shape1.Begin : shape1.Last, shape2.Count == 0 ? shape2.Begin : shape2.Last, score);
		}

		private Shape CreateEmptyShape()
		{
			var shape = new Shape(_editDistance.SpanFactory, new ShapeNode(_editDistance.SpanFactory, CogFeatureSystem.AnchorType, new FeatureStruct()),
				new ShapeNode(_editDistance.SpanFactory, CogFeatureSystem.AnchorType, new FeatureStruct()));
			return shape;
		}

		private void AddNodesToBeginning(ShapeNode startNode, Shape shape)
		{
			if (startNode != startNode.List.Begin)
			{
				foreach (ShapeNode node in startNode.GetNodes(Direction.RightToLeft))
					shape.AddAfter(shape.Begin, node.Clone());
			}
		}

		private double CalcNormalizedScore(Shape shape1, Shape shape2, int score)
		{
			return (score * 2.0) / (CalcMaxScore(shape1) + CalcMaxScore(shape2));
		}

		private int CalcMaxScore(Shape shape)
		{
			Annotation<ShapeNode> ann = shape.Annotations.GetNodes(CogFeatureSystem.StemType).SingleOrDefault();
			return shape.Aggregate(0, (score, node) => score + (ann != null && ann.Span.Contains(node) ? _editDistance.SigmaSubstitution(_wordPair, node, node)
				: (_editDistance.SigmaSubstitution(_wordPair, node, node) / 2)));
		}
	}
}
