using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Config.Components
{
	public class SimpleSyllabifierConfig : IComponentConfig<IProcessor<Variety>>
	{
		public IProcessor<Variety> Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var combineVowels = (bool?) elem.Element(ConfigManager.Cog + "CombineVowels") ?? true;
			var combineCons = (bool?) elem.Element(ConfigManager.Cog + "CombineConsonants") ?? true;
			return new SimpleSyllabifier(combineVowels, combineCons);
		}

		public void Save(IProcessor<Variety> component, XElement elem)
		{
			var syllabifier = (SimpleSyllabifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "CombineVowels", syllabifier.CombineVowels));
			elem.Add(new XElement(ConfigManager.Cog + "CombineConsonants", syllabifier.CombineConsonants));
		}
	}
}
