using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine;

namespace SIL.Cog.Domain.Config.Components
{
	public class UnsupervisedAffixIdentifierConfig : IComponentConfig<IProcessor<Variety>>
	{
		public IProcessor<Variety> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			var stemThresholdStr = (string) elem.Element(ConfigManager.Cog + "AffixThreshold");
			var maxAffixLenStr = (string) elem.Element(ConfigManager.Cog + "MaxAffixLength");
			var catRequired = (string) elem.Element(ConfigManager.Cog + "CategoryRequired");
			return new UnsupervisedAffixIdentifier(spanFactory, double.Parse(stemThresholdStr), int.Parse(maxAffixLenStr), catRequired != null && bool.Parse(catRequired));
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
