using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class CogConfig
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private readonly string _configFilePath;
		private readonly FeatureSystem _featSys;

		private WordListsLoader _loader;
		private IReadOnlyList<IProcessor<Variety>> _varietyProcessors;
		private IReadOnlyList<IProcessor<VarietyPair>> _varietyPairProcessors;
		private string _outputPath;

		public CogConfig(SpanFactory<ShapeNode> spanFactory, string configFilePath)
		{
			_spanFactory = spanFactory;
			_configFilePath = configFilePath;
			_featSys = new FeatureSystem();
		}

		public FeatureSystem FeatureSystem
		{
			get { return _featSys; }
		}

		public WordListsLoader Loader
		{
			get { return _loader; }
		}

		public IReadOnlyList<IProcessor<Variety>> VarietyProcessors
		{
			get { return _varietyProcessors; }
		}

		public IReadOnlyList<IProcessor<VarietyPair>> VarietyPairProcessors
		{
			get { return _varietyPairProcessors; }
		}

		public string OutputPath
		{
			get { return _outputPath; }
		}

		public void Load()
		{
			XElement root = XElement.Load(_configFilePath);
			string dirPath = Path.GetDirectoryName(_configFilePath);
			var segmentPath = (string) root.Element("Segmentation");
			if (dirPath != null && !Path.IsPathRooted(segmentPath))
				segmentPath = Path.Combine(dirPath, segmentPath);

			Segmenter segmenter;
			IDBearerSet<SymbolicFeature> relevantVowelFeatures, relevantConsFeatures;
			IDBearerSet<NaturalClass> naturalClasses;
			LoadSegmentation(segmentPath, out segmenter, out relevantVowelFeatures, out relevantConsFeatures, out naturalClasses);
			XElement wordlistsElem = root.Element("Wordlists");
			Debug.Assert(wordlistsElem != null);

			var wordlistsPath = (string) wordlistsElem;
			if (dirPath != null && !Path.IsPathRooted(wordlistsPath))
				wordlistsPath = Path.Combine(dirPath, wordlistsPath);
			switch ((string) wordlistsElem.Attribute("type"))
			{
				case "text":
					_loader = new TextLoader(segmenter, wordlistsPath);
					break;
				case "wordsurv":
					_loader = new WordSurvXmlLoader(segmenter, wordlistsPath);
					break;
				case "comparanda":
					_loader = new ComparandaLoader(segmenter, wordlistsPath);
					break;
			}

			var varietyProcessors = new List<IProcessor<Variety>>();
			XElement stemmingElem = root.Element("Stemming");
			if (stemmingElem != null)
			{
				var stemmingType = (string) stemmingElem.Attribute("type");
				switch (stemmingType)
				{
					case "heuristic":
						var stemThresholdStr = (string) stemmingElem.Element("AffixThreshold");
						var maxAffixLenStr = (string) stemmingElem.Element("MaxAffixLength");
						var catRequired = (string) stemmingElem.Element("CategoryRequired");
						varietyProcessors.Add(new UnsupervisedAffixIdentifier(_spanFactory, double.Parse(stemThresholdStr), int.Parse(maxAffixLenStr), catRequired != null && bool.Parse(catRequired)));
						varietyProcessors.Add(new Stemmer(_spanFactory));
						break;

					case "list":
						break;
				}
			}

			XElement alignmentElem = root.Element("Alignment");
			Debug.Assert(alignmentElem != null);
			var settings = new EditDistanceSettings();
			var modeStr = (string) alignmentElem.Element("Mode");
			if (modeStr != null)
			{
				switch (modeStr)
				{
					case "local":
						settings.Mode = EditDistanceMode.Local;
						break;
					case "global":
						settings.Mode = EditDistanceMode.Global;
						break;
					case "semi-global":
						settings.Mode = EditDistanceMode.SemiGlobal;
						break;
					case "half-local":
						settings.Mode = EditDistanceMode.HalfLocal;
						break;
				}
			}
			var disableExpansionCompressionStr = (string) alignmentElem.Element("DisableExpansionCompression");
			if (disableExpansionCompressionStr != null)
				settings.DisableExpansionCompression = bool.Parse(disableExpansionCompressionStr);
			var aline = new Aline(_spanFactory, relevantVowelFeatures, relevantConsFeatures, settings);
			var soundChangeAline = new SoundChangeAline(_spanFactory, relevantVowelFeatures, relevantConsFeatures, naturalClasses, settings);

			var varietyPairProcessors = new List<IProcessor<VarietyPair>> {new WordPairGenerator(aline)};

			XElement soundChangeElem = root.Element("SoundChangeInducer");
			Debug.Assert(soundChangeElem != null);
			var soundChangeThresholdStr = (string) soundChangeElem.Element("AlignmentThreshold");
			varietyPairProcessors.Add(new EMSoundChangeInducer(aline, soundChangeAline, double.Parse(soundChangeThresholdStr)));

			XElement cognateIdentElem = root.Element("CognateIdentification");
			Debug.Assert(cognateIdentElem != null);
			switch ((string) cognateIdentElem.Attribute("type"))
			{
				case "blair":
					XElement similarSegmentsElem = cognateIdentElem.Element("SimilarSegments");
					Debug.Assert(similarSegmentsElem != null);
					switch ((string) similarSegmentsElem.Attribute("type"))
					{
						case "list":
							var vowelsPath = (string) similarSegmentsElem.Element("SimilarVowels");
							if (dirPath != null && !Path.IsPathRooted(vowelsPath))
								vowelsPath = Path.Combine(dirPath, vowelsPath);
							var consPath = (string) similarSegmentsElem.Element("SimilarConsonants");
							if (dirPath != null && !Path.IsPathRooted(consPath))
								consPath = Path.Combine(dirPath, consPath);
							var genVowelsStr = (string) similarSegmentsElem.Element("GenerateDiphthongs") ?? "false";
							varietyPairProcessors.Add(new ListSimilarSegmentIdentifier(vowelsPath, consPath, segmenter.Joiners, bool.Parse(genVowelsStr)));
							break;

						case "threshold":
							var vowelThresholdStr = (string) similarSegmentsElem.Element("VowelThreshold");
							var consThresholdStr = (string) similarSegmentsElem.Element("ConsonantThreshold");
							varietyPairProcessors.Add(new ThresholdSimilarSegmentIdentifier(aline, int.Parse(vowelThresholdStr), int.Parse(consThresholdStr)));
							break;
					}
					var blairThresholdStr = (string) cognateIdentElem.Element("AlignmentThreshold");
					varietyPairProcessors.Add(new BlairCognateIdentifier(soundChangeAline, double.Parse(blairThresholdStr)));
					break;

				case "aline":
					var alineThresholdStr = (string) cognateIdentElem.Element("AlignmentThreshold");
					varietyPairProcessors.Add(new ThresholdCognateIdentifier(soundChangeAline, double.Parse(alineThresholdStr)));
					break;
			}

			_outputPath = (string) root.Element("Output");
			if (dirPath != null && !Path.IsPathRooted(_outputPath))
				_outputPath = Path.Combine(dirPath, _outputPath);
			varietyProcessors.Add(new VarietyTextOutput(_outputPath));
			varietyPairProcessors.Add(new VarietyPairTextOutput(_outputPath, soundChangeAline));

			_varietyProcessors = new ReadOnlyList<IProcessor<Variety>>(varietyProcessors);
			_varietyPairProcessors = new ReadOnlyList<IProcessor<VarietyPair>>(varietyPairProcessors);
		}

		private void LoadSegmentation(string path, out Segmenter segmenter, out IDBearerSet<SymbolicFeature> relevantVowelFeatures,
			out IDBearerSet<SymbolicFeature> relevantConsFeatures, out IDBearerSet<NaturalClass> naturalClasses)
		{
			XElement root = XElement.Load(path);
			segmenter = new Segmenter(_spanFactory);
			relevantVowelFeatures = new IDBearerSet<SymbolicFeature>();
			relevantConsFeatures = new IDBearerSet<SymbolicFeature>();
			naturalClasses = new IDBearerSet<NaturalClass>();
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
				relevantVowelFeatures.UnionWith(((string) vowelsElem.Attribute("relevantFeatures")).Split(' ').Select(id => (SymbolicFeature) _featSys.GetFeature(id)));
				XElement naturalClassesElem = vowelsElem.Element("NaturalClasses");
				if (naturalClassesElem != null)
				{
					foreach (XElement ncElem in naturalClassesElem.Elements("NaturalClass"))
					{
						FeatureStruct fs = ParseFeatureStruct(ncElem);
						fs.AddValue(CogFeatureSystem.Type, CogFeatureSystem.VowelType);
						naturalClasses.Add(new NaturalClass((string) ncElem.Attribute("id"), fs));
					}
				}
				XElement symbolsElem = vowelsElem.Element("Symbols");
				if (symbolsElem != null)
				{
					foreach (XElement symbolElem in symbolsElem.Elements("Symbol"))
					{
						FeatureStruct fs = ParseFeatureStruct(symbolElem);
						segmenter.AddVowel((string) symbolElem.Attribute("char"), fs);
					}
				}
			}

			XElement consElem = root.Element("Consonants");
			if (consElem != null)
			{
				relevantConsFeatures.UnionWith(((string) consElem.Attribute("relevantFeatures")).Split(' ').Select(id => (SymbolicFeature) _featSys.GetFeature(id)));
				XElement naturalClassesElem = consElem.Element("NaturalClasses");
				if (naturalClassesElem != null)
				{
					foreach (XElement ncElem in naturalClassesElem.Elements("NaturalClass"))
					{
						FeatureStruct fs = ParseFeatureStruct(ncElem);
						fs.AddValue(CogFeatureSystem.Type, CogFeatureSystem.ConsonantType);
						naturalClasses.Add(new NaturalClass((string) ncElem.Attribute("id"), fs));
					}
				}
				XElement symbolsElem = consElem.Element("Symbols");
				if (symbolsElem != null)
				{
					foreach (XElement symbolElem in symbolsElem.Elements("Symbol"))
					{
						FeatureStruct fs = ParseFeatureStruct(symbolElem);
						segmenter.AddConsonant((string) symbolElem.Attribute("char"), fs);
					}
				}
			}

			XElement modsElem = root.Element("Modifiers");
			if (modsElem != null)
			{
				foreach (XElement symbolElem in modsElem.Elements("Symbol"))
				{
					FeatureStruct fs = ParseFeatureStruct(symbolElem);
					segmenter.AddModifier((string) symbolElem.Attribute("char"), fs, ((string) symbolElem.Attribute("overwrite")) != "false");
				}
			}

			XElement bdrysElem = root.Element("Boundaries");
			if (bdrysElem != null)
			{
				foreach (XElement symbolElem in bdrysElem.Elements("Symbol"))
					segmenter.AddBoundary((string) symbolElem.Attribute("char"));
			}

			XElement tonesElem = root.Element("ToneLetters");
			if (tonesElem != null)
			{
				foreach (XElement symbolElem in tonesElem.Elements("Symbol"))
					segmenter.AddToneLetter((string) symbolElem.Attribute("char"));
			}

			XElement joinersElem = root.Element("Joiners");
			if (joinersElem != null)
			{
				foreach (XElement symbolElem in joinersElem.Elements("Symbol"))
				{
					FeatureStruct fs = ParseFeatureStruct(symbolElem);
					segmenter.AddJoiner((string) symbolElem.Attribute("char"), fs);
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
