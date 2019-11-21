using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using SIL.Machine.FeatureModel;
using SIL.Machine.Morphology;

namespace SIL.Cog.Domain.Config
{
	public static class ConfigManager
	{
		private class ResourceXmlResolver : XmlResolver
		{
			public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
			{
				return GetType().Assembly.GetManifestResourceStream(string.Format("SIL.Cog.Domain.Config.{0}", Path.GetFileName(absoluteUri.ToString())));
			}

			public override ICredentials Credentials
			{
				set { throw new NotImplementedException(); }
			}
		}

		private const string DefaultNamespace = "http://www.sil.org/CogProject/1.3";
		internal static readonly XNamespace Cog = DefaultNamespace;
		private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		internal static readonly XNamespace Xsi = XsiNamespace;
		private static readonly XmlSchemaSet Schema;
		private static readonly Dictionary<XNamespace, IConfigMigration> ConfigMigrations;

		static ConfigManager()
		{
			Schema = new XmlSchemaSet {XmlResolver = new ResourceXmlResolver()};
			if (!Platform.IsMono)  // Bypass bug in old versions of Mono: in Mono 2.10.8, ResourceXmlResolver incorrectly searches the filesystem. 2015-09 RM
			{
				Schema.Add(DefaultNamespace, "CogProject.xsd");
			}
			Schema.Compile();

			ConfigMigrations = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.GetInterfaces().Contains(typeof(IConfigMigration)))
				.Select(t => (IConfigMigration) Activator.CreateInstance(t)).ToDictionary(cm => cm.FromNamespace);
		}

		public static CogProject Load(SegmentPool segmentPool, Stream configFileStream)
		{
			CogProject project;
			Load(segmentPool, configFileStream, out project);
			return project;
		}

		public static bool Load(SegmentPool segmentPool, Stream configFileStream, out CogProject project)
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

