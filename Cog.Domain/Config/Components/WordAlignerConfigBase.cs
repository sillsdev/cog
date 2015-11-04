using System.Linq;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Domain.Config.Components
{
	public abstract class WordAlignerConfigBase : IComponentConfig<IWordAligner>
	{
		public abstract IWordAligner Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem);

		protected void LoadSettings(Segmenter segmenter, FeatureSystem featSys, XElement elem, WordPairAlignerSettings settings)
		{
			var modeStr = (string) elem.Element(ConfigManager.Cog + "Mode");
			if (modeStr != null)
			{
				switch (modeStr)
				{
					case "local":
						settings.Mode = AlignmentMode.Local;
						break;
					case "global":
						settings.Mode = AlignmentMode.Global;
						break;
					case "semi-global":
						settings.Mode = AlignmentMode.SemiGlobal;
						break;
					case "half-local":
						settings.Mode = AlignmentMode.HalfLocal;
						break;
				}
			}
			settings.ExpansionCompressionEnabled = (bool?) elem.Element(ConfigManager.Cog + "ExpansionCompressionEnabled") ?? false;
			XElement soundClassesElem = elem.Element(ConfigManager.Cog + "ContextualSoundClasses");
			if (soundClassesElem != null && soundClassesElem.HasElements)
				settings.ContextualSoundClasses = ConfigManager.LoadSoundClasses(segmenter, featSys, soundClassesElem);
		}

		public abstract void Save(IWordAligner component, XElement elem);

		protected void SaveSettings(WordPairAlignerSettings settings, XElement elem)
		{
			string modeStr = null;
			switch (settings.Mode)
			{
				case AlignmentMode.Local:
					modeStr = "local";
					break;
				case AlignmentMode.Global:
					modeStr = "global";
					break;
				case AlignmentMode.SemiGlobal:
					modeStr = "semi-global";
					break;
				case AlignmentMode.HalfLocal:
					modeStr = "half-local";
					break;
			}
			elem.Add(new XElement(ConfigManager.Cog + "Mode", modeStr));
			elem.Add(new XElement(ConfigManager.Cog + "ExpansionCompressionEnabled", settings.ExpansionCompressionEnabled));
			if (settings.ContextualSoundClasses.Any())
			{
				elem.Add(new XElement(ConfigManager.Cog + "ContextualSoundClasses", ConfigManager.SaveSoundClasses(settings.ContextualSoundClasses)));
			}
		}
	}
}
