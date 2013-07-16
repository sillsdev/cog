using System.Collections.Generic;
using System.Collections.Specialized;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain
{
	public class CogProject : ObservableObject
	{
		private FeatureSystem _featSys;
		private readonly Segmenter _segmenter;

		private readonly KeyedBulkObservableList<string, Variety> _varieties;
		private readonly KeyedBulkObservableList<string, Sense> _senses;
		private readonly VarietyPairCollection _varietyPairs;

		private readonly ObservableDictionary<string, IWordAligner> _wordAligners; 

		private readonly ObservableDictionary<string, IProcessor<CogProject>> _projectProcessors; 
		private readonly ObservableDictionary<string, IProcessor<Variety>> _varietyProcessors;
		private readonly ObservableDictionary<string, IProcessor<VarietyPair>> _varietyPairProcessors;

		public CogProject(SpanFactory<ShapeNode> spanFactory)
		{
			_segmenter = new Segmenter(spanFactory);
			_senses = new KeyedBulkObservableList<string, Sense>(sense => sense.Gloss);
			_senses.CollectionChanged += SensesChanged;
			_varieties = new KeyedBulkObservableList<string, Variety>(variety => variety.Name);
			_varieties.CollectionChanged += VarietiesChanged;
			_varietyPairs = new VarietyPairCollection();

			_wordAligners = new ObservableDictionary<string, IWordAligner>();

			_projectProcessors = new ObservableDictionary<string, IProcessor<CogProject>>();
			_varietyProcessors = new ObservableDictionary<string, IProcessor<Variety>>();
			_varietyPairProcessors = new ObservableDictionary<string, IProcessor<VarietyPair>>();
		}

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					if (_senses.Count > 0)
					{
						var senses = new HashSet<Sense>(_senses);
						foreach (Variety variety in _varieties)
							variety.Words.RemoveAll(w => !senses.Contains(w.Sense));
					}
					else
					{
						foreach (Variety variety in _varieties)
							variety.Words.Clear();
					}
					break;
			}
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					if (_varieties.Count > 0)
					{
						var varieties = new HashSet<Variety>(_varieties);
						_varietyPairs.RemoveAll(vp => !varieties.Contains(vp.Variety1) || !varieties.Contains(vp.Variety2));
					}
					else
					{
						_varietyPairs.Clear();
					}
					break;
			}
		}

		public FeatureSystem FeatureSystem
		{
			get { return _featSys; }
			set { Set(() => FeatureSystem, ref _featSys, value); }
		}

		public Segmenter Segmenter
		{
			get { return _segmenter; }
		}

		public KeyedBulkObservableList<string, Sense> Senses
		{
			get { return _senses; }
		}

		public KeyedBulkObservableList<string, Variety> Varieties
		{
			get { return _varieties; }
		}

		public BulkObservableList<VarietyPair> VarietyPairs
		{
			get { return _varietyPairs; }
		}

		public ObservableDictionary<string, IWordAligner> WordAligners
		{
			get { return _wordAligners; }
		}

		public ObservableDictionary<string, IProcessor<CogProject>> ProjectProcessors
		{
			get { return _projectProcessors; }
		}

		public ObservableDictionary<string, IProcessor<Variety>> VarietyProcessors
		{
			get { return _varietyProcessors; }
		}

		public ObservableDictionary<string, IProcessor<VarietyPair>> VarietyPairProcessors
		{
			get { return _varietyPairProcessors; }
		}
	}
}
