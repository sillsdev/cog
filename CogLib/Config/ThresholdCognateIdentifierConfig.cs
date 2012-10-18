using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.Config
{
	public class ThresholdCognateIdentifierConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			var thresholdStr = (string) elem.Element("Threshold");
			XElement alignerElem = elem.Element("ApplicableAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			return new ThresholdCognateIdentifier(project, double.Parse(thresholdStr), alignerID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var identifier = (ThresholdCognateIdentifier) component;
			elem.Add(new XElement("Threshold", identifier.Threshold));
			elem.Add(new XElement("ApplicableAligner", new XAttribute("ref", identifier.AlignerID)));
		}
	}
}
