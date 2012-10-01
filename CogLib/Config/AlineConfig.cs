using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SIL.Cog.Aligners;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Config
{
	public class AlineConfig : AlignerConfig
	{
		public override IAligner Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			AlignerSettings settings = LoadSettings(project.FeatureSystem, elem);
			XElement relevantFeaturesElem = elem.Element("RelevantFeatures");
			Debug.Assert(relevantFeaturesElem != null);
			var relevantVowelFeatures = new IDBearerSet<SymbolicFeature>(GetFeatures(project.FeatureSystem, (string) relevantFeaturesElem.Attribute("vowelFeatures")));
			var relevantConsFeatures = new IDBearerSet<SymbolicFeature>(GetFeatures(project.FeatureSystem, (string) relevantFeaturesElem.Attribute("consonantFeatures")));

			return new Aline(spanFactory, relevantVowelFeatures, relevantConsFeatures, settings);
		}

		private IEnumerable<SymbolicFeature> GetFeatures(FeatureSystem featSys, string featuresStr)
		{
			return featuresStr.Split(' ').Select(id => (SymbolicFeature) featSys.GetFeature(id));
		}

		public override void Save(IAligner component, XElement elem)
		{
			var aline = (Aline) component;
			SaveSettings(aline.Settings, elem);
			elem.Add(new XElement("RelevantFeatures", new XAttribute("vowelFeatures", GetFeaturesString(aline.RelevantVowelFeatures)),
				new XAttribute("consonantFeatures", GetFeaturesString(aline.RelevantConsonantFeatures))));
		}

		private string GetFeaturesString(IEnumerable<SymbolicFeature> features)
		{
			var sb = new StringBuilder();
			bool first = true;
			foreach (SymbolicFeature feature in features)
			{
				if (!first)
					sb.Append(' ');
				sb.Append(feature.ID);
				first = false;
			}
			return sb.ToString();
		}
	}
}
