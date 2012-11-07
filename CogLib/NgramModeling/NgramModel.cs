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
		public static IReadOnlyList<NgramModel> BuildAll(int maxNgramSize, Variety variety)
		{
			return BuildAll(maxNgramSize, variety, Direction.LeftToRight);
		}

		public static IReadOnlyList<NgramModel> BuildAll(int maxNgramSize, Variety variety, Direction dir)
		{
			return BuildAll(maxNgramSize, variety, dir, new ModifiedKneserNeySmoother());
		}

		public static IReadOnlyList<NgramModel> BuildAll(int maxNgramSize, Variety variety, Direction dir, INgramModelSmoother smoother)
		{
			var model = new NgramModel(maxNgramSize, variety, dir, smoother);
			var list = new List<NgramModel>();
			do
			{
				list.Insert(0, model);
				model = model.Smoother.LowerOrderModel;
			}
			while (model != null);
			return list.AsReadOnlyList();
		}

		public static NgramModel Build(int ngramSize, Variety variety)
		{
			return Build(ngramSize, variety, Direction.LeftToRight);
		}

		public static NgramModel Build(int ngramSize, Variety variety, Direction dir)
		{
			return Build(ngramSize, variety, dir, new ModifiedKneserNeySmoother());
		}

		public static NgramModel Build(int ngramSize, Variety variety, Direction dir, INgramModelSmoother smoother)
		{
			return new NgramModel(ngramSize, variety, dir, smoother);
		}

		private readonly int _ngramSize;
		private readonly HashSet<Ngram> _ngrams;
		private readonly HashSet<string> _categories; 
		private readonly ConditionalFrequencyDistribution<string, Ngram> _ngramCfd;
		private readonly ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> _cfd;
		private readonly INgramModelSmoother _smoother;
		private readonly Variety _variety;
		private readonly Direction _dir;

		private NgramModel(int ngramSize, Variety variety, Direction dir, INgramModelSmoother smoother)
		{
			_ngramSize = ngramSize;
			_variety = variety;
			_dir = dir;
			_smoother = smoother;
			_ngrams = new HashSet<Ngram>();
			_cfd = new ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment>();
			_ngramCfd = new ConditionalFrequencyDistribution<string, Ngram>();
			_categories = new HashSet<string>();
			foreach (Word word in variety.Words)
			{
				if (!string.IsNullOrEmpty(word.Sense.Category))
					_categories.Add(word.Sense.Category);
				foreach (ShapeNode startNode in word.Shape.GetNodes(word.Span, dir).Where(Filter))
				{
					var ngram = new Ngram(startNode.GetNodes(word.Shape.GetEnd(dir), dir).Where(Filter).Take(_ngramSize).Select(node => node.Type() == CogFeatureSystem.AnchorType ? Segment.Anchor : variety.Segments[node]));
					if (ngram.Count != _ngramSize)
						break;

					_ngrams.Add(ngram);
					_ngramCfd[string.Empty].Increment(ngram);
					var context = new Ngram(ngram.Take(_ngramSize - 1));
					var seg = ngram.Last();
					_cfd[Tuple.Create(context, string.Empty)].Increment(seg);

					if (!string.IsNullOrEmpty(word.Sense.Category))
					{
						_ngramCfd[word.Sense.Category].Increment(ngram);
						_cfd[Tuple.Create(context, word.Sense.Category)].Increment(seg);
					}
				}
			}

			_smoother.Smooth(this, _cfd);
		}

		public Direction Direction
		{
			get { return _dir; }
		}

		public double GetProbability(Segment seg, Ngram context)
		{
			return GetProbability(seg, context, string.Empty);
		}

		public double GetProbability(Segment seg, Ngram context, string category)
		{
			return _smoother.GetProbability(seg, context, category);
		}

		public int GetFrequency(Ngram ngram)
		{
			return GetFrequency(ngram, string.Empty);
		}

		public int GetFrequency(Ngram ngram, string category)
		{
			return _ngramCfd[category][ngram];
		}

		public IReadOnlyCollection<Ngram> Ngrams
		{
			get { return _ngrams.AsSimpleReadOnlyCollection(); }
		}

		public IReadOnlyCollection<string> Categories
		{
			get { return _categories.AsSimpleReadOnlyCollection(); }
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
