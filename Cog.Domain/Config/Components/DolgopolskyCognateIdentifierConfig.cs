using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Config.Components
{
	public class DolgopolskyCognateIdentifierConfig : IComponentConfig<ICognateIdentifier>
	{
		public ICognateIdentifier Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			XElement soundClassesElem = elem.Element(ConfigManager.Cog + "SoundClasses");
			Debug.Assert(soundClassesElem != null);
			IEnumerable<SoundClass> soundClasses = ConfigManager.LoadSoundClasses(project.Segmenter, project.FeatureSystem, soundClassesElem);
			var threshold = (int) elem.Element(ConfigManager.Cog + "InitialEquivalenceThreshold");
			return new DolgopolskyCognateIdentifier(segmentPool, soundClasses, threshold);
		}

		public void Save(ICognateIdentifier component, XElement elem)
		{
			var dolgopolsky = (DolgopolskyCognateIdentifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "SoundClasses", ConfigManager.SaveSoundClasses(dolgopolsky.SoundClasses)));
			elem.Add(new XElement(ConfigManager.Cog + "InitialEquivalenceThreshold", dolgopolsky.InitialEquivalenceThreshold));
		}
	}
}
