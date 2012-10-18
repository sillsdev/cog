using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.Config
{
	public class BlairCognateIdentifierConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			var blairThresholdStr = (string) elem.Element("AlignmentThreshold");
			var ignoreRegularInsertionDeletionStr = (string) elem.Element("IgnoreRegularInsertionDeletion");
			var regularConsEqualStr = (string) elem.Element("RegularConsonantsAreEqual");
			XElement alignerElem = elem.Element("ApplicableAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			return new BlairCognateIdentifier(project, double.Parse(blairThresholdStr),
				ignoreRegularInsertionDeletionStr != null && bool.Parse(ignoreRegularInsertionDeletionStr),
				regularConsEqualStr != null && bool.Parse(regularConsEqualStr), alignerID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var blair = (BlairCognateIdentifier) component;
			elem.Add(new XElement("AlignmentThreshold", blair.AlignmentThreshold));
			elem.Add(new XElement("IgnoreRegularInsertionDeletion", blair.IgnoreRegularInsertionDeletion));
			elem.Add(new XElement("RegularConsonantsAreEqual", blair.RegularConsonantEqual));
			elem.Add(new XElement("ApplicableAligner", new XAttribute("ref", blair.AlignerID)));
		}
	}
}
