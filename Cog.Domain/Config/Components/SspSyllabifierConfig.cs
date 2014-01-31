using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Config.Components
{
	public class SspSyllabifierConfig : IComponentConfig<IProcessor<Variety>>
	{
		public IProcessor<Variety> Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var combineVowels = (bool?) elem.Element(ConfigManager.Cog + "CombineVowels") ?? true;
			var combineCons = (bool?) elem.Element(ConfigManager.Cog + "CombineConsonants") ?? true;
			var vowelsSameSonorityTautosyllabic = (bool?) elem.Element(ConfigManager.Cog + "VowelsSameSonorityTautosyllabic") ?? false;
			XElement scaleElem = elem.Element(ConfigManager.Cog + "SonorityScale");
			Debug.Assert(scaleElem != null);
			var sonorityScale = new List<SonorityClass>();
			foreach (XElement scElem in scaleElem.Elements(ConfigManager.Cog + "SonorityClass"))
			{
				var sonority = (int) scElem.Attribute("sonority");
				SoundClass soundClass = ConfigManager.LoadSoundClass(project.Segmenter, project.FeatureSystem, scElem.Elements().First());
				sonorityScale.Add(new SonorityClass(sonority, soundClass));
			}
			return new SspSyllabifier(combineVowels, combineCons,vowelsSameSonorityTautosyllabic, segmentPool, sonorityScale);
		}

		public void Save(IProcessor<Variety> component, XElement elem)
		{
			var syllabifier = (SspSyllabifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "CombineVowels", syllabifier.CombineVowels));
			elem.Add(new XElement(ConfigManager.Cog + "CombineConsonants", syllabifier.CombineConsonants));
			elem.Add(new XElement(ConfigManager.Cog + "VowelsSameSonorityTautosyllabic", syllabifier.VowelsSameSonorityTautosyllabic));
			elem.Add(new XElement(ConfigManager.Cog + "SonorityScale",
				syllabifier.SonorityScale.Select(sc => new XElement(ConfigManager.Cog + "SonorityClass", new XAttribute("sonority", sc.Sonority), ConfigManager.SaveSoundClass(sc.SoundClass)))));
		}
	}
}
