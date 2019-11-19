using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Domain.Config.Components
{
	public class ThresholdSegmentMappingsConfig : IComponentConfig<ISegmentMappings>
	{
		public ISegmentMappings Load(SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var threshold = (int) elem.Element(ConfigManager.Cog + "Threshold");
			XElement alignerElem = elem.Element(ConfigManager.Cog + "ApplicableWordAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			return new ThresholdSegmentMappings(project, threshold, alignerID);
		}

		public void Save(ISegmentMappings component, XElement elem)
		{
			var identifier = (ThresholdSegmentMappings) component;
			elem.Add(new XElement(ConfigManager.Cog + "Threshold", identifier.Threshold));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableWordAligner", new XAttribute("ref", identifier.AlignerID)));
		}
	}
}
