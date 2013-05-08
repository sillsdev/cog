using System.Xml.Linq;
using SIL.Cog.Components;
using SIL.Machine;

namespace SIL.Cog.Config.Components
{
	public class PrecisionRecallCalculatorConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			return new PrecisionRecallCalculator();
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
		}
	}
}