			return LoadProject(segmentPool, doc, out project);
		}

		public static CogProject Load(SegmentPool segmentPool, string configFilePath)
		{
			CogProject project;
			Load(segmentPool, configFilePath, out project);
			return project;
		}

		public static bool Load(SegmentPool segmentPool, string configFilePath, out CogProject project)
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

			return LoadProject(segmentPool, doc, out project);
		}

		public static CogProject LoadFromXmlString(SegmentPool segmentPool, string xmlString)
		{
			CogProject project;
			LoadFromXmlString(segmentPool, xmlString, out project);
			return project;
		}

		public static bool LoadFromXmlString(SegmentPool segmentPool, string xmlString, out CogProject project)
		{
			XDocument doc;
			try
			{
				doc = XDocument.Parse(xmlString);
			}
			catch (XmlException xe)
			{
				throw new ConfigException("The specified file is not a valid Cog config file", xe);
			}

			return LoadProject(segmentPool, doc, out project);
		}

		private static bool LoadProject(SegmentPool segmentPool, XDocument doc, out CogProject project)
		{
			project = new CogProject();

			XElement root = doc.Root;
			Debug.Assert(root != null);

			bool migrated = false;
			while (root.GetDefaultNamespace() != Cog)
			{
				IConfigMigration cm;
				if (ConfigMigrations.TryGetValue(root.GetDefaultNamespace(), out cm))
				{
					cm.Migrate(doc);
					migrated = true;
				}
				else
					throw new ConfigException("The specified file is not a valid Cog config file");
			}

			if (!Platform.IsMono)  // Bypass bug in Mono XSD validation. 2015-08 RM
			{
				doc.Validate(Schema, (sender, args) =>
					{
						switch (args.Severity)
						{
							case XmlSeverityType.Error:
								throw new ConfigException("The specified file is not a valid Cog config file", args.Exception);
						}
					});
			}
			segmentPool.Reset();
			project.Version = (int) root.Attribute("version");
			var featSys = new FeatureSystem();
			XElement featSysElem = root.Element(Cog + "FeatureSystem");
			Debug.Assert(featSysElem != null);
			foreach (XElement featureElem in featSysElem.Elements(Cog + "Feature"))
			{
				var feat = new SymbolicFeature((string) featureElem.Attribute("id"), featureElem.Elements(Cog + "Value")
					.Select(e => new FeatureSymbol((string) e.Attribute("id"), (string) e.Attribute("name")))) {Description = (string) featureElem.Attribute("name")};
				featSys.Add(feat);
			}
			project.FeatureSystem = featSys;

			XElement segmentationElem = root.Element(Cog + "Segmentation");
			Debug.Assert(segmentationElem != null);
			XElement vowelsElem = segmentationElem.Element(Cog + "Vowels");
			if (vowelsElem != null)
			{
				XAttribute maxLenAttr = vowelsElem.Attribute("maxLength");
				if (maxLenAttr != null)
					project.Segmenter.MaxVowelLength = (int) maxLenAttr;

				ParseSymbols(project.FeatureSystem, vowelsElem, project.Segmenter.Vowels);
			}

			XElement consElem = segmentationElem.Element(Cog + "Consonants");
			if (consElem != null)
			{
				XAttribute maxLenAttr = consElem.Attribute("maxLength");
				if (maxLenAttr != null)
					project.Segmenter.MaxConsonantLength = (int) maxLenAttr;

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

			XElement alignersElem = root.Element(Cog + "WordAligners");
			Debug.Assert(alignersElem != null);
			foreach (XElement alignerElem in alignersElem.Elements(Cog + "WordAligner"))
				LoadComponent(segmentPool, project, alignerElem, project.WordAligners);

			XElement cognateIdentifiersElem = root.Element(Cog + "CognateIdentifiers");
			Debug.Assert(cognateIdentifiersElem != null);
			foreach (XElement cognateIdentifierElem in cognateIdentifiersElem.Elements(Cog + "CognateIdentifier"))
				LoadComponent(segmentPool, project, cognateIdentifierElem, project.CognateIdentifiers);

			XElement meaningsElem = root.Element(Cog + "Meanings");
			Debug.Assert(meaningsElem != null);
			foreach (XElement meaningElem in meaningsElem.Elements(Cog + "Meaning"))
			{
				var meaning = new Meaning((string) meaningElem.Attribute("gloss"), (string) meaningElem.Attribute("category"));
				project.Meanings.Add(meaning);
			}

			XElement varietiesElem = root.Element(Cog + "Varieties");
			Debug.Assert(varietiesElem != null);
			foreach (XElement varietyElem in varietiesElem.Elements(Cog + "Variety"))
			{
				var variety = new Variety((string) varietyElem.Attribute("name"));
				XElement wordsElem = varietyElem.Element(Cog + "Words");
				if (wordsElem != null)
				{
					foreach (XElement wordElem in wordsElem.Elements(Cog + "Word"))
					{
						Meaning meaning = project.Meanings[(string) wordElem.Attribute("meaning")];
						var strRep = ((string) wordElem).Trim();
						var stemIndex = (int?) wordElem.Attribute("stemIndex") ?? 0;
						var stemLen = (int?) wordElem.Attribute("stemLength") ?? strRep.Length - stemIndex;
						variety.Words.Add(new Word(strRep, stemIndex, stemLen, meaning));
					}
				}
				XElement affixesElem = varietyElem.Element(Cog + "Affixes");
				if (affixesElem != null)
				{
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
						variety.Affixes.Add(new Affix(affixStr, type, (string) affixElem.Attribute("category")));
					}
				}
				XElement regionsElem = varietyElem.Element(Cog + "Regions");
				if (regionsElem != null)
				{
					foreach (XElement regionElem in regionsElem.Elements(Cog + "Region"))
					{
						var region = new GeographicRegion {Description = (string) regionElem.Element(Cog + "Description")};
						foreach (XElement coordinateElem in regionElem.Elements(Cog + "Coordinates").Elements(Cog + "Coordinate"))
						{
							var latitude = (double) coordinateElem.Element(Cog + "Latitude");
							var longitude = (double) coordinateElem.Element(Cog + "Longitude");
							region.Coordinates.Add(new GeographicCoordinate(latitude, longitude));
						}
						variety.Regions.Add(region);
					}
				}
				project.Varieties.Add(variety);
			}

			XElement cognacyDecisionsElem = root.Element(Cog + "CognacyDecisions");
			Debug.Assert(cognacyDecisionsElem != null);
			foreach (XElement cognacyDecisionElem in cognacyDecisionsElem.Elements(Cog + "CognacyDecision"))
			{
				project.CognacyDecisions.Add(new CognacyDecision(project.Varieties[(string) cognacyDecisionElem.Attribute("variety1")],
					project.Varieties[(string) cognacyDecisionElem.Attribute("variety2")], project.Meanings[(string) cognacyDecisionElem.Attribute("meaning")],
					(bool) cognacyDecisionElem.Attribute("cognacy")));
			}

			XElement projectProcessorsElem = root.Element(Cog + "ProjectProcessors");
			Debug.Assert(projectProcessorsElem != null);
			foreach (XElement projectProcessorElem in projectProcessorsElem.Elements(Cog + "ProjectProcessor"))
				LoadComponent(segmentPool, project, projectProcessorElem, project.ProjectProcessors);

			XElement varietyProcessorsElem = root.Element(Cog + "VarietyProcessors");
			Debug.Assert(varietyProcessorsElem != null);
			foreach (XElement varietyProcessorElem in varietyProcessorsElem.Elements(Cog + "VarietyProcessor"))
				LoadComponent(segmentPool, project, varietyProcessorElem, project.VarietyProcessors);

			XElement varietyPairProcessorsElem = root.Element(Cog + "VarietyPairProcessors");
			Debug.Assert(varietyPairProcessorsElem != null);
			foreach (XElement varietyPairProcessorElem in varietyPairProcessorsElem.Elements(Cog + "VarietyPairProcessor"))
				LoadComponent(segmentPool, project, varietyPairProcessorElem, project.VarietyPairProcessors);

			return migrated;
		}

		internal static T LoadComponent<T>(SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var typeStr = (string) elem.Attribute(Xsi + "type");
			Type type = Type.GetType(string.Format("SIL.Cog.Domain.Config.Components.{0}Config", typeStr));
			Debug.Assert(type != null);
			var config = (IComponentConfig<T>) Activator.CreateInstance(type);
			return config.Load(segmentPool, project, elem);
		}

		private static void LoadComponent<T>(SegmentPool segmentPool, CogProject project, XElement elem, IDictionary<string, T> components)
		{
			var id = (string) elem.Attribute("id");
			components[id] = LoadComponent<T>(segmentPool, project, elem);
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

		private static FeatureStruct LoadFeatureStruct(FeatureSystem featSys, XElement elem)
		{
			var fs = new FeatureStruct();
			foreach (XElement featureValueElem in elem.Elements(Cog + "FeatureValue"))
			{
				var feature = (SymbolicFeature) featSys.GetFeature((string)featureValueElem.Attribute("feature"));
				fs.AddValue(feature, ((string)featureValueElem.Attribute("value")).Split(' ').Select(featSys.GetSymbol));
			}
			return fs;
		}

		private static XElement SaveProject(CogProject project)
		{
			var root = new XElement(Cog + "CogProject", new XAttribute("xmlns", DefaultNamespace), new XAttribute(XNamespace.Xmlns + "xsi", XsiNamespace), new XAttribute("version", project.Version),
				new XElement(Cog + "FeatureSystem", project.FeatureSystem.Cast<SymbolicFeature>().Select(feature => new XElement(Cog + "Feature", new XAttribute("id", feature.ID), new XAttribute("name", feature.Description),
					feature.PossibleSymbols.Select(symbol => new XElement(Cog + "Value", new XAttribute("id", symbol.ID), new XAttribute("name", symbol.Description)))))),
				new XElement(Cog + "Segmentation",
					new XElement(Cog + "Vowels", new XAttribute("maxLength", project.Segmenter.MaxVowelLength), SaveSymbols(project.Segmenter.Vowels)),
					new XElement(Cog + "Consonants", new XAttribute("maxLength", project.Segmenter.MaxConsonantLength), SaveSymbols(project.Segmenter.Consonants)),
					new XElement(Cog + "Modifiers", SaveSymbols(project.Segmenter.Modifiers)),
					new XElement(Cog + "Boundaries", SaveSymbols(project.Segmenter.Boundaries)),
					new XElement(Cog + "ToneLetters", SaveSymbols(project.Segmenter.ToneLetters)),
					new XElement(Cog + "Joiners", SaveSymbols(project.Segmenter.Joiners))));

			root.Add(new XElement(Cog + "WordAligners", project.WordAligners.Select(kvp => SaveComponent("WordAligner", kvp.Key, kvp.Value))));
			root.Add(new XElement(Cog + "CognateIdentifiers", project.CognateIdentifiers.Select(kvp => SaveComponent("CognateIdentifier", kvp.Key, kvp.Value))));

			var meaningsElem = new XElement(Cog + "Meanings");
			foreach (Meaning meaning in project.Meanings)
			{
				var meaningElem = new XElement(Cog + "Meaning", new XAttribute("gloss", meaning.Gloss));
				if (!string.IsNullOrEmpty(meaning.Category))
					meaningElem.Add(new XAttribute("category", meaning.Category));
				meaningsElem.Add(meaningElem);
			}
			root.Add(meaningsElem);

			root.Add(new XElement(Cog + "Varieties",
				project.Varieties.Select(SaveVariety)));

			root.Add(new XElement(Cog + "CognacyDecisions", project.CognacyDecisions.Select(cd =>
				new XElement(Cog + "CognacyDecision",
					new XAttribute("variety1", cd.Variety1.Name),
					new XAttribute("variety2", cd.Variety2.Name),
					new XAttribute("meaning", cd.Meaning.Gloss),
					new XAttribute("cognacy", cd.Cognacy)))));
			root.Add(new XElement(Cog + "ProjectProcessors", project.ProjectProcessors.Select(kvp => SaveComponent("ProjectProcessor", kvp.Key, kvp.Value))));
			root.Add(new XElement(Cog + "VarietyProcessors", project.VarietyProcessors.Select(kvp => SaveComponent("VarietyProcessor", kvp.Key, kvp.Value))));
			root.Add(new XElement(Cog + "VarietyPairProcessors", project.VarietyPairProcessors.Select(kvp => SaveComponent("VarietyPairProcessor", kvp.Key, kvp.Value))));

			return root;
		}

		public static void Save(CogProject project, Stream configFileStream)
		{
			XElement root = SaveProject(project);
			root.Save(configFileStream);
		}

		public static void Save(CogProject project, string configFilePath)
		{
			XElement root = SaveProject(project);
			root.Save(configFilePath);
		}

		private static XElement SaveVariety(Variety variety)
		{
			var varietyElem = new XElement(Cog + "Variety", new XAttribute("name", variety.Name));
			if (variety.Words.Count > 0)
				varietyElem.Add(new XElement(Cog + "Words", variety.Words.Select(word => SaveWord(word, word.Meaning.Gloss))));
			if (variety.Affixes.Count > 0)
				varietyElem.Add(new XElement(Cog + "Affixes", variety.Affixes.Select(SaveAffix)));
			if (variety.Regions.Count > 0)
				varietyElem.Add(new XElement(Cog + "Regions", variety.Regions.Select(SaveRegion)));
			return varietyElem;
		}

		private static IEnumerable<XElement> SaveSymbols(IEnumerable<Symbol> symbols)
		{
			return symbols.Select(symbol => new XElement(Cog + "Symbol", new XAttribute("strRep", symbol.StrRep),
				CreateFeatureStruct(symbol.FeatureStruct)));
		}

		private static XElement SaveWord(Word word, string gloss)
		{
			var wordElem = new XElement(Cog + "Word", new XAttribute("meaning", gloss));
			if (word.StemIndex != 0 || word.StemLength != word.StrRep.Length - word.StemIndex)
			{
				wordElem.Add(new XAttribute("stemIndex", word.StemIndex));
				wordElem.Add(new XAttribute("stemLength", word.StemLength));
			}

			wordElem.Add(word.StrRep);
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

		private static XElement SaveRegion(GeographicRegion region)
		{
			var regionElem = new XElement(Cog + "Region");
			if (!string.IsNullOrEmpty(region.Description))
				regionElem.Add(new XElement(Cog + "Description", region.Description));
			regionElem.Add(new XElement(Cog + "Coordinates",
					region.Coordinates.Select(coord => new XElement(Cog + "Coordinate", new XElement(Cog + "Latitude", coord.Latitude), new XElement(Cog + "Longitude", coord.Longitude)))));
			return regionElem;
		}

		private static IEnumerable<XElement> CreateFeatureStruct(FeatureStruct fs)
		{
			return from feature in fs.Features.Cast<SymbolicFeature>()
				   where feature != CogFeatureSystem.Type
				   select new XElement(Cog + "FeatureValue", new XAttribute("feature", feature.ID), new XAttribute("value", ((FeatureSymbol) fs.GetValue(feature)).ID));
		}

		internal static IEnumerable<SoundClass> LoadSoundClasses(Segmenter segmenter, FeatureSystem featSys, XElement elem)
		{
			foreach (XElement scElem in elem.Elements())
				yield return LoadSoundClass(segmenter, featSys, scElem);
		}

		internal static SoundClass LoadSoundClass(Segmenter segmenter, FeatureSystem featSys, XElement elem)
		{
			var name = (string) elem.Attribute("name");
			if (elem.Name == Cog + "NaturalClass")
			{
				FeatureStruct fs = LoadFeatureStruct(featSys, elem);
				fs.AddValue(CogFeatureSystem.Type, ((string) elem.Attribute("type")) == "vowel" ? CogFeatureSystem.VowelType : CogFeatureSystem.ConsonantType);
				return new NaturalClass(name, fs);
			}
			if (elem.Name == Cog + "UnnaturalClass")
			{
				IEnumerable<string> segments = elem.Elements(Cog + "Segment").Select(segElem => (string) segElem);
				var ignoreModifiers = (bool?) elem.Attribute("ignoreModifiers") ?? false;
				return new UnnaturalClass(name, segments, ignoreModifiers, segmenter);
			}
			return null;
		}

		internal static IEnumerable<XElement> SaveSoundClasses(IEnumerable<SoundClass> soundClasses)
		{
			foreach (SoundClass sc in soundClasses)
				yield return SaveSoundClass(sc);
		}

		internal static XElement SaveSoundClass(SoundClass soundClass)
		{
			var nc = soundClass as NaturalClass;
			if (nc != null)
			{
				return new XElement(Cog + "NaturalClass", new XAttribute("name", nc.Name),
					new XAttribute("type", nc.Type == CogFeatureSystem.VowelType ? "vowel" : "consonant"), CreateFeatureStruct(nc.FeatureStruct));
			}
			var unc = soundClass as UnnaturalClass;
			if (unc != null)
			{
				return new XElement(Cog + "UnnaturalClass", new XAttribute("name", unc.Name), new XAttribute("ignoreModifiers", unc.IgnoreModifiers),
				                    unc.Segments.Select(s => new XElement(Cog + "Segment", s)));
			}
			return null;
		}

		internal static XElement SaveComponent<T>(string elemName, T component)
		{
			var elem = new XElement(Cog + elemName, new XAttribute(Xsi + "type", component.GetType().Name));
			Type type = Type.GetType(string.Format("SIL.Cog.Domain.Config.Components.{0}Config", component.GetType().Name));
			Debug.Assert(type != null);
			var config = (IComponentConfig<T>) Activator.CreateInstance(type);
			config.Save(component, elem);
			return elem;
		}

		private static XElement SaveComponent<T>(string elemName, string id, T component)
		{
			XElement elem = SaveComponent(elemName, component);
			elem.Add(new XAttribute("id", id));
			return elem;
		}
	}
}
