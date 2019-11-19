using System.Xml.Linq;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Domain.Config.Components
{
	public class BlairCognateIdentifierConfig : IComponentConfig<ICognateIdentifier>
	{
		public ICognateIdentifier Load(SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var ignoreRegularInsertionDeletion = (bool?) elem.Element(ConfigManager.Cog + "IgnoreRegularInsertionDeletion") ?? false;
			var regularConsEqual = (bool?) elem.Element(ConfigManager.Cog + "RegularConsonantsAreEqual") ?? false;
			var automaticRegularCorrespondenceThreshold = (bool?) elem.Element(ConfigManager.Cog + "AutomaticRegularCorrespondenceThreshold") ?? false;
			var defaultRegularCorrespondenceThreshold = (int?) elem.Element(ConfigManager.Cog + "DefaultRegularCorrespondenceThreshold") ?? 3;

			var ignoredMappings = ConfigManager.LoadComponent<ISegmentMappings>(segmentPool, project, elem.Element(ConfigManager.Cog + "IgnoredCorrespondences"));
			var similarSegments = ConfigManager.LoadComponent<ISegmentMappings>(segmentPool, project, elem.Element(ConfigManager.Cog + "SimilarSegments"));

			return new BlairCognateIdentifier(segmentPool, ignoreRegularInsertionDeletion, regularConsEqual, automaticRegularCorrespondenceThreshold,
				defaultRegularCorrespondenceThreshold, ignoredMappings, similarSegments);
		}

		public void Save(ICognateIdentifier component, XElement elem)
		{
			var blair = (BlairCognateIdentifier) component;
			elem.Add(new XElement(ConfigManager.Cog + "IgnoreRegularInsertionDeletion", blair.IgnoreRegularInsertionDeletion));
			elem.Add(new XElement(ConfigManager.Cog + "RegularConsonantsAreEqual", blair.RegularConsonantEqual));
			elem.Add(new XElement(ConfigManager.Cog + "AutomaticRegularCorrespondenceThreshold", blair.AutomaticRegularCorrespondenceThreshold));
			elem.Add(new XElement(ConfigManager.Cog + "DefaultRegularCorrespondenceThreshold", blair.DefaultRegularCorrespondenceThreshold));
			elem.Add(ConfigManager.SaveComponent("IgnoredCorrespondences", blair.IgnoredMappings));
			elem.Add(ConfigManager.SaveComponent("SimilarSegments", blair.SimilarSegments));
		}
	}
}
