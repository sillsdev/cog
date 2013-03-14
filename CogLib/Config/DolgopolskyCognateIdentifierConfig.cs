using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.Config
{
	public class DolgopolskyCognateIdentifierConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			XElement soundClassesElem = elem.Element(ConfigManager.Cog + "SoundClasses");
			Debug.Assert(soundClassesElem != null);
			IEnumerable<SoundClass> soundClasses = ConfigManager.LoadSoundClasses(project.Segmenter, project.FeatureSystem, soundClassesElem);
			XElement alignerElem = elem.Element(ConfigManager.Cog + "ApplicableAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			var thresholdStr = (string) elem.Element(ConfigManager.Cog + "InitialEquivalenceThreshold");
			return new DolgopolskyCognateIdentifier(project, soundClasses, int.Parse(thresholdStr), alignerID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var dolgopolsky = (DolgopolskyCognateIdentifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "SoundClasses", ConfigManager.SaveSoundClasses(dolgopolsky.SoundClasses)));
			elem.Add(new XElement(ConfigManager.Cog + "InitialEquivalenceThreshold", dolgopolsky.InitialEquivalenceThreshold));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableAligner", new XAttribute("ref", dolgopolsky.AlignerID)));
		}
	}
}
