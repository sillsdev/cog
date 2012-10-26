using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Aligners
{
	public class AlignerResult : IAlignerResult
	{
		private readonly AlignerBase _aligner;
		private readonly VarietyPair _varietyPair;
		private readonly Word _word1;
		private readonly Word _word2;
		private readonly int[,] _sim;
		private int _bestScore = -1;

		internal AlignerResult(AlignerBase aligner, VarietyPair varietyPair, Word word1, Word word2)
		{
			_aligner = aligner;
			_varietyPair = varietyPair;
			_word1 = word1;
			_word2 = word2;

			_sim = new int[_word1.Shape.GetNodes(_word1.Stem.Span).Where(Filter).Count() + 1, _word2.Shape.GetNodes(_word2.Stem.Span).Where(Filter).Count() + 1];
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

		internal static bool Filter(ShapeNode node)
		{
			return node.Annotation.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType, CogFeatureSystem.AnchorType);
		}

		private ShapeNode GetStartNode(ShapeNode node)
		{
			while (node != null && node != node.List.End && !Filter(node))
				node = node.Next;
			return node;
		}

		private ShapeNode GetEndNode(ShapeNode node)
		{
			while (node != null && node != node.List.Begin && !Filter(node))
				node = node.Prev;
			return node;
		}

		private IEnumerable<Alignment> GetAlignments(int threshold, bool all)
		{
			Annotation<ShapeNode> stemAnn1 = _word1.Stem;
			Annotation<ShapeNode> stemAnn2 = _word2.Stem;
			ShapeNode start1 = GetStartNode(stemAnn1.Span.Start);
			ShapeNode start2 = GetStartNode(stemAnn2.Span.Start);
			ShapeNode end1 = GetEndNode(stemAnn1.Span.End);
			ShapeNode end2 = GetEndNode(stemAnn2.Span.End);

			switch (_aligner.Settings.Mode)
			{
				case AlignerMode.Global:
				case AlignerMode.HalfLocal:
					{
						foreach (Alignment alignment in GetAlignments(end1, end2, _sim.GetLength(0) - 1, _sim.GetLength(1) - 1, threshold, all))
							yield return alignment;
					}
					break;

				case AlignerMode.SemiGlobal:
					{
						ShapeNode node1 = start1;
						for (int i = 1; i < _sim.GetLength(0); i++)
						{
							foreach (Alignment alignment in GetAlignments(node1, end2, i, _sim.GetLength(1) - 1, threshold, all))
								yield return alignment;
							node1 = node1.GetNext(Filter);
						}

						ShapeNode node2 = start2;
						for (int j = 1; j < _sim.GetLength(1); j++)
						{
							foreach (Alignment alignment in GetAlignments(end1, node2, _sim.GetLength(0) - 1, j, threshold, all))
								yield return alignment;
							node2 = node2.GetNext(Filter);
						}
					}
					break;

				case AlignerMode.Local:
					{
						ShapeNode node1 = start1;
						for (int i = 1; i < _sim.GetLength(0); i++)
						{
							ShapeNode node2 = start2;
							for (int j = 1; j < _sim.GetLength(1); j++)
							{
								foreach (Alignment alignment in GetAlignments(node1, node2, i, j, threshold, all))
									yield return alignment;
								node2 = node2.GetNext(Filter);
							}
							node1 = node1.GetNext(Filter);
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
				AddNodesToEnd(node1.GetNext(Filter), alignment.Item1);
				if (alignment.Item4.Next.CompareTo(alignment.Item2.Last) <= 0)
					alignment.Item2.Annotations.Add(alignment.Item4.Next, alignment.Item2.Last, FeatureStruct.New().Symbol(CogFeatureSystem.StemType).Value);
				AddNodesToEnd(node2.GetNext(Filter), alignment.Item2);
				yield return new Alignment(alignment.Item1, alignment.Item2, CalcNormalizedScore(alignment.Item1, alignment.Item2, alignment.Item5));
			}
		}

		private int ComputeSimilarityMatrix()
		{
			int maxScore = int.MinValue;
			ShapeNode start1 = GetStartNode(_word1.Stem.Span.Start);
			ShapeNode start2 = GetStartNode(_word2.Stem.Span.Start);

			ShapeNode node1;
			ShapeNode node2;
			if (_aligner.Settings.Mode == AlignerMode.Global)
			{
				node1 = start1;
				for (int i = 1; i < _sim.GetLength(0); i++)
				{
					_sim[i, 0] = _sim[i - 1, 0] + _aligner.SigmaDeletion(_varietyPair, node1);
					node1 = node1.GetNext(Filter);
				}

				node2 = start2;
				for (int j = 1; j < _sim.GetLength(1); j++)
				{
					_sim[0, j] = _sim[0, j - 1] + _aligner.SigmaInsertion(_varietyPair, node2);
					node2 = node2.GetNext(Filter);
				}
			}

			node1 = start1;
			for (int i = 1; i < _sim.GetLength(0); i++)
			{
				node2 = start2;
				for (int j = 1; j < _sim.GetLength(1); j++)
				{
					int m1 = _sim[i - 1, j] + _aligner.SigmaDeletion(_varietyPair, node1);
					int m2 = _sim[i, j - 1] + _aligner.SigmaInsertion(_varietyPair, node2);
					int m3 = _sim[i - 1, j - 1] + _aligner.SigmaSubstitution(_varietyPair, node1, node2);
					int m4 = _aligner.Settings.DisableExpansionCompression || j - 2 < 0 ? int.MinValue : _sim[i - 1, j - 2] + _aligner.SigmaExpansion(_varietyPair, node1, node2.GetPrev(Filter), node2);
					int m5 = _aligner.Settings.DisableExpansionCompression || i - 2 < 0 ? int.MinValue : _sim[i - 2, j - 1] + _aligner.SigmaCompression(_varietyPair, node1.GetPrev(Filter), node1, node2);

					if (_aligner.Settings.Mode == AlignerMode.Local || _aligner.Settings.Mode == AlignerMode.HalfLocal)
						_sim[i, j] = new[] { m1, m2, m3, m4, m5, 0 }.Max();
					else
						_sim[i, j] = new[] {m1, m2, m3, m4, m5}.Max();

					if (_sim[i, j] > maxScore)
					{
						if (_aligner.Settings.Mode == AlignerMode.SemiGlobal)
						{
							if (i == _sim.GetLength(0) - 1 || j == _sim.GetLength(1) - 1)
								maxScore = _sim[i, j];
						}
						else
						{
							maxScore = _sim[i, j];
						}
					}
					node2 = node2.GetNext(Filter);
				}
				node1 = node1.GetNext(Filter);
			}
			return _aligner.Settings.Mode == AlignerMode.Global || _aligner.Settings.Mode == AlignerMode.HalfLocal ? _sim[_sim.GetLength(0) - 1, _sim.GetLength(1) - 1] : maxScore;
		}

		private void AddNodesToEnd(ShapeNode startNode, Shape shape)
		{
			if (startNode != startNode.List.End)
			{
				ShapeNode start = null;
				foreach (ShapeNode node in startNode.GetNodes().Where(Filter))
				{
					ShapeNode newNode = node.DeepClone();
					shape.Add(newNode);
					if (start == null)
						start = newNode;
				}
				shape.Annotations.Add(start, shape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.SuffixType).Value);
			}
		}

		private IEnumerable<Tuple<Shape, Shape, ShapeNode, ShapeNode, int>> Retrieve(ShapeNode node1, ShapeNode node2, int i, int j, int score, int threshold, bool all)
		{
			if (_aligner.Settings.Mode != AlignerMode.Global && (i == 0 || j == 0))
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
					opScore = _aligner.SigmaSubstitution(_varietyPair, node1, node2);
					if (_sim[i - 1, j - 1] + opScore + score >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1.GetPrev(Filter), node2.GetPrev(Filter), i - 1, j - 1, score + opScore, threshold, all))
						{
							alignment.Item1.Add(node1.DeepClone());
							alignment.Item2.Add(node2.DeepClone());
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if (j != 0)
				{
					opScore = _aligner.SigmaInsertion(_varietyPair, node2);
					if (i == 0 || _sim[i, j - 1] + opScore + score >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1, node2.GetPrev(Filter), i, j - 1, score + opScore, threshold, all))
						{
							alignment.Item1.Add(FeatureStruct.New()
								.Symbol(CogFeatureSystem.NullType)
								.Feature(CogFeatureSystem.StrRep).EqualTo("-").Value);
							alignment.Item2.Add(node2.DeepClone());
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if (!_aligner.Settings.DisableExpansionCompression && i != 0 && j - 2 >= 0)
				{
					opScore = _aligner.SigmaExpansion(_varietyPair, node1, node2.GetPrev(Filter), node2);
					if (_sim[i - 1, j - 2] + opScore + score >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1.GetPrev(Filter), node2.GetPrev(Filter).GetPrev(Filter), i - 1, j - 2, score + opScore, threshold, all))
						{
							alignment.Item1.Add(node1.DeepClone());

							ShapeNode clusterNode1 = node2.GetPrev(Filter).DeepClone();
							alignment.Item2.Add(clusterNode1);
							ShapeNode clusterNode2 = node2.DeepClone();
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
					opScore = _aligner.SigmaDeletion(_varietyPair, node1);
					if (j == 0 || _sim[i - 1, j] + opScore + score >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1.GetPrev(Filter), node2, i - 1, j, score + opScore, threshold, all))
						{
							alignment.Item1.Add(node1.DeepClone());
							alignment.Item2.Add(FeatureStruct.New()
								.Symbol(CogFeatureSystem.NullType)
								.Feature(CogFeatureSystem.StrRep).EqualTo("-").Value);
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if (!_aligner.Settings.DisableExpansionCompression && i - 2 >= 0 && j != 0)
				{
					opScore = _aligner.SigmaCompression(_varietyPair, node1.GetPrev(Filter), node1, node2);
					if (_sim[i - 2, j - 1] + opScore + score >= threshold)
					{
						foreach (Tuple<Shape, Shape, ShapeNode, ShapeNode, int> alignment in Retrieve(node1.GetPrev(Filter).GetPrev(Filter), node2.GetPrev(Filter), i - 2, j - 1, score + opScore, threshold, all))
						{
							ShapeNode clusterNode1 = node1.GetPrev(Filter).DeepClone();
							alignment.Item1.Add(clusterNode1);
							ShapeNode clusterNode2 = node1.DeepClone();
							alignment.Item1.Add(clusterNode2);
							alignment.Item1.Annotations.Add(clusterNode1, clusterNode2, FeatureStruct.New().Symbol(CogFeatureSystem.ClusterType).Value);

							alignment.Item2.Add(node2.DeepClone());
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if ((_aligner.Settings.Mode == AlignerMode.Local || _aligner.Settings.Mode == AlignerMode.HalfLocal) && _sim[i, j] == 0)
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
			var shape = new Shape(_aligner.SpanFactory, begin => new ShapeNode(_aligner.SpanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
			return shape;
		}

		private void AddNodesToBeginning(ShapeNode startNode, Shape shape)
		{
			if (startNode != startNode.List.Begin)
			{
				ShapeNode end = null;
				foreach (ShapeNode node in startNode.GetNodes(Direction.RightToLeft).Where(Filter))
				{
					ShapeNode newNode = node.DeepClone();
					shape.AddAfter(shape.Begin, newNode);
					if (end == null)
						end = newNode;
				}
				shape.Annotations.Add(shape.First, end, FeatureStruct.New().Symbol(CogFeatureSystem.PrefixType).Value);
			}
		}

		private double CalcNormalizedScore(Shape shape1, Shape shape2, int score)
		{
			return Math.Min(1.0, (double) score / Math.Max(CalcMaxScore1(shape1), CalcMaxScore2(shape2)));
		}

		private int CalcMaxScore1(Shape shape1)
		{
			Annotation<ShapeNode> stemAnn = shape1.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			return shape1.Where(node => node.Annotation.Type() != CogFeatureSystem.NullType).Aggregate(0,
				(score, node) => score + (stemAnn != null && stemAnn.Span.Contains(node) ? _aligner.GetMaxScore1(_varietyPair, node) : (_aligner.GetMaxScore1(_varietyPair, node) / 2)));
		}

		private int CalcMaxScore2(Shape shape2)
		{
			Annotation<ShapeNode> stemAnn = shape2.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			return shape2.Where(node => node.Annotation.Type() != CogFeatureSystem.NullType).Aggregate(0,
				(score, node) => score + (stemAnn != null && stemAnn.Span.Contains(node) ? _aligner.GetMaxScore2(_varietyPair, node) : (_aligner.GetMaxScore2(_varietyPair, node) / 2)));
		}
	}
}
