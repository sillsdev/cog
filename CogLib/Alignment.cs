using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog
{
	public class Alignment<T> where T : class
	{
		private readonly AlignmentCell<T>[,] _matrix;
		private readonly ReadOnlyList<AlignmentCell<T>> _prefixes;
		private readonly ReadOnlyList<AlignmentCell<T>> _suffixes;
		private readonly int _rawScore;
		private readonly double _normalizedScore;

		public Alignment(int rawScore, double normalizedScore, params Tuple<IEnumerable<T>, IEnumerable<AlignmentCell<T>>, IEnumerable<T>>[] sequences)
			: this(rawScore, normalizedScore, (IEnumerable<Tuple<IEnumerable<T>, IEnumerable<AlignmentCell<T>>, IEnumerable<T>>>) sequences)
		{
		}

		public Alignment(int rawScore, double normalizedScore, IEnumerable<Tuple<IEnumerable<T>, IEnumerable<AlignmentCell<T>>, IEnumerable<T>>> sequences)
		{
			_rawScore = rawScore;
			_normalizedScore = normalizedScore;
			Tuple<IEnumerable<T>, IEnumerable<AlignmentCell<T>>, IEnumerable<T>>[] sequenceArray = sequences.ToArray();
			var prefixes = new AlignmentCell<T>[sequenceArray.Length];
			var suffixes = new AlignmentCell<T>[sequenceArray.Length];
			for (int i = 0; i < sequenceArray.Length; i++)
			{
				prefixes[i] = new AlignmentCell<T>(sequenceArray[i].Item1.ToArray());

				AlignmentCell<T>[] columnArray = sequenceArray[i].Item2.ToArray();
				if (_matrix == null)
					_matrix = new AlignmentCell<T>[sequenceArray.Length, columnArray.Length];
				for (int j = 0; j < columnArray.Length; j++)
					_matrix[i, j] = columnArray[j];

				suffixes[i] = new AlignmentCell<T>(sequenceArray[i].Item3.ToArray());
			}
			_prefixes = new ReadOnlyList<AlignmentCell<T>>(prefixes);
			_suffixes = new ReadOnlyList<AlignmentCell<T>>(suffixes);
		}

		public int RawScore
		{
			get { return _rawScore; }
		}

		public double NormalizedScore
		{
			get { return _normalizedScore; }
		}

		public int SequenceCount
		{
			get { return _matrix.GetLength(0); }
		}

		public int ColumnCount
		{
			get { return _matrix.GetLength(1); }
		}

		public IReadOnlyList<AlignmentCell<T>> Prefixes
		{
			get { return _prefixes; }
		}

		public IReadOnlyList<AlignmentCell<T>> Suffixes
		{
			get { return _suffixes; }
		}

		public AlignmentCell<T> this[int sequenceIndex, int columnIndex]
		{
			get { return _matrix[sequenceIndex, columnIndex]; }
		}
	}
}
