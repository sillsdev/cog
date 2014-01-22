using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine;

namespace SIL.Cog.Domain.Config.Components
{
	public class ThresholdCognateIdentifierConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var threshold = (double) elem.Element(ConfigManager.Cog + "Threshold");
			XElement alignerElem = elem.Element(ConfigManager.Cog + "ApplicableAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			return new ThresholdCognateIdentifier(project, threshold, alignerID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var identifier = (ThresholdCognateIdentifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "Threshold", identifier.Threshold));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableAligner", new XAttribute("ref", identifier.AlignerID)));
		}
	}
}
