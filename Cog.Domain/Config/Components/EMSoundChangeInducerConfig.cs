using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Config.Components
{
	public class EMSoundChangeInducerConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			XElement alignerElem = elem.Element(ConfigManager.Cog + "ApplicableWordAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			XElement cognateIdentifierElem = elem.Element(ConfigManager.Cog + "ApplicableCognateIdentifier");
			Debug.Assert(cognateIdentifierElem != null);
			var cognateIdentifierID = (string) cognateIdentifierElem.Attribute("ref");
			return new EMSoundChangeInducer(segmentPool, project, alignerID, cognateIdentifierID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var inducer = (EMSoundChangeInducer) component;
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableWordAligner", new XAttribute("ref", inducer.AlignerID)));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableCognateIdentifier", new XAttribute("ref", inducer.CognateIdentifierID)));
		}
	}
}
