using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Config.Components
{
	public class BlairCognateIdentifierConfig : IComponentConfig<ICognateIdentifier>
	{
		public ICognateIdentifier Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var ignoreRegularInsertionDeletion = (bool?) elem.Element(ConfigManager.Cog + "IgnoreRegularInsertionDeletion") ?? false;
			var regularConsEqual = (bool?) elem.Element(ConfigManager.Cog + "RegularConsonantsAreEqual") ?? false;

			var ignoredMappings = ConfigManager.LoadComponent<ISegmentMappings>(spanFactory, segmentPool, project, elem.Element(ConfigManager.Cog + "IgnoredCorrespondences"));
			var similarSegments = ConfigManager.LoadComponent<ISegmentMappings>(spanFactory, segmentPool, project, elem.Element(ConfigManager.Cog + "SimilarSegments"));

			return new BlairCognateIdentifier(segmentPool, ignoreRegularInsertionDeletion, regularConsEqual, ignoredMappings, similarSegments);
		}

		public void Save(ICognateIdentifier component, XElement elem)
		{
			var blair = (BlairCognateIdentifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "IgnoreRegularInsertionDeletion", blair.IgnoreRegularInsertionDeletion));
			elem.Add(new XElement(ConfigManager.Cog + "RegularConsonantsAreEqual", blair.RegularConsonantEqual));
			elem.Add(ConfigManager.SaveComponent("IgnoredCorrespondences", blair.IgnoredMappings));
			elem.Add(ConfigManager.SaveComponent("SimilarSegments", blair.SimilarSegments));
		}
	}
}
