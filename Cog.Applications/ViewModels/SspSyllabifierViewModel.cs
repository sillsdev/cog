using System.Collections.Generic;
using System.Linq;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.ViewModels
{
	public class SspSyllabifierViewModel : ComponentSettingsViewModelBase
	{
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private readonly IAnalysisService _analysisService;
		private bool _syllabificationEnabled;
		private readonly SoundClassesViewModel _sonorityClasses;

		public SspSyllabifierViewModel(SegmentPool segmentPool, IProjectService projectService, IAnalysisService analysisService, SoundClassesViewModel sonorityClasses)
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
			IProcessor<Variety> processor;
			if (project.VarietyProcessors.TryGetValue("syllabifier", out processor))
			{
				var syllabifier = (SspSyllabifier) processor;
				sonorityScale = syllabifier.SonorityScale;
				Set(() => SyllabificationEnabled, ref _syllabificationEnabled, true);
			}
			else
			{
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
						new SonorityClass(9, new NaturalClass("Close vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("close-vowel").Symbol("syllabic+").Value)),
						new SonorityClass(10, new NaturalClass("Mid vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("mid-vowel").Symbol("syllabic+").Value)),
						new SonorityClass(11, new NaturalClass("Open vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("open-vowel").Symbol("syllabic+").Value))
					};
				Set(() => SyllabificationEnabled, ref _syllabificationEnabled, false);
			}

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

		public bool SyllabificationEnabled
		{
			get { return _syllabificationEnabled; }
			set { SetChanged(() => SyllabificationEnabled, ref _syllabificationEnabled, value); }
		}

		public SoundClassesViewModel SonorityClasses
		{
			get { return _sonorityClasses; }
		}

		public override object UpdateComponent()
		{
			SspSyllabifier syllabifier;
			if (_syllabificationEnabled)
			{
				syllabifier = new SspSyllabifier(_segmentPool, _sonorityClasses.SoundClasses.Select(sc => new SonorityClass(sc.Sonority, sc.DomainSoundClass)));
				_projectService.Project.VarietyProcessors["syllabifier"] = syllabifier;
			}
			else
			{
				syllabifier = null;
				_projectService.Project.VarietyProcessors.Remove("syllabifier");
			}

			_analysisService.SegmentAll();
			return syllabifier;
		}
	}
}
