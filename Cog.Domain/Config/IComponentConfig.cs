using System.Xml.Linq;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Config
{
	public interface IComponentConfig<T>
	{
		T Load(SegmentPool segmentPool, CogProject project, XElement elem);
		void Save(T component, XElement elem);
	}
}
