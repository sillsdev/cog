using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Config
{
	public static class ConfigManager
	{
		public static CogProject Load(SpanFactory<ShapeNode> spanFactory, string configFilePath)
		{
			var project = new CogProject(spanFactory);

			XElement root = XElement.Load(configFilePath);

			var featSys = new FeatureSystem();
			XElement featSysElem = root.Element("FeatureSystem");
			Debug.Assert(featSysElem != null);
			foreach (XElement featureElem in featSysElem.Elements("Feature"))
			{
				var feat = new SymbolicFeature((string) featureElem.Attribute("id")) {Description = (string) featureElem.Attribute("name")};
				foreach (XElement valueElem in featureElem.Elements("Value"))
					feat.PossibleSymbols.Add(new FeatureSymbol((string) valueElem.Attribute("id")) {Description = (string) valueElem.Attribute("name")});
				featSys.Add(feat);
			}
			featSys.Freeze();
			project.FeatureSystem = featSys;

			XElement segmentationElem = root.Element("Segmentation");
			Debug.Assert(segmentationElem != null);
			XElement vowelsElem = segmentationElem.Element("Vowels");
			if (vowelsElem != null)
			{
				var maxLenStr = (string) vowelsElem.Attribute("maxLength");
				if (!string.IsNullOrEmpty(maxLenStr))
					project.Segmenter.MaxVowelLength = int.Parse(maxLenStr);

				ParseSymbols(project.FeatureSystem, vowelsElem, project.Segmenter.Vowels);
			}

			XElement consElem = segmentationElem.Element("Consonants");
			if (consElem != null)
			{
				var maxLenStr = (string) consElem.Attribute("maxLength");
				if (!string.IsNullOrEmpty(maxLenStr))
					project.Segmenter.MaxConsonantLength = int.Parse(maxLenStr);

				ParseSymbols(project.FeatureSystem, consElem, project.Segmenter.Consonants);
			}

			XElement modsElem = segmentationElem.Element("Modifiers");
			if (modsElem != null)
				ParseSymbols(project.FeatureSystem, modsElem, project.Segmenter.Modifiers);

			XElement bdrysElem = segmentationElem.Element("Boundaries");
			if (bdrysElem != null)
				ParseSymbols(project.FeatureSystem, bdrysElem, project.Segmenter.Boundaries);

			XElement tonesElem = segmentationElem.Element("ToneLetters");
			if (tonesElem != null)
				ParseSymbols(project.FeatureSystem, tonesElem, project.Segmenter.ToneLetters);

			XElement joinersElem = segmentationElem.Element("Joiners");
			if (joinersElem != null)
				ParseSymbols(project.FeatureSystem, joinersElem, project.Segmenter.Joiners);

			XElement alignersElem = root.Element("Aligners");
			Debug.Assert(alignersElem != null);
			foreach (XElement alignerElem in alignersElem.Elements("Aligner"))
				LoadComponent(spanFactory, project, alignerElem, project.Aligners);

			var senses = new Dictionary<string, Sense>();
			XElement sensesElem = root.Element("Senses");
			Debug.Assert(sensesElem != null);
			foreach (XElement senseElem in sensesElem.Elements("Sense"))
			{
				var sense = new Sense((string) senseElem.Attribute("gloss"), (string) senseElem.Attribute("category"));
				senses[(string) senseElem.Attribute("id")] = sense;
				project.Senses.Add(sense);
			}

			XElement varietiesElem = root.Element("Varieties");
			Debug.Assert(varietiesElem != null);
			foreach (XElement varietyElem in varietiesElem.Elements("Variety"))
			{
				var variety = new Variety((string) varietyElem.Attribute("name"));
				XElement wordsElem = varietyElem.Element("Words");
				Debug.Assert(wordsElem != null);
				foreach (XElement wordElem in wordsElem.Elements("Word"))
				{
					Sense sense;
					if (senses.TryGetValue((string) wordElem.Attribute("sense"), out sense))
					{
						var sb = new StringBuilder();
						string prefix = null;
						XElement prefixElem = wordElem.Element("Prefix");
						if (prefixElem != null)
						{
							prefix = ((string) prefixElem).Trim();
							sb.Append(prefix);
						}
						var stem = ((string) wordElem.Element("Stem")).Trim();
						sb.Append(stem);
						string suffix = null;
						XElement suffixElem = wordElem.Element("Suffix");
						if (suffixElem != null)
						{
							suffix = ((string) suffixElem).Trim();
							sb.Append(suffix);
						}
						Shape shape;
						if (!project.Segmenter.ToShape(prefix, stem, suffix, out shape))
							shape = project.Segmenter.EmptyShape;
						variety.Words.Add(new Word(sb.ToString(), shape, sense));
					}
				}
				XElement affixesElem = varietyElem.Element("Affixes");
				Debug.Assert(affixesElem != null);
				foreach (XElement affixElem in affixesElem.Elements("Affix"))
				{
					var type = AffixType.Prefix;
					switch ((string) affixElem.Attribute("type"))
					{
						case "prefix":
							type = AffixType.Prefix;
							break;
						case "suffix":
							type = AffixType.Suffix;
							break;
					}

					var affixStr = ((string) affixElem).Trim();
					Shape shape;
					if (!project.Segmenter.ToShape(affixStr, out shape))
						shape = project.Segmenter.EmptyShape;
					variety.Affixes.Add(new Affix(affixStr, type, shape, (string) affixElem.Attribute("category")));
				}
				project.Varieties.Add(variety);
			}

			XElement projectProcessorsElem = root.Element("ProjectProcessors");
			Debug.Assert(projectProcessorsElem != null);
			foreach (XElement projectProcessorElem in projectProcessorsElem.Elements("ProjectProcessor"))
				LoadComponent(spanFactory, project, projectProcessorElem, project.ProjectProcessors);

			XElement varietyProcessorsElem = root.Element("VarietyProcessors");
			Debug.Assert(varietyProcessorsElem != null);
			foreach (XElement varietyProcessorElem in varietyProcessorsElem.Elements("VarietyProcessor"))
				LoadComponent(spanFactory, project, varietyProcessorElem, project.VarietyProcessors);

			XElement varietyPairProcessorsElem = root.Element("VarietyPairProcessors");
			Debug.Assert(varietyPairProcessorsElem != null);
			foreach (XElement varietyPairProcessorElem in varietyPairProcessorsElem.Elements("VarietyPairProcessor"))
				LoadComponent(spanFactory, project, varietyPairProcessorElem, project.VarietyPairProcessors);

			return project;
		}

		private static void LoadComponent<T>(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem, IDictionary<string, T> components)
		{
			var typeStr = (string) elem.Attribute("type");
			Type type = Type.GetType(string.Format("SIL.Cog.Config.{0}Config", typeStr));
			Debug.Assert(type != null);
			var config = (IComponentConfig<T>) Activator.CreateInstance(type);
			var id = (string) elem.Attribute("id");
			components[id] = config.Load(spanFactory, project, elem);
		}

		private static void ParseSymbols(FeatureSystem featSys, XElement elem, ICollection<Symbol> symbols)
		{
			foreach (XElement symbolElem in elem.Elements("Symbol"))
			{
				FeatureStruct fs = LoadFeatureStruct(featSys, symbolElem);
				var strRep = (string) symbolElem.Attribute("strRep");
				symbols.Add(new Symbol(strRep, fs, ((string) symbolElem.Attribute("overwrite")) != "false"));
			}
		}

		public static FeatureStruct LoadFeatureStruct(FeatureSystem featSys, XElement elem)
		{
			var fs = new FeatureStruct();
			foreach (XElement featureValueElem in elem.Elements("FeatureValue"))
			{
				var feature = (SymbolicFeature) featSys.GetFeature((string)featureValueElem.Attribute("feature"));
				fs.AddValue(feature, ((string)featureValueElem.Attribute("value")).Split(' ').Select(featSys.GetSymbol));
			}
			return fs;
		}

		public static void Save(CogProject project, string configFilePath)
		{
			var root = new XElement("CogProject",
				new XElement("FeatureSystem", project.FeatureSystem.Cast<SymbolicFeature>().Select(feature => new XElement("Feature", new XAttribute("id", feature.ID), new XAttribute("name", feature.Description),
					feature.PossibleSymbols.Select(symbol => new XElement("Value", new XAttribute("id", symbol.ID), new XAttribute("name", symbol.Description)))))),
				new XElement("Segmentation",
					new XElement("Vowels", new XAttribute("maxLength", project.Segmenter.MaxVowelLength), SaveSymbols(project.Segmenter.Vowels)),
					new XElement("Consonants", new XAttribute("maxLength", project.Segmenter.MaxConsonantLength), SaveSymbols(project.Segmenter.Consonants)),
					new XElement("Modifiers", SaveSymbols(project.Segmenter.Modifiers)),
					new XElement("Boundaries", SaveSymbols(project.Segmenter.Boundaries)),
					new XElement("ToneLetters", SaveSymbols(project.Segmenter.ToneLetters)),
					new XElement("Joiners", SaveSymbols(project.Segmenter.Joiners))));

			root.Add(new XElement("Aligners", project.Aligners.Select(kvp => SaveComponent("Aligner", kvp.Key, kvp.Value))));

			var senseIds = new Dictionary<Sense, string>();
			var sensesElem = new XElement("Senses");
			int i = 1;
			foreach (Sense sense in project.Senses)
			{
				string senseId = "sense" + i++;
				var senseElem = new XElement("Sense", new XAttribute("id", senseId), new XAttribute("gloss", sense.Gloss));
				if (!string.IsNullOrEmpty(sense.Category))
					senseElem.Add(new XAttribute("category", sense.Category));
				sensesElem.Add(senseElem);
				senseIds[sense] = senseId;
			}
			root.Add(sensesElem);

			root.Add(new XElement("Varieties",
				project.Varieties.Select(variety => new XElement("Variety", new XAttribute("name", variety.Name),
					new XElement("Words", variety.Words.Select(word => SaveWord(word, senseIds[word.Sense]))),
					new XElement("Affixes", variety.Affixes.Select(SaveAffix))))));

			root.Add(new XElement("ProjectProcessors", project.ProjectProcessors.Select(kvp => SaveComponent("ProjectProcessor", kvp.Key, kvp.Value))));
			root.Add(new XElement("VarietyProcessors", project.VarietyProcessors.Select(kvp => SaveComponent("VarietyProcessor", kvp.Key, kvp.Value))));
			root.Add(new XElement("VarietyPairProcessors", project.VarietyPairProcessors.Select(kvp => SaveComponent("VarietyPairProcessor", kvp.Key, kvp.Value))));

			root.Save(configFilePath);
		}

		private static IEnumerable<XElement> SaveSymbols(IEnumerable<Symbol> symbols)
		{
			return symbols.Select(symbol => new XElement("Symbol", new XAttribute("strRep", symbol.StrRep),
				CreateFeatureStruct(symbol.FeatureStruct)));
		}

		private static XElement SaveWord(Word word, string senseId)
		{
			var wordElem = new XElement("Word", new XAttribute("sense", senseId));
			if (word.Shape.Count > 0)
			{
				Annotation<ShapeNode> prefix = word.Prefix;
				if (prefix != null)
					wordElem.Add(new XElement("Prefix", prefix.OriginalStrRep()));
				wordElem.Add(new XElement("Stem", word.Stem.OriginalStrRep()));
				Annotation<ShapeNode> suffix = word.Suffix;
				if (suffix != null)
					wordElem.Add(new XElement("Suffix", suffix.OriginalStrRep()));
			}
			else
			{
				wordElem.Add(new XElement("Stem", word.StrRep));
			}
			return wordElem;
		}

		private static XElement SaveAffix(Affix affix)
		{
			string typeStr = null;
			switch (affix.Type)
			{
				case AffixType.Prefix:
					typeStr = "prefix";
					break;
				case AffixType.Suffix:
					typeStr = "suffix";
					break;
			}
			Debug.Assert(typeStr != null);
			var affixElem = new XElement("Affix", new XAttribute("type", typeStr));
			if (!string.IsNullOrEmpty(affix.Category))
				affixElem.Add(new XAttribute("category", affix.Category));
			affixElem.Add(affix.StrRep);
			return affixElem;
		}

		public static IEnumerable<XElement> CreateFeatureStruct(FeatureStruct fs)
		{
			return from feature in fs.Features.Cast<SymbolicFeature>()
				   where feature != CogFeatureSystem.Type
				   select new XElement("FeatureValue", new XAttribute("feature", feature.ID), new XAttribute("value", ((FeatureSymbol) fs.GetValue(feature)).ID));
		}

		private static XElement SaveComponent<T>(string elemName, string id, T component)
		{
			var elem = new XElement(elemName, new XAttribute("id", id), new XAttribute("type", component.GetType().Name));
			Type type = Type.GetType(string.Format("SIL.Cog.Config.{0}Config", component.GetType().Name));
			Debug.Assert(type != null);
			var config = (IComponentConfig<T>) Activator.CreateInstance(type);
			config.Save(component, elem);
			return elem;
		}
	}
}
