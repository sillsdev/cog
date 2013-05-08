using System.Xml.Linq;
using SIL.Cog.Components;
using SIL.Machine;

namespace SIL.Cog.Config.Components
{
	public class StemmerConfig : IComponentConfig<IProcessor<Variety>>
	{
		public IProcessor<Variety> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			return new Stemmer(spanFactory, project);
		}

		public void Save(IProcessor<Variety> component, XElement elem)
		{
		}
	}
}
