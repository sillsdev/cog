using System.Collections.Generic;
using System.Linq;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application.Services.ProjectMigrations
{
	internal class ProjectMigration4 : IProjectMigration
	{
		public int Version
		{
			get { return 4; }
		}

		public void Migrate(SegmentPool segmentPool, CogProject project)
		{
			// add prenasal superscript for n
			project.Segmenter.Consonants.AddSymbolBasedOn("ⁿ", "n");

			foreach (KeyValuePair<string, IProcessor<Variety>> kvp in project.VarietyProcessors.Where(kvp => kvp.Value is SspSyllabifier).ToArray())
			{
				var syllabifier = (SspSyllabifier) kvp.Value;
				SonorityClass[] scale = syllabifier.SonorityScale.OrderBy(sc => sc.Sonority).ThenBy(sc => sc.SoundClass.Name).ToArray();
				// if the user has changed the sonority scale preserve their changes and do not update
				if (HasSonorityScaleChanged(project, scale))
					continue;

				// add prenasal sonority class
				var newScale = new List<SonorityClass> {new SonorityClass(1, new UnnaturalClass("Prenasal", new[] {"ᵐ", "ⁿ", "ᵑ"}, false, project.Segmenter))};
				foreach (SonorityClass sc in scale)
				{
					SoundClass newClass;
					switch (sc.SoundClass.Name)
					{
						case "Glide":
							// correct the ignore modifiers flag on the "Glide" class
							newClass = new UnnaturalClass("Glide", new[] {"j", "ɥ", "ɰ", "w"}, true, project.Segmenter);
							break;
						case "Open vowel":
							// correct the height feature value on the "Open vowel" class
							newClass = new NaturalClass("Open vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic+").Symbol("open").Value);
							break;
						default:
							newClass = sc.SoundClass;
							break;
					}
					// increment sonority for all existing classes
					newScale.Add(new SonorityClass(sc.Sonority + 1, newClass));
				}
				project.VarietyProcessors[kvp.Key] = new SspSyllabifier(syllabifier.CombineVowels, syllabifier.CombineConsonants, syllabifier.VowelsSameSonorityTautosyllabic,
					segmentPool, newScale);
			}
		}

		private bool HasSonorityScaleChanged(CogProject project, SonorityClass[] scale)
		{
			SonorityClass[] origSonorityScale = {
					new SonorityClass(1, new NaturalClass("Stop", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("stop").Symbol("nasal-").Value)),
					new SonorityClass(2, new NaturalClass("Affricate", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("affricate").Value)),
					new SonorityClass(3, new NaturalClass("Fricative", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("fricative").Symbol("lateral-").Value)),
					new SonorityClass(4, new NaturalClass("Nasal", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("nasal+").Value)),
					new SonorityClass(5, new NaturalClass("Trill", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("trill").Value)),
					new SonorityClass(6, new NaturalClass("Lateral", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("lateral+").Value)),
					new SonorityClass(7, new NaturalClass("Flap", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("flap").Value)),
					new SonorityClass(8, new UnnaturalClass("Glide", new[] {"j", "ɥ", "ɰ", "w"}, false, project.Segmenter)),
					new SonorityClass(8, new NaturalClass("Non-syllabic vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic-").Value)),
					new SonorityClass(9, new NaturalClass("Close vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic+").Symbol("close").Value)),
					new SonorityClass(10, new NaturalClass("Near-close vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic+").Symbol("near-close").Value)),
					new SonorityClass(11, new NaturalClass("Close-mid vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic+").Symbol("close-mid").Value)),
					new SonorityClass(12, new NaturalClass("Mid vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic+").Symbol("mid").Value)),
					new SonorityClass(13, new NaturalClass("Open-mid vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic+").Symbol("open-mid").Value)),
					new SonorityClass(14, new NaturalClass("Near-open vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic+").Symbol("near-open").Value)),
					new SonorityClass(15, new NaturalClass("Open vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic+").Symbol("open-vowel").Value))
				};

			if (origSonorityScale.Length != scale.Length)
				return true;

			for (int i = 0; i < origSonorityScale.Length; i++)
			{
				SonorityClass origClass = origSonorityScale[i];
				SonorityClass curClass = scale[i];
				if (origClass.Sonority != curClass.Sonority || origClass.SoundClass.Name != curClass.SoundClass.Name)
					return true;

				var origNC = origClass.SoundClass as NaturalClass;
				if (origNC != null)
				{
					var curNC = curClass.SoundClass as NaturalClass;
					if (curNC == null)
						return true;
					FeatureStruct origFS1 = origNC.FeatureStruct;
					FeatureStruct origFS2 = null;
					// the original open vowel class can have the height set to open or the manner set to open-vowel
					if (origNC.Name == "Open vowel")
						origFS2 = FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic+").Symbol("open").Value;
					if (!origFS1.ValueEquals(curNC.FeatureStruct) && (origFS2 == null || !origFS2.ValueEquals(curNC.FeatureStruct)))
						return true;
				}
				else
				{
					var origUC = (UnnaturalClass) origClass.SoundClass;
					var curUC = curClass.SoundClass as UnnaturalClass;
					if (curUC == null || !origUC.Segments.SequenceEqual(curUC.Segments))
						return true;
				}
			}
			return false;
		}
	}
}
