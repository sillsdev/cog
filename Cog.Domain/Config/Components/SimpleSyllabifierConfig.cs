using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine;

namespace SIL.Cog.Domain.Config.Components
{
	public class SimpleSyllabifierConfig : IComponentConfig<IProcessor<Variety>>
	{
		public IProcessor<Variety> Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var combineSegmentsStr = (string) elem.Element(ConfigManager.Cog + "CombineSegments");
			return new SimpleSyllabifier(combineSegmentsStr == null || bool.Parse(combineSegmentsStr));
		}

		public void Save(IProcessor<Variety> component, XElement elem)
		{
			var syllabifier = (SimpleSyllabifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "CombineSegments", syllabifier.CombineSegments));
		}
	}
}
