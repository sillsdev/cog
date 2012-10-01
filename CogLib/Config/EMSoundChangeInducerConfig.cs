using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.Config
{
	public class EMSoundChangeInducerConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			var soundChangeThresholdStr = (string) elem.Element("AlignmentThreshold");
			XElement alignerElem = elem.Element("ApplicableAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			return new EMSoundChangeInducer(project, double.Parse(soundChangeThresholdStr), alignerID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var inducer = (EMSoundChangeInducer) component;
			elem.Add(new XElement("AlignmentThreshold", inducer.Threshold));
			elem.Add(new XElement("ApplicableAligner", new XAttribute("ref", inducer.AlignerID)));
		}
	}
}
