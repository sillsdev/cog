using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Domain.Config.Components
{
	public class SimpleWordPairGeneratorConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var initialAlignmentThreshold = (double) elem.Element(ConfigManager.Cog + "InitialAlignmentThreshold");
			XElement alignerElem = elem.Element(ConfigManager.Cog + "ApplicableWordAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			return new SimpleWordPairGenerator(segmentPool, project, initialAlignmentThreshold, alignerID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var wordPairGenerator = (SimpleWordPairGenerator) component;
			elem.Add(new XElement(ConfigManager.Cog + "InitialAlignmentThreshold", wordPairGenerator.InitialAlignmentThreshold));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableWordAligner", new XAttribute("ref", wordPairGenerator.AlignerID)));
		}
	}
}
