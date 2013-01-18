using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Config
{
	public static class ConfigManager
	{
		private class ResourceXmlResolver : XmlResolver
		{
			public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
			{
				return GetType().Assembly.GetManifestResourceStream(string.Format("SIL.Cog.Config.{0}", Path.GetFileName(absoluteUri.ToString())));
			}

			public override ICredentials Credentials
			{
				set { throw new NotImplementedException(); }
			}
		}

		private const string DefaultNamespace = "http://www.sil.org/CogProject";
		internal static readonly XNamespace Cog = DefaultNamespace;
		private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		private static readonly XNamespace Xsi = XsiNamespace;
		private static readonly XmlSchemaSet Schema;

		static ConfigManager()
		{
			Schema = new XmlSchemaSet {XmlResolver = new ResourceXmlResolver()};
			Schema.Add(DefaultNamespace, "CogProject.xsd");
			Schema.Compile();
		}

		public static CogProject Load(SpanFactory<ShapeNode> spanFactory, Stream configFileStream)
		{
			XDocument doc;
			try
			{
				doc = XDocument.Load(configFileStream);
			}
			catch (XmlException xe)
			{
				throw new ConfigException("The specified file is not a valid Cog config file", xe);
			}

			return LoadProject(spanFactory, doc);
		}

		public static CogProject Load(SpanFactory<ShapeNode> spanFactory, string configFilePath)
		{
			XDocument doc;
			try
			{
				doc = XDocument.Load(configFilePath);
			}
			catch (XmlException xe)
			{
				throw new ConfigException("The specified file is not a valid Cog config file", xe);
			}

			return LoadProject(spanFactory, doc);
		}

		private static CogProject LoadProject(SpanFactory<ShapeNode> spanFactory, XDocument doc)
		{
			var project = new CogProject(spanFactory);

			XElement root = doc.Root;
			Debug.Assert(root != null);
			if (root.GetDefaultNamespace() != DefaultNamespace)
				throw new ConfigException("The specified file is not a valid Cog config file");
			doc.Validate(Schema, (sender, args) =>
				{
					switch (args.Severity)
					{
						case XmlSeverityType.Error:
							throw new ConfigException("The specified file is not a valid Cog config file", args.Exception);
					}
				});
			var featSys = new FeatureSystem();
			XElement featSysElem = root.Element(Cog + "FeatureSystem");
			Debug.Assert(featSysElem != null);
			foreach (XElement featureElem in featSysElem.Elements(Cog + "Feature"))
			{
				var feat = new SymbolicFeature((string) featureElem.Attribute("id"), featureElem.Elements(Cog + "Value")
					.Select(e => new FeatureSymbol((string) e.Attribute("id"), (string) e.Attribute("name")))) {Description = (string) featureElem.Attribute("name")};
				featSys.Add(feat);
			}
			featSys.Freeze();
			project.FeatureSystem = featSys;

			XElement segmentationElem = root.Element(Cog + "Segmentation");
			Debug.Assert(segmentationElem != null);
			XElement vowelsElem = segmentationElem.Element(Cog + "Vowels");
			if (vowelsElem != null)
			{
				var maxLenStr = (string) vowelsElem.Attribute("maxLength");
				if (!string.IsNullOrEmpty(maxLenStr))
					project.Segmenter.MaxVowelLength = int.Parse(maxLenStr);

				ParseSymbols(project.FeatureSystem, vowelsElem, project.Segmenter.Vowels);
			}

			XElement consElem = segmentationElem.Element(Cog + "Consonants");
			if (consElem != null)
			{
				var maxLenStr = (string) consElem.Attribute("maxLength");
				if (!string.IsNullOrEmpty(maxLenStr))
					project.Segmenter.MaxConsonantLength = int.Parse(maxLenStr);

				ParseSymbols(project.FeatureSystem, consElem, project.Segmenter.Consonants);
			}

			XElement modsElem = segmentationElem.Element(Cog + "Modifiers");
			if (modsElem != null)
				ParseSymbols(project.FeatureSystem, modsElem, project.Segmenter.Modifiers);

			XElement bdrysElem = segmentationElem.Element(Cog + "Boundaries");
			if (bdrysElem != null)
				ParseSymbols(project.FeatureSystem, bdrysElem, project.Segmenter.Boundaries);

			XElement tonesElem = segmentationElem.Element(Cog + "ToneLetters");
			if (tonesElem != null)
				ParseSymbols(project.FeatureSystem, tonesElem, project.Segmenter.ToneLetters);

			XElement joinersElem = segmentationElem.Element(Cog + "Joiners");
			if (joinersElem != null)
				ParseSymbols(project.FeatureSystem, joinersElem, project.Segmenter.Joiners);

			XElement alignersElem = root.Element(Cog + "Aligners");
			Debug.Assert(alignersElem != null);
			foreach (XElement alignerElem in alignersElem.Elements(Cog + "Aligner"))
				LoadComponent(spanFactory, project, alignerElem, project.Aligners);

			var senses = new Dictionary<string, Sense>();
			XElement sensesElem = root.Element(Cog + "Senses");
			Debug.Assert(sensesElem != null);
			foreach (XElement senseElem in sensesElem.Elements(Cog + "Sense"))
			{
				var sense = new Sense((string) senseElem.Attribute("gloss"), (string) senseElem.Attribute("category"));
				senses[(string) senseElem.Attribute("id")] = sense;
				project.Senses.Add(sense);
			}

			XElement varietiesElem = root.Element(Cog + "Varieties");
			Debug.Assert(varietiesElem != null);
			foreach (XElement varietyElem in varietiesElem.Elements(Cog + "Variety"))
			{
				var variety = new Variety((string) varietyElem.Attribute("name"));
				XElement wordsElem = varietyElem.Element(Cog + "Words");
				Debug.Assert(wordsElem != null);
				foreach (XElement wordElem in wordsElem.Elements(Cog + "Word"))
				{
					Sense sense;
					if (senses.TryGetValue((string) wordElem.Attribute("sense"), out sense))
					{
						var sb = new StringBuilder();
						string prefix = null;
						XElement prefixElem = wordElem.Element(Cog + "Prefix");
						if (prefixElem != null)
						{
							prefix = ((string) prefixElem).Trim();
							sb.Append(prefix);
						}
						var stem = ((string) wordElem.Element(Cog + "Stem")).Trim();
						sb.Append(stem);
						string suffix = null;
						XElement suffixElem = wordElem.Element(Cog + "Suffix");
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
				XElement affixesElem = varietyElem.Element(Cog + "Affixes");
				Debug.Assert(affixesElem != null);
				foreach (XElement affixElem in affixesElem.Elements(Cog + "Affix"))
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

			XElement projectProcessorsElem = root.Element(Cog + "ProjectProcessors");
			Debug.Assert(projectProcessorsElem != null);
			foreach (XElement projectProcessorElem in projectProcessorsElem.Elements(Cog + "ProjectProcessor"))
				LoadComponent(spanFactory, project, projectProcessorElem, project.ProjectProcessors);

			XElement varietyProcessorsElem = root.Element(Cog + "VarietyProcessors");
			Debug.Assert(varietyProcessorsElem != null);
			foreach (XElement varietyProcessorElem in varietyProcessorsElem.Elements(Cog + "VarietyProcessor"))
				LoadComponent(spanFactory, project, varietyProcessorElem, project.VarietyProcessors);

			XElement varietyPairProcessorsElem = root.Element(Cog + "VarietyPairProcessors");
			Debug.Assert(varietyPairProcessorsElem != null);
			foreach (XElement varietyPairProcessorElem in varietyPairProcessorsElem.Elements(Cog + "VarietyPairProcessor"))
				LoadComponent(spanFactory, project, varietyPairProcessorElem, project.VarietyPairProcessors);

			return project;
		}

		private static void LoadComponent<T>(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem, IDictionary<string, T> components)
		{
			var typeStr = (string) elem.Attribute(Xsi + "type");
			Type type = Type.GetType(string.Format("SIL.Cog.Config.{0}Config", typeStr));
			Debug.Assert(type != null);
			var config = (IComponentConfig<T>) Activator.CreateInstance(type);
			var id = (string) elem.Attribute("id");
			components[id] = config.Load(spanFactory, project, elem);
		}

		private static void ParseSymbols(FeatureSystem featSys, XElement elem, ICollection<Symbol> symbols)
		{
			foreach (XElement symbolElem in elem.Elements(Cog + "Symbol"))
			{
				FeatureStruct fs = LoadFeatureStruct(featSys, symbolElem);
				var strRep = (string) symbolElem.Attribute("strRep");
				symbols.Add(new Symbol(strRep, fs, ((string) symbolElem.Attribute("overwrite")) != "false"));
			}
		}

		public static FeatureStruct LoadFeatureStruct(FeatureSystem featSys, XElement elem)
		{
			var fs = new FeatureStruct();
			foreach (XElement featureValueElem in elem.Elements(Cog + "FeatureValue"))
			{
				var feature = (SymbolicFeature) featSys.GetFeature((string)featureValueElem.Attribute("feature"));
				fs.AddValue(feature, ((string)featureValueElem.Attribute("value")).Split(' ').Select(featSys.GetSymbol));
			}
			return fs;
		}

		public static void Save(CogProject project, string configFilePath)
		{
			var root = new XElement(Cog + "CogProject", new XAttribute("xmlns", DefaultNamespace), new XAttribute(XNamespace.Xmlns + "xsi", XsiNamespace),
				new XElement(Cog + "FeatureSystem", project.FeatureSystem.Cast<SymbolicFeature>().Select(feature => new XElement(Cog + "Feature", new XAttribute("id", feature.ID), new XAttribute("name", feature.Description),
					feature.PossibleSymbols.Select(symbol => new XElement(Cog + "Value", new XAttribute("id", symbol.ID), new XAttribute("name", symbol.Description)))))),
				new XElement(Cog + "Segmentation",
					new XElement(Cog + "Vowels", new XAttribute("maxLength", project.Segmenter.MaxVowelLength), SaveSymbols(project.Segmenter.Vowels)),
					new XElement(Cog + "Consonants", new XAttribute("maxLength", project.Segmenter.MaxConsonantLength), SaveSymbols(project.Segmenter.Consonants)),
					new XElement(Cog + "Modifiers", SaveSymbols(project.Segmenter.Modifiers)),
					new XElement(Cog + "Boundaries", SaveSymbols(project.Segmenter.Boundaries)),
					new XElement(Cog + "ToneLetters", SaveSymbols(project.Segmenter.ToneLetters)),
					new XElement(Cog + "Joiners", SaveSymbols(project.Segmenter.Joiners))));

			root.Add(new XElement(Cog + "Aligners", project.Aligners.Select(kvp => SaveComponent("Aligner", kvp.Key, kvp.Value))));

			var senseIds = new Dictionary<Sense, string>();
			var sensesElem = new XElement(Cog + "Senses");
			int i = 1;
			foreach (Sense sense in project.Senses)
			{
				string senseId = "sense" + i++;
				var senseElem = new XElement(Cog + "Sense", new XAttribute("id", senseId), new XAttribute("gloss", sense.Gloss));
				if (!string.IsNullOrEmpty(sense.Category))
					senseElem.Add(new XAttribute("category", sense.Category));
				sensesElem.Add(senseElem);
				senseIds[sense] = senseId;
			}
			root.Add(sensesElem);

			root.Add(new XElement(Cog + "Varieties",
				project.Varieties.Select(variety => new XElement(Cog + "Variety", new XAttribute("name", variety.Name),
					new XElement(Cog + "Words", variety.Words.Select(word => SaveWord(word, senseIds[word.Sense]))),
					new XElement(Cog + "Affixes", variety.Affixes.Select(SaveAffix))))));

			root.Add(new XElement(Cog + "ProjectProcessors", project.ProjectProcessors.Select(kvp => SaveComponent("ProjectProcessor", kvp.Key, kvp.Value))));
			root.Add(new XElement(Cog + "VarietyProcessors", project.VarietyProcessors.Select(kvp => SaveComponent("VarietyProcessor", kvp.Key, kvp.Value))));
			root.Add(new XElement(Cog + "VarietyPairProcessors", project.VarietyPairProcessors.Select(kvp => SaveComponent("VarietyPairProcessor", kvp.Key, kvp.Value))));

			root.Save(configFilePath);
		}

		private static IEnumerable<XElement> SaveSymbols(IEnumerable<Symbol> symbols)
		{
			return symbols.Select(symbol => new XElement(Cog + "Symbol", new XAttribute("strRep", symbol.StrRep),
				CreateFeatureStruct(symbol.FeatureStruct)));
		}

		private static XElement SaveWord(Word word, string senseId)
		{
			var wordElem = new XElement(Cog + "Word", new XAttribute("sense", senseId));
			if (word.Shape.Count > 0)
			{
				Annotation<ShapeNode> prefix = word.Prefix;
				if (prefix != null)
					wordElem.Add(new XElement(Cog + "Prefix", prefix.OriginalStrRep()));
				wordElem.Add(new XElement(Cog + "Stem", word.Stem.OriginalStrRep()));
				Annotation<ShapeNode> suffix = word.Suffix;
				if (suffix != null)
					wordElem.Add(new XElement(Cog + "Suffix", suffix.OriginalStrRep()));
			}
			else
			{
				wordElem.Add(new XElement(Cog + "Stem", word.StrRep));
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
			var affixElem = new XElement(Cog + "Affix", new XAttribute("type", typeStr));
			if (!string.IsNullOrEmpty(affix.Category))
				affixElem.Add(new XAttribute("category", affix.Category));
			affixElem.Add(affix.StrRep);
			return affixElem;
		}

		internal static IEnumerable<XElement> CreateFeatureStruct(FeatureStruct fs)
		{
			return from feature in fs.Features.Cast<SymbolicFeature>()
				   where feature != CogFeatureSystem.Type
				   select new XElement(Cog + "FeatureValue", new XAttribute("feature", feature.ID), new XAttribute("value", ((FeatureSymbol) fs.GetValue(feature)).ID));
		}

		private static XElement SaveComponent<T>(string elemName, string id, T component)
		{
			var elem = new XElement(Cog + elemName, new XAttribute("id", id), new XAttribute(Xsi + "type", component.GetType().Name));
			Type type = Type.GetType(string.Format("SIL.Cog.Config.{0}Config", component.GetType().Name));
			Debug.Assert(type != null);
			var config = (IComponentConfig<T>) Activator.CreateInstance(type);
			config.Save(component, elem);
			return elem;
		}
	}
}
