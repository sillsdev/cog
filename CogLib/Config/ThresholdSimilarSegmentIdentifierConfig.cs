using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.Config
{
	public class ThresholdSimilarSegmentIdentifierConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			var vowelThresholdStr = (string) elem.Element(ConfigManager.Cog + "VowelThreshold");
			var consThresholdStr = (string) elem.Element(ConfigManager.Cog + "ConsonantThreshold");
			XElement alignerElem = elem.Element(ConfigManager.Cog + "ApplicableAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			return new ThresholdSimilarSegmentIdentifier(project, int.Parse(vowelThresholdStr), int.Parse(consThresholdStr), alignerID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var identifier = (ThresholdSimilarSegmentIdentifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "VowelThreshold", identifier.VowelThreshold));
			elem.Add(new XElement(ConfigManager.Cog + "ConsonantThreshold", identifier.ConsonantThreshold));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableAligner", new XAttribute("ref", identifier.AlignerID)));
		}
	}
}
