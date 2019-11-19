using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Domain.Config.Components
{
	public class CognacyWordPairGeneratorConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var initialAlignmentThreshold = (double) elem.Element(ConfigManager.Cog + "InitialAlignmentThreshold");
			XElement alignerElem = elem.Element(ConfigManager.Cog + "ApplicableWordAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			XElement cognateIdentifierElem = elem.Element(ConfigManager.Cog + "ApplicableCognateIdentifier");
			Debug.Assert(cognateIdentifierElem != null);
			var cognateIdentifierID = (string) cognateIdentifierElem.Attribute("ref");
			return new CognacyWordPairGenerator(segmentPool, project, initialAlignmentThreshold, alignerID, cognateIdentifierID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var wordPairGenerator = (CognacyWordPairGenerator) component;
			elem.Add(new XElement(ConfigManager.Cog + "InitialAlignmentThreshold", wordPairGenerator.InitialAlignmentThreshold));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableWordAligner", new XAttribute("ref", wordPairGenerator.AlignerID)));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableCognateIdentifier", new XAttribute("ref", wordPairGenerator.CognateIdentifierID)));
		}
	}
}
