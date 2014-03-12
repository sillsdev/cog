using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Config.Components
{
	public class ThresholdCognateIdentifierConfig : IComponentConfig<ICognateIdentifier>
	{
		public ICognateIdentifier Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var threshold = (double) elem.Element(ConfigManager.Cog + "Threshold");
			return new ThresholdCognateIdentifier(threshold);
		}

		public void Save(ICognateIdentifier component, XElement elem)
		{
			var identifier = (ThresholdCognateIdentifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "Threshold", identifier.Threshold));
		}
	}
}
