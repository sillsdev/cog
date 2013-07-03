using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Cog.Statistics;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.NgramModeling
{
	public class NgramModel
	{
		public static IEnumerable<NgramModel> TrainAll(int maxNgramSize, Variety variety)
		{
			return TrainAll(maxNgramSize, variety, Direction.LeftToRight);
		}

		public static IEnumerable<NgramModel> TrainAll(int maxNgramSize, Variety variety, Direction dir)
		{
			return TrainAll(maxNgramSize, variety, dir, () => new ModifiedKneserNeySmoother());
		}

		public static IEnumerable<NgramModel> TrainAll(int maxNgramSize, Variety variety, Func<INgramModelSmoother> smootherFactory)
		{
			return TrainAll(maxNgramSize, variety, Direction.LeftToRight, smootherFactory);
		}

		public static IEnumerable<NgramModel> TrainAll(int maxNgramSize, Variety variety, Direction dir, Func<INgramModelSmoother> smootherFactory)
		{
			var model = new NgramModel(maxNgramSize, variety, dir, smootherFactory());
			var models = new NgramModel[maxNgramSize];
			for (int i = maxNgramSize - 1; i >= 0; i--)
			{
				models[i] = model;
				if (i > 0)
					model = model.Smoother.LowerOrderModel ?? new NgramModel(i, variety, dir, smootherFactory());
			}
			return models;
		}

		public static NgramModel Train(int ngramSize, Variety variety)
		{
			return Train(ngramSize, variety, Direction.LeftToRight);
		}

		public static NgramModel Train(int ngramSize, Variety variety, Direction dir)
		{
			return Train(ngramSize, variety, dir, new ModifiedKneserNeySmoother());
		}

		public static NgramModel Train(int ngramSize, Variety variety, INgramModelSmoother smoother)
		{
			return Train(ngramSize, variety, Direction.LeftToRight, smoother);
		}

		public static NgramModel Train(int ngramSize, Variety variety, Direction dir, INgramModelSmoother smoother)
		{
			return new NgramModel(ngramSize, variety, dir, smoother);
		}

		private readonly int _ngramSize;
		private readonly HashSet<Ngram> _ngrams;
		private readonly HashSet<string> _categories; 
		private readonly INgramModelSmoother _smoother;
		private readonly Variety _variety;
		private readonly Direction _dir;

		internal NgramModel(int ngramSize, Variety variety, Direction dir, INgramModelSmoother smoother)
		{
			_ngramSize = ngramSize;
			_variety = variety;
			_dir = dir;
			_smoother = smoother;
			_ngrams = new HashSet<Ngram>();
			var cfd = new ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment>();
			_categories = new HashSet<string>();
			foreach (Word word in variety.Words)
			{
				if (!string.IsNullOrEmpty(word.Sense.Category))
					_categories.Add(word.Sense.Category);
				foreach (ShapeNode startNode in word.Shape.GetNodes(word.Span).Where(Filter))
				{
					var ngram = new Ngram(startNode.GetNodes(word.Shape.End).Where(Filter).Take(_ngramSize).Select(variety.SegmentPool.Get));
					if (ngram.Count != _ngramSize)
						break;

					_ngrams.Add(ngram);
					Ngram context = ngram.TakeAllExceptLast(dir);
					Segment seg = ngram.GetLast(dir);
					cfd[Tuple.Create(context, (string) null)].Increment(seg);

					if (!string.IsNullOrEmpty(word.Sense.Category))
						cfd[Tuple.Create(context, word.Sense.Category)].Increment(seg);
				}
			}

			_smoother.Smooth(ngramSize, variety, dir, cfd);
		}

		public Direction Direction
		{
			get { return _dir; }
		}

		public double GetProbability(Segment seg, Ngram context)
		{
			if (context.Count != _ngramSize - 1)
				throw new ArgumentException("The context size is not valid.", "context");
			return GetProbability(seg, context, null);
		}

		public double GetProbability(Segment seg, Ngram context, string category)
		{
			if (context.Count != _ngramSize - 1)
				throw new ArgumentException("The context size is not valid.", "context");
			return _smoother.GetProbability(seg, context, category);
		}

		public IReadOnlyCollection<Ngram> Ngrams
		{
			get { return _ngrams.ToReadOnlyCollection(); }
		}

		public IReadOnlyCollection<string> Categories
		{
			get { return _categories.ToReadOnlyCollection(); }
		}

		public INgramModelSmoother Smoother
		{
			get { return _smoother; }
		}

		public int NgramSize
		{
			get { return _ngramSize; }
		}

		public Variety Variety
		{
			get { return _variety; }
		}

		private static bool Filter(ShapeNode node)
		{
			return node.Annotation.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType, CogFeatureSystem.AnchorType);
		}
	}
}
