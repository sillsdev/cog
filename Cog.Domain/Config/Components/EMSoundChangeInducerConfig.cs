using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine;

namespace SIL.Cog.Domain.Config.Components
{
	public class EMSoundChangeInducerConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			var soundChangeThresholdStr = (string) elem.Element(ConfigManager.Cog + "InitialAlignmentThreshold");
			XElement alignerElem = elem.Element(ConfigManager.Cog + "ApplicableAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			XElement cognateIdentifierElem = elem.Element(ConfigManager.Cog + "CognateIdentifier");
			Debug.Assert(cognateIdentifierElem != null);
			var cognateIdentifierID = (string) cognateIdentifierElem.Attribute("ref");
			return new EMSoundChangeInducer(project, double.Parse(soundChangeThresholdStr), alignerID, cognateIdentifierID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var inducer = (EMSoundChangeInducer) component;
			elem.Add(new XElement(ConfigManager.Cog + "InitialAlignmentThreshold", inducer.InitialAlignmentThreshold));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableAligner", new XAttribute("ref", inducer.AlignerID)));
			elem.Add(new XElement(ConfigManager.Cog + "CognateIdentifier", new XAttribute("ref", inducer.CognateIdentifierID)));
		}
	}
}
