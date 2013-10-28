using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine;

namespace SIL.Cog.Domain.Config.Components
{
	public class SspSyllabifierConfig : IComponentConfig<IProcessor<Variety>>
	{
		public IProcessor<Variety> Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var combineSegmentsStr = (string) elem.Element(ConfigManager.Cog + "CombineSegments");
			var vowelsSameSonorityTautosyllabicStr = (string) elem.Element(ConfigManager.Cog + "VowelsSameSonorityTautosyllabic");
			XElement scaleElem = elem.Element(ConfigManager.Cog + "SonorityScale");
			Debug.Assert(scaleElem != null);
			var sonorityScale = new List<SonorityClass>();
			foreach (XElement scElem in scaleElem.Elements(ConfigManager.Cog + "SonorityClass"))
			{
				int sonority = int.Parse((string) scElem.Attribute("sonority"));
				SoundClass soundClass = ConfigManager.LoadSoundClass(project.Segmenter, project.FeatureSystem, scElem.Elements().First());
				sonorityScale.Add(new SonorityClass(sonority, soundClass));
			}
			return new SspSyllabifier(combineSegmentsStr == null || bool.Parse(combineSegmentsStr), vowelsSameSonorityTautosyllabicStr != null && bool.Parse(vowelsSameSonorityTautosyllabicStr),
				segmentPool, sonorityScale);
		}

		public void Save(IProcessor<Variety> component, XElement elem)
		{
			var syllabifier = (SspSyllabifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "CombineSegments", syllabifier.CombineSegments));
			elem.Add(new XElement(ConfigManager.Cog + "VowelsSameSonorityTautosyllabic", syllabifier.VowelsSameSonorityTautosyllabic));
			elem.Add(new XElement(ConfigManager.Cog + "SonorityScale",
				syllabifier.SonorityScale.Select(sc => new XElement(ConfigManager.Cog + "SonorityClass", new XAttribute("sonority", sc.Sonority), ConfigManager.SaveSoundClass(sc.SoundClass)))));
		}
	}
}
