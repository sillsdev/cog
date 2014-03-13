using System.Collections.Generic;
using System.Collections.Specialized;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain
{
	public class CogProject : ObservableObject
	{
		private int _version;
		private FeatureSystem _featSys;
		private readonly Segmenter _segmenter;

		private readonly KeyedBulkObservableList<string, Variety> _varieties;
		private readonly KeyedBulkObservableList<string, Meaning> _meanings;
		private readonly VarietyPairCollection _varietyPairs;

		private readonly ObservableDictionary<string, IWordAligner> _wordAligners;
		private readonly ObservableDictionary<string, ICognateIdentifier> _cognateIdentifiers; 

		private readonly ObservableDictionary<string, IProcessor<CogProject>> _projectProcessors; 
		private readonly ObservableDictionary<string, IProcessor<Variety>> _varietyProcessors;
		private readonly ObservableDictionary<string, IProcessor<VarietyPair>> _varietyPairProcessors;

		public CogProject(SpanFactory<ShapeNode> spanFactory)
		{
			_segmenter = new Segmenter(spanFactory);
			_meanings = new KeyedBulkObservableList<string, Meaning>(meaning => meaning.Gloss);
			_meanings.CollectionChanged += MeaningsChanged;
			_varieties = new KeyedBulkObservableList<string, Variety>(variety => variety.Name);
			_varieties.CollectionChanged += VarietiesChanged;
			_varietyPairs = new VarietyPairCollection();

			_wordAligners = new ObservableDictionary<string, IWordAligner>();
			_cognateIdentifiers = new ObservableDictionary<string, ICognateIdentifier>();

			_projectProcessors = new ObservableDictionary<string, IProcessor<CogProject>>();
			_varietyProcessors = new ObservableDictionary<string, IProcessor<Variety>>();
			_varietyPairProcessors = new ObservableDictionary<string, IProcessor<VarietyPair>>();
		}

		private void MeaningsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					if (_meanings.Count > 0)
					{
						var meanings = new HashSet<Meaning>(_meanings);
						foreach (Variety variety in _varieties)
							variety.Words.RemoveAll(w => !meanings.Contains(w.Meaning));
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

		public int Version
		{
			get { return _version; }
			set { Set(() => Version, ref _version, value); }
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

		public KeyedBulkObservableList<string, Meaning> Meanings
		{
			get { return _meanings; }
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

		public ObservableDictionary<string, ICognateIdentifier> CognateIdentifiers
		{
			get { return _cognateIdentifiers; }
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
