using System.Xml.Linq;
using SIL.Cog.Aligners;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Config
{
	public abstract class AlignerConfig : IComponentConfig<IAligner>
	{
		public abstract IAligner Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem);

		protected AlignerSettings LoadSettings(Segmenter segmenter, FeatureSystem featSys, XElement elem)
		{
			var settings = new AlignerSettings();
			var modeStr = (string) elem.Element(ConfigManager.Cog + "Mode");
			if (modeStr != null)
			{
				switch (modeStr)
				{
					case "local":
						settings.Mode = AlignerMode.Local;
						break;
					case "global":
						settings.Mode = AlignerMode.Global;
						break;
					case "semi-global":
						settings.Mode = AlignerMode.SemiGlobal;
						break;
					case "half-local":
						settings.Mode = AlignerMode.HalfLocal;
						break;
				}
			}
			var disableExpansionCompressionStr = (string) elem.Element(ConfigManager.Cog + "DisableExpansionCompression");
			if (disableExpansionCompressionStr != null)
				settings.DisableExpansionCompression = bool.Parse(disableExpansionCompressionStr);
			XElement soundClassesElem = elem.Element(ConfigManager.Cog + "ContextualSoundClasses");
			if (soundClassesElem != null && soundClassesElem.HasElements)
				settings.ContextualSoundClasses = ConfigManager.LoadSoundClasses(segmenter, featSys, soundClassesElem);
			return settings;
		}

		public abstract void Save(IAligner component, XElement elem);

		protected void SaveSettings(AlignerSettings settings, XElement elem)
		{
			string modeStr = null;
			switch (settings.Mode)
			{
				case AlignerMode.Local:
					modeStr = "local";
					break;
				case AlignerMode.Global:
					modeStr = "global";
					break;
				case AlignerMode.SemiGlobal:
					modeStr = "semi-global";
					break;
				case AlignerMode.HalfLocal:
					modeStr = "half-local";
					break;
			}
			elem.Add(new XElement(ConfigManager.Cog + "Mode", modeStr));
			elem.Add(new XElement(ConfigManager.Cog + "DisableExpansionCompression", settings.DisableExpansionCompression));
			if (settings.ContextualSoundClasses != null)
			{
				elem.Add(new XElement(ConfigManager.Cog + "ContextualSoundClasses", ConfigManager.SaveSoundClasses(settings.ContextualSoundClasses)));
			}
		}
	}
}
