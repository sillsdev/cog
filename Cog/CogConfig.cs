using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class CogConfig
	{
		private readonly string _configFilePath;
		private readonly FeatureSystem _featSys;
		private readonly Segmenter _segmenter;
		private readonly IDBearerSet<SymbolicFeature> _relevantVowelFeatures;
		private readonly IDBearerSet<SymbolicFeature> _relevantConsFeatures;
		private readonly IDBearerSet<NaturalClass> _naturalClasses; 

		public CogConfig(SpanFactory<ShapeNode> spanFactory, string configFilePath)
		{
			_configFilePath = configFilePath;
			_featSys = new FeatureSystem();
			_segmenter = new Segmenter(spanFactory);
			_relevantVowelFeatures = new IDBearerSet<SymbolicFeature>();
			_relevantConsFeatures = new IDBearerSet<SymbolicFeature>();
			_naturalClasses = new IDBearerSet<NaturalClass>();
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

		public IEnumerable<NaturalClass> NaturalClasses
		{
			get { return _naturalClasses; }
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
						feat.PossibleSymbols.Add(new FeatureSymbol((string) valueElem.Attribute("id")) {Weight = 100.0 * double.Parse((string) valueElem.Attribute("metric"))});
					_featSys.Add(feat);
				}
			}
			XElement vowelsElem = root.Element("Vowels");
			if (vowelsElem != null)
			{
				_relevantVowelFeatures.UnionWith(((string) vowelsElem.Attribute("relevantFeatures")).Split(' ').Select(id => (SymbolicFeature) _featSys.GetFeature(id)));
				XElement naturalClassesElem = vowelsElem.Element("NaturalClasses");
				if (naturalClassesElem != null)
				{
					foreach (XElement ncElem in naturalClassesElem.Elements("NaturalClass"))
					{
						FeatureStruct fs = ParseFeatureStruct(ncElem);
						fs.AddValue(CogFeatureSystem.Type, CogFeatureSystem.VowelType);
						_naturalClasses.Add(new NaturalClass((string) ncElem.Attribute("id"), fs));
					}
				}
				XElement symbolsElem = vowelsElem.Element("Symbols");
				if (symbolsElem != null)
				{
					foreach (XElement symbolElem in symbolsElem.Elements("Symbol"))
					{
						FeatureStruct fs = ParseFeatureStruct(symbolElem);
						_segmenter.AddVowel((string) symbolElem.Attribute("char"), fs);
					}
				}
			}

			XElement consElem = root.Element("Consonants");
			if (consElem != null)
			{
				_relevantConsFeatures.UnionWith(((string) consElem.Attribute("relevantFeatures")).Split(' ').Select(id => (SymbolicFeature) _featSys.GetFeature(id)));
				XElement naturalClassesElem = consElem.Element("NaturalClasses");
				if (naturalClassesElem != null)
				{
					foreach (XElement ncElem in naturalClassesElem.Elements("NaturalClass"))
					{
						FeatureStruct fs = ParseFeatureStruct(ncElem);
						fs.AddValue(CogFeatureSystem.Type, CogFeatureSystem.ConsonantType);
						_naturalClasses.Add(new NaturalClass((string) ncElem.Attribute("id"), fs));
					}
				}
				XElement symbolsElem = consElem.Element("Symbols");
				if (symbolsElem != null)
				{
					foreach (XElement symbolElem in symbolsElem.Elements("Symbol"))
					{
						FeatureStruct fs = ParseFeatureStruct(symbolElem);
						_segmenter.AddConsonant((string) symbolElem.Attribute("char"), fs);
					}
				}
			}

			XElement modsElem = root.Element("Modifiers");
			if (modsElem != null)
			{
				foreach (XElement symbolElem in modsElem.Elements("Symbol"))
				{
					FeatureStruct fs = ParseFeatureStruct(symbolElem);
					_segmenter.AddModifier((string) symbolElem.Attribute("char"), fs, ((string) symbolElem.Attribute("overwrite")) != "false");
				}
			}

			XElement bdrysElem = root.Element("Boundaries");
			if (bdrysElem != null)
			{
				foreach (XElement symbolElem in bdrysElem.Elements("Symbol"))
					_segmenter.AddBoundary((string) symbolElem.Attribute("char"));
			}

			XElement tonesElem = root.Element("ToneLetters");
			if (tonesElem != null)
			{
				foreach (XElement symbolElem in tonesElem.Elements("Symbol"))
					_segmenter.AddToneLetter((string) symbolElem.Attribute("char"));
			}

			XElement joinersElem = root.Element("Joiners");
			if (joinersElem != null)
			{
				foreach (XElement symbolElem in joinersElem.Elements("Symbol"))
				{
					FeatureStruct fs = ParseFeatureStruct(symbolElem);
					_segmenter.AddJoiner((string) symbolElem.Attribute("char"), fs);
				}
			}
		}

		private FeatureStruct ParseFeatureStruct(XElement elem)
		{
			var fs = new FeatureStruct();
			foreach (XElement featureValueElem in elem.Elements("FeatureValue"))
			{
				var feature = (SymbolicFeature)_featSys.GetFeature((string)featureValueElem.Attribute("feature"));
				fs.AddValue(feature, ((string)featureValueElem.Attribute("value")).Split(' ').Select(id => _featSys.GetSymbol(id)));
			}
			return fs;
		}
	}
}
