using System.Xml.Linq;
using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.Config
{
	public class StemmerConfig : IComponentConfig<IProcessor<Variety>>
	{
		public IProcessor<Variety> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			return new Stemmer(spanFactory);
		}

		public void Save(IProcessor<Variety> component, XElement elem)
		{
		}
	}
}
