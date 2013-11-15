using System.Collections.Generic;
using System.Linq;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.ViewModels
{
	public class SyllabifierViewModel : ComponentSettingsViewModelBase
	{
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private readonly IAnalysisService _analysisService;
		private bool _automaticSyllabificationEnabled;
		private readonly SoundClassesViewModel _sonorityClasses;
		private bool _combineVowels;
		private bool _combineConsonants;
		private bool _vowelsSameSonorityTautosyllabic;

		public SyllabifierViewModel(SegmentPool segmentPool, IProjectService projectService, IAnalysisService analysisService, SoundClassesViewModel sonorityClasses)
			: base("Syllabification")
		{
			_segmentPool = segmentPool;
			_projectService = projectService;
			_analysisService = analysisService;
			_sonorityClasses = sonorityClasses;
			_sonorityClasses.PropertyChanged += ChildPropertyChanged;
			_sonorityClasses.DisplaySonority = true;
		}

		public override void Setup()
		{
			CogProject project = _projectService.Project;
			IEnumerable<SonorityClass> sonorityScale;
			var syllabifier = (SimpleSyllabifier) project.VarietyProcessors["syllabifier"];
			var sspSyllabifier = syllabifier as SspSyllabifier;
			bool automaticSyllabificationEnabled;
			bool vowelsSameSonorityTautosyllabic;
			if (sspSyllabifier != null)
			{
				vowelsSameSonorityTautosyllabic = sspSyllabifier.VowelsSameSonorityTautosyllabic;
				sonorityScale = sspSyllabifier.SonorityScale;
				automaticSyllabificationEnabled = true;
			}
			else
			{
				vowelsSameSonorityTautosyllabic = false;
				sonorityScale = new[]
					{
						new SonorityClass(1, new NaturalClass("Stop", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("stop").Symbol("nasal-").Value)),
						new SonorityClass(2, new NaturalClass("Affricate", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("affricate").Value)),
						new SonorityClass(3, new NaturalClass("Fricative", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("fricative").Symbol("lateral-").Value)),
						new SonorityClass(4, new NaturalClass("Nasal", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("nasal+").Value)),
						new SonorityClass(5, new NaturalClass("Trill", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("trill").Value)),
						new SonorityClass(6, new NaturalClass("Lateral", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("lateral+").Value)),
						new SonorityClass(7, new NaturalClass("Flap", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("flap").Value)),
						new SonorityClass(8, new UnnaturalClass("Glide", new[] {"j", "ɥ", "ɰ", "w"}, true, project.Segmenter)),
						new SonorityClass(8, new NaturalClass("Non-syllabic vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic-").Value)),
						new SonorityClass(9, new NaturalClass("Close vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("close").Symbol("syllabic+").Value)),
						new SonorityClass(10, new NaturalClass("Near-close vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("near-close").Symbol("syllabic+").Value)),
						new SonorityClass(11, new NaturalClass("Close-mid vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("close-mid").Symbol("syllabic+").Value)),
						new SonorityClass(12, new NaturalClass("Mid vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("mid").Symbol("syllabic+").Value)),
						new SonorityClass(13, new NaturalClass("Open-mid vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("open-mid").Symbol("syllabic+").Value)),
						new SonorityClass(14, new NaturalClass("Near-open vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("near-open").Symbol("syllabic+").Value)),
						new SonorityClass(15, new NaturalClass("Open vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("open").Symbol("syllabic+").Value))
					};
				automaticSyllabificationEnabled = false;
			}

			Set(() => CombineVowels, ref _combineVowels, syllabifier.CombineVowels);
			Set(() => CombineConsonants, ref _combineConsonants, syllabifier.CombineConsonants);
			Set(() => VowelsSameSonorityTautosyllabic, ref _vowelsSameSonorityTautosyllabic, vowelsSameSonorityTautosyllabic);
			Set(() => AutomaticSyllabificationEnabled, ref _automaticSyllabificationEnabled, automaticSyllabificationEnabled);

			_sonorityClasses.SelectedSoundClass = null;
			_sonorityClasses.SoundClasses.Clear();
			foreach (SonorityClass sonorityClass in sonorityScale)
				_sonorityClasses.SoundClasses.Add(new SoundClassViewModel(sonorityClass.SoundClass, sonorityClass.Sonority));
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			_sonorityClasses.AcceptChanges();
		}

		public bool AutomaticSyllabificationEnabled
		{
			get { return _automaticSyllabificationEnabled; }
			set { SetChanged(() => AutomaticSyllabificationEnabled, ref _automaticSyllabificationEnabled, value); }
		}

		public bool CombineVowels
		{
			get { return _combineVowels; }
			set { SetChanged(() => CombineVowels, ref _combineVowels, value); }
		}

		public bool CombineConsonants
		{
			get { return _combineConsonants; }
			set { SetChanged(() => CombineConsonants, ref _combineConsonants, value); }
		}

		public bool VowelsSameSonorityTautosyllabic
		{
			get { return _vowelsSameSonorityTautosyllabic; }
			set { SetChanged(() => VowelsSameSonorityTautosyllabic, ref _vowelsSameSonorityTautosyllabic, value); }
		}

		public SoundClassesViewModel SonorityClasses
		{
			get { return _sonorityClasses; }
		}

		public override object UpdateComponent()
		{
			SimpleSyllabifier syllabifier = _automaticSyllabificationEnabled
				? new SspSyllabifier(_combineVowels, _combineConsonants, _vowelsSameSonorityTautosyllabic, _segmentPool, _sonorityClasses.SoundClasses.Select(sc => new SonorityClass(sc.Sonority, sc.DomainSoundClass)))
				: new SimpleSyllabifier(_combineVowels, _combineConsonants);
			_projectService.Project.VarietyProcessors["syllabifier"] = syllabifier;

			_analysisService.SegmentAll();
			return syllabifier;
		}
	}
}
