using System.Xml.Linq;
using SIL.Cog.Components;
using SIL.Machine;

namespace SIL.Cog.Config.Components
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
