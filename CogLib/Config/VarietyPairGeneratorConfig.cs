using System.Xml.Linq;
using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.Config
{
	public class VarietyPairGeneratorConfig : IComponentConfig<IProcessor<CogProject>>
	{
		public IProcessor<CogProject> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			return new VarietyPairGenerator();
		}

		public void Save(IProcessor<CogProject> component, XElement elem)
		{
		}
	}
}
