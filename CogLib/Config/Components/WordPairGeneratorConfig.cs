using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Components;
using SIL.Machine;

namespace SIL.Cog.Config.Components
{
	public class WordPairGeneratorConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			XElement alignerElem = elem.Element(ConfigManager.Cog + "ApplicableAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string) alignerElem.Attribute("ref");
			return new WordPairGenerator(project, alignerID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var generator = (WordPairGenerator) component;
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableAligner", new XAttribute("ref", generator.AlignerID)));
		}
	}
}
