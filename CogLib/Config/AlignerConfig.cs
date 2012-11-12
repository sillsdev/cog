using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SIL.Cog.Aligners;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Config
{
	public abstract class AlignerConfig : IComponentConfig<IAligner>
	{
		public abstract IAligner Load(SpanFactory<ShapeNode> spanFactory, CogProject project, XElement elem);

		protected AlignerSettings LoadSettings(FeatureSystem featSys, XElement elem)
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
			XElement naturalClassesElem = elem.Element(ConfigManager.Cog + "NaturalClasses");
			if (naturalClassesElem != null && naturalClassesElem.HasElements)
			{
				var naturalClasses = new List<NaturalClass>();
				foreach (XElement ncElem in naturalClassesElem.Elements(ConfigManager.Cog + "NaturalClass"))
				{
					FeatureStruct fs = ConfigManager.LoadFeatureStruct(featSys, ncElem);
					fs.AddValue(CogFeatureSystem.Type, ((string) ncElem.Attribute("type")) == "vowel" ? CogFeatureSystem.VowelType : CogFeatureSystem.ConsonantType);
					naturalClasses.Add(new NaturalClass((string) ncElem.Attribute("name"), fs));
				}
				settings.NaturalClasses = naturalClasses;
			}
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
			if (settings.NaturalClasses != null)
			{
				elem.Add(new XElement(ConfigManager.Cog + "NaturalClasses", settings.NaturalClasses.Select(naturalClass => new XElement(ConfigManager.Cog + "NaturalClass", new XAttribute("name", naturalClass.Name),
					new XAttribute("type", naturalClass.Type == CogFeatureSystem.VowelType ? "vowel" : "consonant"), ConfigManager.CreateFeatureStruct(naturalClass.FeatureStruct)))));
			}
		}
	}
}
