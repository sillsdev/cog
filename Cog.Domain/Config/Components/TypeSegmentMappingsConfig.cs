using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Config.Components
{
	public class TypeSegmentMappingsConfig : IComponentConfig<ISegmentMappings>
	{
		public ISegmentMappings Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var vowelMappings = ConfigManager.LoadComponent<ISegmentMappings>(spanFactory, segmentPool, project, elem.Element(ConfigManager.Cog + "VowelMappings"));
			var consonantMappings = ConfigManager.LoadComponent<ISegmentMappings>(spanFactory, segmentPool, project, elem.Element(ConfigManager.Cog + "ConsonantMappings"));
			return new TypeSegmentMappings(vowelMappings, consonantMappings);
		}

		public void Save(ISegmentMappings component, XElement elem)
		{
			var ssmappings = (TypeSegmentMappings) component;
			elem.Add(ConfigManager.SaveComponent("VowelMappings", ssmappings.VowelMappings));
			elem.Add(ConfigManager.SaveComponent("ConsonantMappings", ssmappings.ConsonantMappings));
		}
	}
}
