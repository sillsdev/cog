using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using SIL.Cog.Components;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Config.Components
{
	public class AlineConfig : AlignerConfig
	{
		public override IWordPairAligner Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			WordPairAlignerSettings settings = LoadSettings(project.Segmenter, project.FeatureSystem, elem);
			XElement relevantFeaturesElem = elem.Element(ConfigManager.Cog + "RelevantFeatures");
			Debug.Assert(relevantFeaturesElem != null);

			var relevantVowelFeatures = new List<SymbolicFeature>();
			var relevantConsFeatures = new List<SymbolicFeature>();
			var featureWeights = new Dictionary<SymbolicFeature, int>();
			var valueMetrics = new Dictionary<FeatureSymbol, int>();

			foreach (XElement featureElem in relevantFeaturesElem.Elements(ConfigManager.Cog + "RelevantFeature"))
			{
				var feature = project.FeatureSystem.GetFeature<SymbolicFeature>((string) featureElem.Attribute("ref"));
				var vowelStr = (string) featureElem.Attribute("vowel");
				if (vowelStr != null && bool.Parse(vowelStr))
					relevantVowelFeatures.Add(feature);
				var consStr = (string) featureElem.Attribute("consonant");
				if (consStr != null && bool.Parse(consStr))
					relevantConsFeatures.Add(feature);
				featureWeights[feature] = int.Parse((string) featureElem.Attribute("weight"));
				foreach (XElement valueElem in featureElem.Elements(ConfigManager.Cog + "RelevantValue"))
				{
					FeatureSymbol symbol = feature.PossibleSymbols[(string) valueElem.Attribute("ref")];
					valueMetrics[symbol] = int.Parse((string) valueElem.Attribute("metric"));
				}
			}

			return new Aline(relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics, settings);
		}

		public override void Save(IWordPairAligner component, XElement elem)
		{
			var aline = (Aline) component;
			SaveSettings(aline.Settings, elem);
			elem.Add(new XElement(ConfigManager.Cog + "RelevantFeatures", aline.FeatureWeights.Select(kvp =>
				new XElement(ConfigManager.Cog + "RelevantFeature", new XAttribute("ref", kvp.Key.ID), new XAttribute("weight", kvp.Value.ToString(CultureInfo.InvariantCulture)),
					new XAttribute("vowel", aline.RelevantVowelFeatures.Contains(kvp.Key)),
					new XAttribute("consonant", aline.RelevantConsonantFeatures.Contains(kvp.Key)),
					kvp.Key.PossibleSymbols.Select(fs =>
						new XElement(ConfigManager.Cog + "RelevantValue", new XAttribute("ref", fs.ID), new XAttribute("metric", aline.ValueMetrics[fs].ToString(CultureInfo.InvariantCulture))))))));
		}
	}
}
