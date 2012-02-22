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
			return relevantFeatures.Sum(feat => Diff(p, q, feat) * (int) feat.Weight);
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
		private readonly VarietyPair _varietyPair;
		private readonly Word _word1;
		private readonly Word _word2;
		private readonly int[,] _sim;
		private int _bestScore = -1;

		internal EditDistanceMatrix(EditDistance editDistance, VarietyPair varietyPair, Word word1, Word word2)
		{
			_editDistance = editDistance;
			_varietyPair = varietyPair;
			_word1 = word1;
			_word2 = word2;
			Annotation<ShapeNode> ann1 = _word1.Shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			Annotation<ShapeNode> ann2 = _word2.Shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			_sim = new int[(ann1 != null ? ann1.Span.Length : _word1.Shape.Count) + 1, (ann2 != null ? ann2.Span.Length : _word2.Shape.Count) + 1];
		}

		public int BestScore
		{
			get
			{
				if (_bestScore == -1)
					_bestScore = ComputeSimilarityMatrix();
				return _bestScore;
			}
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
			Annotation<ShapeNode> ann1 = _word1.Shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			Annotation<ShapeNode> ann2 = _word2.Shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			ShapeNode start1 = ann1 != null ? ann1.Span.Start : _word1.Shape.First;
			ShapeNode start2 = ann2 != null ? ann2.Span.Start : _word2.Shape.First;
			ShapeNode end1 = ann1 != null ? ann1.Span.End : _word1.Shape.Last;
			ShapeNode end2 = ann2 != null ? ann2.Span.End : _word2.Shape.Last;

			switch (_editDistance.Settings.Mode)
			{
				case EditDistanceMode.Global:
				case EditDistanceMode.HalfLocal:
					{
						foreach (Alignment alignment in GetAlignments(end1, end2, _sim.GetLength(0) - 1, _sim.GetLength(1) - 1, threshold, all))
							yield return alignment;
					}
					break;

				case EditDistanceMode.SemiGlobal:
					{
						ShapeNode node1 = start1;
						for (int i = 1; i < _sim.GetLength(0); i++)
						{
							foreach (Alignment alignment in GetAlignments(node1, end2, i, _sim.GetLength(1) - 1, threshold, all))
								yield return alignment;
							node1 = node1.Next;
						}

						ShapeNode node2 = start2;
						for (int j = 1; j < _sim.GetLength(1); j++)
						{
							foreach (Alignment alignment in GetAlignments(end1, node2, _sim.GetLength(0) - 1, j, threshold, all))
								yield return alignment;
							node2 = node2.Next;
						}
					}
					break;

				case EditDistanceMode.Local:
					{
						ShapeNode node1 = start1;
						for (int i = 1; i < _sim.GetLength(0); i++)
						{
							ShapeNode node2 = start2;
							for (int j = 1; j < _sim.GetLength(1); j++)
							{
								foreach (Alignment alignment in GetAlignments(node1, node2, i, j, threshold, all))
									yield return alignment;
								node2 = node2.Next;
							}
							node1 = node1.Next;
						}
					}
					break;
			}
		}

		private IEnumerable<Alignment> GetAlignments(ShapeNode node1, ShapeNode node2, int i, int j, int threshold, bool all)
		{
			if (_sim[i, j] < threshold)
				yield break;

			foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1, node2, i, j, 0, threshold, all))
			{
				if (alignment.Item3.Next.CompareTo(alignment.Item1.Last) <= 0)
					alignment.Item1.Annotations.Add(alignment.Item3.Next, alignment.Item1.Last, FeatureStruct.New().Symbol(CogFeatureSystem.StemType).Value);
				AddNodesToEnd(node1.Next, alignment.Item1);
				if (alignment.Item4.Next.CompareTo(alignment.Item2.Last) <= 0)
					alignment.Item2.Annotations.Add(alignment.Item4.Next, alignment.Item2.Last, FeatureStruct.New().Symbol(CogFeatureSystem.StemType).Value);
				AddNodesToEnd(node2.Next, alignment.Item2);
				yield return new Alignment(alignment.Item1, alignment.Item2, CalcNormalizedScore(alignment.Item1, alignment.Item2, alignment.Item5));
			}
		}

		private int ComputeSimilarityMatrix()
		{
			int maxScore = int.MinValue;
			Annotation<ShapeNode> ann1 = _word1.Shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			Annotation<ShapeNode> ann2 = _word2.Shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			ShapeNode start1 = ann1 != null ? ann1.Span.Start : _word1.Shape.First;
			ShapeNode start2 = ann2 != null ? ann2.Span.Start : _word2.Shape.First;

			ShapeNode node1;
			ShapeNode node2;
			if (_editDistance.Settings.Mode == EditDistanceMode.Global)
			{
				node1 = start1;
				for (int i = 1; i < _sim.GetLength(0); i++)
				{
					_sim[i, 0] = _sim[i - 1, 0] + _editDistance.SigmaDeletion(_varietyPair, node1);
					node1 = node1.Next;
				}

				node2 = start2;
				for (int j = 1; j < _sim.GetLength(1); j++)
				{
					_sim[0, j] = _sim[0, j - 1] + _editDistance.SigmaInsertion(_varietyPair, node2);
					node2 = node2.Next;
				}
			}

			node1 = start1;
			for (int i = 1; i < _sim.GetLength(0); i++)
			{
				node2 = start2;
				for (int j = 1; j < _sim.GetLength(1); j++)
				{
					int m1 = _sim[i - 1, j] + _editDistance.SigmaDeletion(_varietyPair, node1);
					int m2 = _sim[i, j - 1] + _editDistance.SigmaInsertion(_varietyPair, node2);
					int m3 = _sim[i - 1, j - 1] + _editDistance.SigmaSubstitution(_varietyPair, node1, node2);
					int m4 = _editDistance.Settings.DisableExpansionCompression || j - 2 < 0 ? int.MinValue : _sim[i - 1, j - 2] + _editDistance.SigmaExpansion(_varietyPair, node1, node2.Prev, node2);
					int m5 = _editDistance.Settings.DisableExpansionCompression || i - 2 < 0 ? int.MinValue : _sim[i - 2, j - 1] + _editDistance.SigmaCompression(_varietyPair, node1.Prev, node1, node2);

					if (_editDistance.Settings.Mode == EditDistanceMode.Local || _editDistance.Settings.Mode == EditDistanceMode.HalfLocal)
						_sim[i, j] = new[] { m1, m2, m3, m4, m5, 0 }.Max();
					else
						_sim[i, j] = new[] {m1, m2, m3, m4, m5}.Max();

					if (_sim[i, j] > maxScore)
					{
						if (_editDistance.Settings.Mode == EditDistanceMode.SemiGlobal)
						{
							if (i == _sim.GetLength(0) - 1 || j == _sim.GetLength(1) - 1)
								maxScore = _sim[i, j];
						}
						else
						{
							maxScore = _sim[i, j];
						}
					}
					node2 = node2.Next;
				}
				node1 = node1.Next;
			}
			return _editDistance.Settings.Mode == EditDistanceMode.Global || _editDistance.Settings.Mode == EditDistanceMode.HalfLocal ? _sim[_sim.GetLength(0) - 1, _sim.GetLength(1) - 1] : maxScore;
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
			if (_editDistance.Settings.Mode != EditDistanceMode.Global && (i == 0 || j == 0))
			{
				yield return CreateAlignment(node1, node2, score);
			}
			else if (i == 0 && j == 0)
			{
				yield return CreateAlignment(node1, node2, score);
			}
			else
			{
				int opScore;
				if (i != 0 && j != 0)
				{
					opScore = _editDistance.SigmaSubstitution(_varietyPair, node1, node2);
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
				}

				if (j != 0)
				{
					opScore = _editDistance.SigmaInsertion(_varietyPair, node2);
					if (i == 0 || _sim[i, j - 1] + opScore + score >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1, node2.Prev, i, j - 1, score + opScore, threshold, all))
						{
							alignment.Item1.Add(FeatureStruct.New().Symbol(CogFeatureSystem.NullType).Feature(CogFeatureSystem.StrRep).EqualTo("-").Value);
							alignment.Item2.Add(node2.Clone());
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if (!_editDistance.Settings.DisableExpansionCompression && i != 0 && j - 2 >= 0)
				{
					opScore = _editDistance.SigmaExpansion(_varietyPair, node1, node2.Prev, node2);
					if (_sim[i - 1, j - 2] + opScore + score >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1.Prev, node2.Prev.Prev, i - 1, j - 2, score + opScore, threshold, all))
						{
							alignment.Item1.Add(node1.Clone());

							ShapeNode clusterNode1 = node2.Prev.Clone();
							alignment.Item2.Add(clusterNode1);
							ShapeNode clusterNode2 = node2.Clone();
							alignment.Item2.Add(clusterNode2);
							alignment.Item2.Annotations.Add(clusterNode1, clusterNode2, FeatureStruct.New().Symbol(CogFeatureSystem.ClusterType).Value);
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if (i != 0)
				{
					opScore = _editDistance.SigmaDeletion(_varietyPair, node1);
					if (j == 0 || _sim[i - 1, j] + opScore + score >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1.Prev, node2, i - 1, j, score + opScore, threshold, all))
						{
							alignment.Item1.Add(node1.Clone());
							alignment.Item2.Add(FeatureStruct.New().Symbol(CogFeatureSystem.NullType).Feature(CogFeatureSystem.StrRep).EqualTo("-").Value);
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if (!_editDistance.Settings.DisableExpansionCompression && i - 2 >= 0 && j != 0)
				{
					opScore = _editDistance.SigmaCompression(_varietyPair, node1.Prev, node1, node2);
					if (_sim[i - 2, j - 1] + opScore + score >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1.Prev.Prev, node2.Prev, i - 2, j - 1, score + opScore, threshold, all))
						{
							ShapeNode clusterNode1 = node1.Prev.Clone();
							alignment.Item1.Add(clusterNode1);
							ShapeNode clusterNode2 = node1.Clone();
							alignment.Item1.Add(clusterNode2);
							alignment.Item1.Annotations.Add(clusterNode1, clusterNode2, FeatureStruct.New().Symbol(CogFeatureSystem.ClusterType).Value);

							alignment.Item2.Add(node2.Clone());
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if ((_editDistance.Settings.Mode == EditDistanceMode.Local || _editDistance.Settings.Mode == EditDistanceMode.HalfLocal) && _sim[i, j] == 0)
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
			var shape = new Shape(_editDistance.SpanFactory, begin => new ShapeNode(_editDistance.SpanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
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
			return Math.Min(1.0, (double) score / Math.Max(CalcMaxScore(shape1), CalcMaxScore(shape2)));
		}

		private int CalcMaxScore(Shape shape)
		{
			Annotation<ShapeNode> stemAnn = shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			return shape.Aggregate(0, (score, node) => score + (stemAnn != null && stemAnn.Span.Contains(node) ? _editDistance.GetMaxScore(_varietyPair, node)
				: (_editDistance.GetMaxScore(_varietyPair, node) / 2)));
		}
	}
}
