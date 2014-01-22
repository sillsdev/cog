using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine;

namespace SIL.Cog.Domain.Config.Components
{
	public class UnsupervisedAffixIdentifierConfig : IComponentConfig<IProcessor<Variety>>
	{
		public IProcessor<Variety> Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var stemThreshold = (double) elem.Element(ConfigManager.Cog + "AffixThreshold");
			var maxAffixLen = (int) elem.Element(ConfigManager.Cog + "MaxAffixLength");
			bool catRequired = (bool?) elem.Element(ConfigManager.Cog + "CategoryRequired") ?? false;
			return new UnsupervisedAffixIdentifier(spanFactory, segmentPool, stemThreshold, maxAffixLen, catRequired);
		}

		public void Save(IProcessor<Variety> component, XElement elem)
		{
			var identifier = (UnsupervisedAffixIdentifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "AffixThreshold", identifier.Threshold));
			elem.Add(new XElement(ConfigManager.Cog + "MaxAffixLength", identifier.MaxAffixLength));
			elem.Add(new XElement(ConfigManager.Cog + "CategoryRequired", identifier.CategoryRequired));
		}
	}
}
