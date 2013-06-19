using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.Cog.SequenceAlignment
{
	public enum AlignmentMode
	{
		Global = 0,
		SemiGlobal,
		HalfLocal,
		Local
	}

	public class PairwiseAlignmentAlgorithm<T> where T : class
	{
		private readonly int[,] _sim;
		private readonly IPairwiseAlignmentScorer<T> _scorer;
		private readonly int _startIndex1;
		private readonly int _count1;
		private readonly T[] _sequence1;
		private readonly int _startIndex2;
		private readonly int _count2;
		private readonly T[] _sequence2;
		private int _bestRawScore = -1;

		public PairwiseAlignmentAlgorithm(IPairwiseAlignmentScorer<T> scorer, IEnumerable<T> sequence1, IEnumerable<T> sequence2)
			: this(scorer, sequence1, 0, -1, sequence2, 0, -1)
		{
		}

		public PairwiseAlignmentAlgorithm(IPairwiseAlignmentScorer<T> scorer, IEnumerable<T> sequence1, int startIndex1, int count1, IEnumerable<T> sequence2, int startIndex2, int count2)
		{
			_scorer = scorer;
			_sequence1 = sequence1.ToArray();
			_startIndex1 = startIndex1;
			_count1 = count1 < 0 ? _sequence1.Length - _startIndex1 : count1;
			_sequence2 = sequence2.ToArray();
			_startIndex2 = startIndex2;
			_count2 = count2 < 0 ? _sequence2.Length - _startIndex2 : count2;
			_sim = new int[_count1 + 1, _count2 + 1];
		}

		public AlignmentMode Mode { get; set; }
		public bool ExpansionCompressionEnabled { get; set; }

		public int BestRawScore
		{
			get { return _bestRawScore; }
		}

		public void Compute()
		{
			int maxScore = int.MinValue;

			if (Mode == AlignmentMode.Global)
			{
				for (int i = 1; i < _sim.GetLength(0); i++)
				{
					_sim[i, 0] = _sim[i - 1, 0] + _scorer.GetDeletionScore(Get1(i), null);
				}

				for (int j = 1; j < _sim.GetLength(1); j++)
				{
					_sim[0, j] = _sim[0, j - 1] + _scorer.GetInsertionScore(null, Get2(j));
				}
			}

			for (int i = 1; i < _sim.GetLength(0); i++)
			{
				for (int j = 1; j < _sim.GetLength(1); j++)
				{
					int m1 = _sim[i - 1, j] + _scorer.GetDeletionScore(Get1(i), Get2(j));
					int m2 = _sim[i, j - 1] + _scorer.GetInsertionScore(Get1(i), Get2(j));
					int m3 = _sim[i - 1, j - 1] + _scorer.GetSubstitutionScore(Get1(i), Get2(j));
					int m4 = !ExpansionCompressionEnabled || j - 2 < 0 ? int.MinValue : _sim[i - 1, j - 2] + _scorer.GetExpansionScore(Get1(i), Get2(j - 1), Get2(j));
					int m5 = !ExpansionCompressionEnabled || i - 2 < 0 ? int.MinValue : _sim[i - 2, j - 1] + _scorer.GetCompressionScore(Get1(i - 1), Get1(i), Get2(j));

					if (Mode == AlignmentMode.Local || Mode == AlignmentMode.HalfLocal)
						_sim[i, j] = new[] {m1, m2, m3, m4, m5, 0}.Max();
					else
						_sim[i, j] = new[] {m1, m2, m3, m4, m5}.Max();

					if (_sim[i, j] > maxScore)
					{
						if (Mode == AlignmentMode.SemiGlobal)
						{
							if (i == _sim.GetLength(0) - 1 || j == _sim.GetLength(1) - 1)
								maxScore = _sim[i, j];
						}
						else
						{
							maxScore = _sim[i, j];
						}
					}
				}
			}
			_bestRawScore = Mode == AlignmentMode.Global || Mode == AlignmentMode.HalfLocal ? _sim[_sim.GetLength(0) - 1, _sim.GetLength(1) - 1] : maxScore;
		}

		private T Get1(int i)
		{
			if (i == 0)
				return null;
			return _sequence1[_startIndex1 + i - 1];
		}

		private T Get2(int j)
		{
			if (j == 0)
				return null;
			return _sequence2[_startIndex2 + j - 1];
		}

		public IEnumerable<Alignment<T>> GetAlignments()
		{
			return GetAlignments(_bestRawScore, false);
		}

		public IEnumerable<Alignment<T>> GetAlignments(double scoreMargin)
		{
			return GetAlignments((int) (scoreMargin * _bestRawScore), true);
		}

		private IEnumerable<Alignment<T>> GetAlignments(int threshold, bool all)
		{
			switch (Mode)
			{
				case AlignmentMode.Global:
				case AlignmentMode.HalfLocal:
					{
						foreach (Alignment<T> alignment in GetAlignments(_sim.GetLength(0) - 1, _sim.GetLength(1) - 1, threshold, all))
							yield return alignment;
					}
					break;

				case AlignmentMode.SemiGlobal:
					{
						for (int i = 1; i < _sim.GetLength(0); i++)
						{
							foreach (Alignment<T> alignment in GetAlignments(i, _sim.GetLength(1) - 1, threshold, all))
								yield return alignment;
						}

						for (int j = 1; j < _sim.GetLength(1); j++)
						{
							foreach (Alignment<T> alignment in GetAlignments(_sim.GetLength(0) - 1, j, threshold, all))
								yield return alignment;
						}
					}
					break;

				case AlignmentMode.Local:
					{
						for (int i = 1; i < _sim.GetLength(0); i++)
						{
							for (int j = 1; j < _sim.GetLength(1); j++)
							{
								foreach (Alignment<T> alignment in GetAlignments(i, j, threshold, all))
									yield return alignment;
							}
						}
					}
					break;
			}
		}

		private IEnumerable<Alignment<T>> GetAlignments(int i, int j, int threshold, bool all)
		{
			if (_sim[i, j] < threshold)
				yield break;

			foreach (Tuple<List<AlignmentCell<T>>, List<AlignmentCell<T>>, int, int, int> alignment in Retrieve(i, j, 0, threshold, all))
			{
				int startIndex1 = alignment.Item3;
				int endIndex1 = _startIndex1 + i;
				int startIndex2 = alignment.Item4;
				int endIndex2 = _startIndex2 + j;

				yield return new Alignment<T>(alignment.Item5, CalcNormalizedScore(startIndex1, endIndex1, startIndex2, endIndex2, alignment.Item5),
					Tuple.Create(_sequence1.Take(startIndex1), (IEnumerable<AlignmentCell<T>>) alignment.Item1, _sequence1.Skip(endIndex1)),
					Tuple.Create(_sequence2.Take(startIndex2), (IEnumerable<AlignmentCell<T>>) alignment.Item2, _sequence2.Skip(endIndex2)));
			}
		}

		private double CalcNormalizedScore(int startIndex1, int endIndex1, int startIndex2, int endIndex2, int score)
		{
			return Math.Max(0.0, Math.Min(1.0, (double) score / Math.Max(CalcMaxScore1(startIndex1, endIndex1), CalcMaxScore2(startIndex2, endIndex2))));
		}

		private int CalcMaxScore1(int startIndex, int endIndex)
		{
			int sum = 0;
			for (int i = _startIndex1; i < _count1; i++)
			{
				int score = _scorer.GetMaxScore1(_sequence1[i]);
				sum += (i < startIndex || i >= endIndex) ? score / 2 : score;
			}
			return sum;
		}

		private int CalcMaxScore2(int startIndex, int endIndex)
		{
			int sum = 0;
			for (int j = _startIndex2; j < _count2; j++)
			{
				int score = _scorer.GetMaxScore2(_sequence2[j]);
				sum += (j < startIndex || j >= endIndex) ? score / 2 : score;
			}
			return sum;
		}

		private IEnumerable<Tuple<List<AlignmentCell<T>>, List<AlignmentCell<T>>, int, int, int>> Retrieve(int i, int j, int score, int threshold, bool all)
		{
			if (Mode != AlignmentMode.Global && (i == 0 || j == 0))
			{
				yield return CreateAlignment(i, j, score);
			}
			else if (i == 0 && j == 0)
			{
				yield return CreateAlignment(i, j, score);
			}
			else
			{
				int opScore;
				if (i != 0 && j != 0)
				{
					opScore = _scorer.GetSubstitutionScore(Get1(i), Get2(j));
					if (_sim[i - 1, j - 1] + opScore + score >= threshold)
					{
						foreach (Tuple<List<AlignmentCell<T>>, List<AlignmentCell<T>>, int, int, int> alignment in Retrieve(i - 1, j - 1, score + opScore, threshold, all))
						{
							alignment.Item1.Add(new AlignmentCell<T>(Get1(i)));
							alignment.Item2.Add(new AlignmentCell<T>(Get2(j)));
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if (j != 0)
				{
					opScore = _scorer.GetInsertionScore(Get1(i), Get2(j));
					if (i == 0 || _sim[i, j - 1] + opScore + score >= threshold)
					{
						foreach (Tuple<List<AlignmentCell<T>>, List<AlignmentCell<T>>, int, int, int> alignment in Retrieve(i, j - 1, score + opScore, threshold, all))
						{
							alignment.Item1.Add(new AlignmentCell<T>());
							alignment.Item2.Add(new AlignmentCell<T>(Get2(j)));
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if (ExpansionCompressionEnabled && i != 0 && j - 2 >= 0)
				{
					opScore = _scorer.GetExpansionScore(Get1(i), Get2(j - 1), Get2(j));
					if (_sim[i - 1, j - 2] + opScore + score >= threshold)
					{
						foreach (Tuple<List<AlignmentCell<T>>, List<AlignmentCell<T>>, int, int, int> alignment in Retrieve(i - 1, j - 2, score + opScore, threshold, all))
						{
							alignment.Item1.Add(new AlignmentCell<T>(Get1(i)));
							alignment.Item2.Add(new AlignmentCell<T>(Get2(j - 1), Get2(j)));
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if (i != 0)
				{
					opScore = _scorer.GetDeletionScore(Get1(i), Get2(j));
					if (j == 0 || _sim[i - 1, j] + opScore + score >= threshold)
					{
						foreach (Tuple<List<AlignmentCell<T>>, List<AlignmentCell<T>>, int, int, int> alignment in Retrieve(i - 1, j, score + opScore, threshold, all))
						{
							alignment.Item1.Add(new AlignmentCell<T>(Get1(i)));
							alignment.Item2.Add(new AlignmentCell<T>());
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if (ExpansionCompressionEnabled && i - 2 >= 0 && j != 0)
				{
					opScore = _scorer.GetCompressionScore(Get1(i - 1), Get1(i), Get2(j));
					if (_sim[i - 2, j - 1] + opScore + score >= threshold)
					{
						foreach (Tuple<List<AlignmentCell<T>>, List<AlignmentCell<T>>, int, int, int> alignment in Retrieve(i - 2, j - 1, score + opScore, threshold, all))
						{
							alignment.Item1.Add(new AlignmentCell<T>(_sequence1[i - 2], _sequence1[i - 1]));
							alignment.Item2.Add(new AlignmentCell<T>(_sequence2[j - 1]));
							yield return alignment;
							if (!all)
								yield break;
						}
					}
				}

				if ((Mode == AlignmentMode.Local || Mode == AlignmentMode.HalfLocal) && _sim[i, j] == 0)
					yield return CreateAlignment(i, j, score);
			}
		}

		private Tuple<List<AlignmentCell<T>>, List<AlignmentCell<T>>, int, int, int> CreateAlignment(int i, int j, int score)
		{
			return Tuple.Create(new List<AlignmentCell<T>>(), new List<AlignmentCell<T>>(), _startIndex1 + i, _startIndex2 + j, score);
		}
	}
}
