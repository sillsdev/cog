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
			var vowelThresholdStr = (string) elem.Element("VowelThreshold");
			var consThresholdStr = (string) elem.Element("ConsonantThreshold");
			XElement alignerElem = elem.Element("ApplicableAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			return new ThresholdSimilarSegmentIdentifier(project, int.Parse(vowelThresholdStr), int.Parse(consThresholdStr), alignerID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var identifier = (ThresholdSimilarSegmentIdentifier) component;
			elem.Add(new XElement("VowelThreshold", identifier.VowelThreshold));
			elem.Add(new XElement("ConsonantThreshold", identifier.ConsonantThreshold));
			elem.Add(new XElement("ApplicableAligner", new XAttribute("ref", identifier.AlignerID)));
		}
	}
}
