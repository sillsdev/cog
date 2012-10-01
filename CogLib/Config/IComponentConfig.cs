using System.Xml.Linq;
using SIL.Machine;

namespace SIL.Cog.Config
{
	public interface IComponentConfig<T>
	{
		T Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem);
		void Save(T component, XElement elem);
	}
}
