using System.Xml.Linq;
using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.Config
{
	public class UnsupervisedAffixIdentifierConfig : IComponentConfig<IProcessor<Variety>>
	{
		public IProcessor<Variety> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			var stemThresholdStr = (string) elem.Element("AffixThreshold");
			var maxAffixLenStr = (string) elem.Element("MaxAffixLength");
			var catRequired = (string) elem.Element("CategoryRequired");
			return new UnsupervisedAffixIdentifier(spanFactory, double.Parse(stemThresholdStr), int.Parse(maxAffixLenStr), catRequired != null && bool.Parse(catRequired));
		}

		public void Save(IProcessor<Variety> component, XElement elem)
		{
			var identifier = (UnsupervisedAffixIdentifier) component;
			elem.Add(new XElement("AffixThreshold", identifier.Threshold));
			elem.Add(new XElement("MaxAffixLength", identifier.MaxAffixLength));
			elem.Add(new XElement("CategoryRequired", identifier.CategoryRequired));
		}
	}
}
