using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.Config
{
	public class ListSimilarSegmentIdentifierConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem)
		{
			XElement similarVowelsElem = elem.Element("SimilarVowels");
			XElement similarConsElem = elem.Element("SimilarConsonants");
			var genVowelsStr = (string) elem.Element("GenerateDiphthongs") ?? "false";
			return new ListSimilarSegmentIdentifier(ParseMappings(project.Segmenter, similarVowelsElem), ParseMappings(project.Segmenter, similarConsElem), bool.Parse(genVowelsStr));
		}

		private IEnumerable<Tuple<string, string>> ParseMappings(Segmenter segmenter, XElement elem)
		{
			foreach (XElement mappingElem in elem.Elements("Mapping"))
			{
				string seg1Str, seg2Str;
				if (GetNormalizedStrRep(segmenter, (string) mappingElem.Attribute("segment1"), out seg1Str) && GetNormalizedStrRep(segmenter, (string) mappingElem.Attribute("segment2"), out seg2Str))
					yield return Tuple.Create(seg1Str, seg2Str);
			}
		}

		private static bool GetNormalizedStrRep(Segmenter segmenter, string str, out string normalizedStr)
		{
			Shape shape;
			if (segmenter.ToShape(str, out shape) && shape.Count == 1)
			{
				normalizedStr = shape.First.StrRep();
				return true;
			}
			normalizedStr = null;
			return false;
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
			var identifier = (ListSimilarSegmentIdentifier) component;
			elem.Add(new XElement("SimilarVowels", CreateMappings(identifier.VowelMappings)));
			elem.Add(new XElement("SimilarConsonants", CreateMappings(identifier.ConsonantMappings)));
			elem.Add(new XElement("GenerateDiphthongs", identifier.GenerateDiphthongs));
		}

		private IEnumerable<XElement> CreateMappings(IEnumerable<Tuple<string, string>> mappings)
		{
			return mappings.Select(mapping => new XElement("Mapping", new XAttribute("segment1", mapping.Item1), new XAttribute("segment2", mapping.Item2)));
		}
	}
}
