using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.Config
{
	public class WordPairGeneratorConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			XElement alignerElem = elem.Element("ApplicableAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			return new WordPairGenerator(project, alignerID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var generator = (WordPairGenerator) component;
			elem.Add(new XElement("ApplicableAligner", new XAttribute("ref", generator.AlignerID)));
		}
	}
}
