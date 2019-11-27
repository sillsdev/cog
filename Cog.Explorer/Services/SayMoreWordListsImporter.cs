using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SIL.Cog.Domain;
using SIL.Extensions;

namespace SIL.Cog.Explorer.Services
{
	public class SayMoreWordListsImporter
	{
		public void Import(string projectFileName, CogProject project)
		{
			var projectDoc = XDocument.Load(projectFileName);
			string varietyName = Path.GetFileNameWithoutExtension(projectFileName);
			var codeAndName = (string)projectDoc.Root.Element("VernacularISO3CodeAndName");
			if (!string.IsNullOrEmpty(codeAndName))
			{
				int colonIndex = codeAndName.IndexOf(":");
				varietyName = codeAndName.Substring(colonIndex + 1).Trim();
			}
			var variety = new Variety(varietyName);

			var meanings = new Dictionary<string, Meaning>();
			string projectDir = Path.GetDirectoryName(projectFileName);
			string sessionsDir = Path.Combine(projectDir, "Sessions");
			foreach (string sessionDir in Directory.EnumerateDirectories(sessionsDir))
			{
				string sessionId = Path.GetFileName(sessionDir);
				string sessionFileName = Path.Combine(sessionDir, sessionId + ".session");
				LoadSession(sessionFileName, variety, meanings);
			}

			project.Meanings.ReplaceAll(meanings.Values);
			project.Varieties.ReplaceAll(variety.ToEnumerable());
		}

		private static void LoadSession(string sessionFileName, Variety variety, Dictionary<string, Meaning> meanings)
		{
			var sessionDoc = XDocument.Load(sessionFileName);
			string sessionDir = Path.GetDirectoryName(sessionFileName);
			foreach (string annotationsFileName in Directory.EnumerateFiles(sessionDir, "*.annotations.eaf"))
				LoadAnnotations(annotationsFileName, variety, meanings, sessionDoc.Root);
		}

		private static void LoadAnnotations(string annotationsFileName, Variety variety,
			Dictionary<string, Meaning> meanings, XElement sessionElem)
		{
			string sessionDir = Path.GetDirectoryName(annotationsFileName);
			var annotationsDoc = XDocument.Load(annotationsFileName);
			XElement headerElem = annotationsDoc.Root.Element("HEADER");
			XElement mediaDescriptorElem = headerElem.Element("MEDIA_DESCRIPTOR");
			var audioFileName = (string)mediaDescriptorElem.Attribute("MEDIA_URL");

			var timeSlots = new Dictionary<string, int>();
			XElement timeOrderElem = annotationsDoc.Root.Element("TIME_ORDER");
			foreach (XElement timeSlotElem in timeOrderElem.Elements("TIME_SLOT"))
				timeSlots[(string)timeSlotElem.Attribute("TIME_SLOT_ID")] = (int)timeSlotElem.Attribute("TIME_VALUE");

			var annotationToMeaningMap = new Dictionary<string, Meaning>();
			XElement translationTierElem = annotationsDoc.Root.Elements("TIER")
				.First(e => (string)e.Attribute("TIER_ID") == "Phrase Free Translation");
			foreach (XElement annotationElem in translationTierElem.Elements("ANNOTATION")
				.Elements("REF_ANNOTATION"))
			{
				var gloss = ((string)annotationElem.Element("ANNOTATION_VALUE")).Trim();
				if (!meanings.TryGetValue(gloss, out Meaning meaning))
				{
					meaning = new Meaning(gloss, null);
					meanings[gloss] = meaning;
				}
				annotationToMeaningMap[(string)annotationElem.Attribute("ANNOTATION_REF")] = meaning;
			}

			XElement transcriptionTierElem = annotationsDoc.Root.Elements("TIER")
				.First(e => (string)e.Attribute("TIER_ID") == "Transcription");
			foreach (XElement annotationElem in transcriptionTierElem.Elements("ANNOTATION")
				.Elements("ALIGNABLE_ANNOTATION"))
			{
				var id = (string)annotationElem.Attribute("ANNOTATION_ID");
				var startSlotRef = (string)annotationElem.Attribute("TIME_SLOT_REF1");
				var endSlotRef = (string)annotationElem.Attribute("TIME_SLOT_REF2");
				var strRep = ((string)annotationElem.Element("ANNOTATION_VALUE")).Trim();
				int start = timeSlots[startSlotRef];
				int end = timeSlots[endSlotRef];
				Meaning meaning = annotationToMeaningMap[id];
				var word = new Word(strRep, meaning)
				{
					Audio = new Audio(Path.Combine(sessionDir, audioFileName), start, end),
					Participants = (string)sessionElem.Element("participants")
				};
				variety.Words.Add(word);
			}
		}
	}
}
