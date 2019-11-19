using System.Linq;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Domain.Config.Components
{
	public class UnionSegmentMappingsConfig : IComponentConfig<ISegmentMappings>
	{
		public ISegmentMappings Load(SegmentPool segmentPool, CogProject project, XElement elem)
		{
			return new UnionSegmentMappings(elem.Elements(ConfigManager.Cog + "SegmentMappings").Select(e => ConfigManager.LoadComponent<ISegmentMappings>(segmentPool, project, e)));
		}

		public void Save(ISegmentMappings component, XElement elem)
		{
			var unionSegmentMappings = (UnionSegmentMappings) component;
			foreach (ISegmentMappings segmentMappings in unionSegmentMappings.SegmentMappingsComponents)
				elem.Add(ConfigManager.SaveComponent("SegmentMappings", segmentMappings));
		}
	}
}
