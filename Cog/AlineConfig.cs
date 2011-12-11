using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class AlineConfig
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private readonly string _configFilePath;
		private readonly FeatureSystem _featSys;
		private readonly Segmenter _segmenter;
		private readonly IDBearerSet<SymbolicFeature> _relevantVowelFeatures;
		private readonly IDBearerSet<SymbolicFeature> _relevantConsFeatures;
		private readonly Dictionary<Tuple<string, string>, SoundChange> _segmentCorrespondences;

		public AlineConfig(SpanFactory<ShapeNode> spanFactory, string configFilePath)
		{
			_spanFactory = spanFactory;
			_configFilePath = configFilePath;
			_featSys = new FeatureSystem();
			_segmenter = new Segmenter(spanFactory);
			_relevantVowelFeatures = new IDBearerSet<SymbolicFeature>();
			_relevantConsFeatures = new IDBearerSet<SymbolicFeature>();
			_segmentCorrespondences = new Dictionary<Tuple<string, string>, SoundChange>();
		}

		public SpanFactory<ShapeNode> SpanFactory
		{
			get { return _spanFactory; }
		}

		public FeatureSystem FeatureSystem
		{
			get { return _featSys; }
		}

		public Segmenter Segmenter
		{
			get { return _segmenter; }
		}

		public IEnumerable<SymbolicFeature> RelevantVowelFeatures
		{
			get { return _relevantVowelFeatures; }
		}

		public IEnumerable<SymbolicFeature> RelevantConsonantFeatures
		{
			get { return _relevantConsFeatures; }
		}

		public IEnumerable<SoundChange> SegmentCorrespondences
		{
			get { return _segmentCorrespondences.Values; }
		}

		public void AddSegmentCorrespondence(SoundChange pair)
		{
			_segmentCorrespondences[Tuple.Create(pair.U, pair.V)] = pair;
		}

		public bool TryGetSegmentCorrespondence(string u, string v, out SoundChange pair)
		{
			return _segmentCorrespondences.TryGetValue(Tuple.Create(u, v), out pair);
		}

		public SoundChange GetSegmentCorrespondence(string u, string v)
		{
			return _segmentCorrespondences[Tuple.Create(u, v)];
		}

		public void Load()
		{
			XElement root = XElement.Load(_configFilePath);
			XElement featSysElem = root.Element("FeatureSystem");
			if (featSysElem != null)
			{
				foreach (XElement featureElem in featSysElem.Elements("Feature"))
				{
					var feat = new SymbolicFeature((string) featureElem.Attribute("id")) {Weight = double.Parse((string) featureElem.Attribute("weight"))};
					foreach (XElement valueElem in featureElem.Elements("Value"))
						feat.AddPossibleSymbol(new FeatureSymbol((string) valueElem.Attribute("id")) {Weight = double.Parse((string) valueElem.Attribute("metric"))});
					_featSys.AddFeature(feat);
				}
			}
			XElement vowelsElem = root.Element("Vowels");
			if (vowelsElem != null)
			{
				_relevantVowelFeatures.UnionWith(((string) vowelsElem.Attribute("relevantFeatures")).Split(' ').Select(id => (SymbolicFeature) _featSys.GetFeature(id)));
				foreach (XElement symbolElem in vowelsElem.Elements("Symbol"))
				{
					string strRep;
					FeatureStruct fs;
					ParseSymbol(symbolElem, out strRep, out fs);
					_segmenter.AddVowel(strRep, fs);
				}
			}

			XElement consElem = root.Element("Consonants");
			if (consElem != null)
			{
				_relevantConsFeatures.UnionWith(((string) consElem.Attribute("relevantFeatures")).Split(' ').Select(id => (SymbolicFeature) _featSys.GetFeature(id)));
				foreach (XElement symbolElem in consElem.Elements("Symbol"))
				{
					string strRep;
					FeatureStruct fs;
					ParseSymbol(symbolElem, out strRep, out fs);
					_segmenter.AddConsonant(strRep, fs);
				}
			}

			XElement modsElem = root.Element("Modifiers");
			if (modsElem != null)
			{
				foreach (XElement symbolElem in modsElem.Elements("Symbol"))
				{
					string strRep;
					FeatureStruct fs;
					ParseSymbol(symbolElem, out strRep, out fs);
					_segmenter.AddModifier(strRep, fs);
				}
			}

			XElement bdrysElem = root.Element("Boundaries");
			if (bdrysElem != null)
			{
				foreach (XElement symbolElem in bdrysElem.Elements("Symbol"))
				{
					string strRep;
					FeatureStruct fs;
					ParseSymbol(symbolElem, out strRep, out fs);
					_segmenter.AddBoundary(strRep);
				}
			}

			XElement tonesElem = root.Element("ToneLetters");
			if (tonesElem != null)
			{
				foreach (XElement symbolElem in tonesElem.Elements("Symbol"))
				{
					string strRep;
					FeatureStruct fs;
					ParseSymbol(symbolElem, out strRep, out fs);
					_segmenter.AddToneLetter(strRep);
				}
			}

			XElement joinersElem = root.Element("Joiners");
			if (joinersElem != null)
			{
				foreach (XElement symbolElem in joinersElem.Elements("Symbol"))
				{
					string strRep;
					FeatureStruct fs;
					ParseSymbol(symbolElem, out strRep, out fs);
					_segmenter.AddJoiner(strRep, fs);
				}
			}
		}

		private void ParseSymbol(XElement symbolElem, out string strRep, out FeatureStruct fs)
		{
			strRep = (string) symbolElem.Attribute("char");
			fs = new FeatureStruct();
			foreach (XElement featureValueElem in symbolElem.Elements("FeatureValue"))
			{
				var feature = (SymbolicFeature) _featSys.GetFeature((string) featureValueElem.Attribute("feature"));
				fs.AddValue(feature, ((string) featureValueElem.Attribute("value")).Split(' ').Select(id => _featSys.GetSymbol(id)));
			}
		}
	}
}
