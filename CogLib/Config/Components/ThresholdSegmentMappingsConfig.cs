using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Components;
using SIL.Machine;

namespace SIL.Cog.Config.Components
{
	public class ThresholdSegmentMappingsConfig : IComponentConfig<ISegmentMappings>
	{
		public ISegmentMappings Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			var thresholdStr = (string) elem.Element(ConfigManager.Cog + "Threshold");
			XElement alignerElem = elem.Element(ConfigManager.Cog + "ApplicableAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			return new ThresholdSegmentMappings(project, int.Parse(thresholdStr), alignerID);
		}

		public void Save(ISegmentMappings component, XElement elem)
		{
			var identifier = (ThresholdSegmentMappings) component;
			elem.Add(new XElement(ConfigManager.Cog + "Threshold", identifier.Threshold));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableAligner", new XAttribute("ref", identifier.AlignerID)));
		}
	}
}