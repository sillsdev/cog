using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Config.Components
{
	public class PoorMansAffixIdentifierConfig : IComponentConfig<IProcessor<Variety>>
	{
		public IProcessor<Variety> Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var stemThreshold = (double) elem.Element(ConfigManager.Cog + "AffixThreshold");
			var maxAffixLen = (int) elem.Element(ConfigManager.Cog + "MaxAffixLength");
			return new PoorMansAffixIdentifier(spanFactory, segmentPool, stemThreshold, maxAffixLen);
		}

		public void Save(IProcessor<Variety> component, XElement elem)
		{
			var identifier = (PoorMansAffixIdentifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "AffixThreshold", identifier.Threshold));
			elem.Add(new XElement(ConfigManager.Cog + "MaxAffixLength", identifier.MaxAffixLength));
		}
	}
}
