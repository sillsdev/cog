using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class CogProject : NotifyPropertyChangedBase
	{
		private FeatureSystem _featSys;
		private readonly Segmenter _segmenter;

		private readonly ObservableCollection<Variety> _varieties;
		private readonly ObservableCollection<Sense> _senses;
		private readonly VarietyPairCollection _varietyPairs;

		private readonly ObservableDictionary<string, IAligner> _aligners; 

		private readonly ObservableDictionary<string, IProcessor<CogProject>> _projectProcessors; 
		private readonly ObservableDictionary<string, IProcessor<Variety>> _varietyProcessors;
		private readonly ObservableDictionary<string, IProcessor<VarietyPair>> _varietyPairProcessors;

		public CogProject(SpanFactory<ShapeNode> spanFactory)
		{
			_segmenter = new Segmenter(spanFactory);
			_senses = new ObservableCollection<Sense>();
			_senses.CollectionChanged += SensesChanged;
			_varieties = new ObservableCollection<Variety>();
			_varietyPairs = new VarietyPairCollection();

			_aligners = new ObservableDictionary<string, IAligner>();

			_projectProcessors = new ObservableDictionary<string, IProcessor<CogProject>>();
			_varietyProcessors = new ObservableDictionary<string, IProcessor<Variety>>();
			_varietyPairProcessors = new ObservableDictionary<string, IProcessor<VarietyPair>>();
		}

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Remove:
					RemoveWords(e.OldItems);
					break;

				case NotifyCollectionChangedAction.Reset:
					foreach (Variety variety in _varieties)
						variety.Words.Clear();
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveWords(e.OldItems);
					break;
			}
		}

		private void RemoveWords(IList senses)
		{
			foreach (Variety variety in _varieties)
				foreach (Sense sense in senses)
					variety.Words.RemoveAll(sense);
		}

		public FeatureSystem FeatureSystem
		{
			get { return _featSys; }
			set
			{
				_featSys = value;
				OnPropertyChanged("FeatureSystem");
			}
		}

		public Segmenter Segmenter
		{
			get { return _segmenter; }
		}

		public ObservableCollection<Sense> Senses
		{
			get { return _senses; }
		}

		public ObservableCollection<Variety> Varieties
		{
			get { return _varieties; }
		}

		public ObservableCollection<VarietyPair> VarietyPairs
		{
			get { return _varietyPairs; }
		}

		public ObservableDictionary<string, IAligner> Aligners
		{
			get { return _aligners; }
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
