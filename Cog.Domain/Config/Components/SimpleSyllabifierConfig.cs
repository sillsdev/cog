using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine;

namespace SIL.Cog.Domain.Config.Components
{
	public class SimpleSyllabifierConfig : IComponentConfig<IProcessor<Variety>>
	{
		public IProcessor<Variety> Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var combineVowelsStr = (string) elem.Element(ConfigManager.Cog + "CombineVowels");
			var combineConsStr = (string) elem.Element(ConfigManager.Cog + "CombineConsonants");
			return new SimpleSyllabifier(combineVowelsStr == null || bool.Parse(combineVowelsStr), combineConsStr == null || bool.Parse(combineConsStr));
		}

		public void Save(IProcessor<Variety> component, XElement elem)
		{
			var syllabifier = (SimpleSyllabifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "CombineVowels", syllabifier.CombineVowels));
			elem.Add(new XElement(ConfigManager.Cog + "CombineConsonants", syllabifier.CombineConsonants));
		}
	}
}
