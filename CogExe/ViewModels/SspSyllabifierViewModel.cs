using System.Collections.Generic;
using System.Linq;
using SIL.Cog.Components;
using SIL.Cog.Services;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class SspSyllabifierViewModel : ComponentSettingsViewModelBase
	{
		private bool _syllabificationEnabled;
		private readonly SoundClassesViewModel _sonorityClasses;

		public SspSyllabifierViewModel(IDialogService dialogService, CogProject project, SspSyllabifier syllabifier)
			: base("Syllabification", project)
		{
			_sonorityClasses = CreateSonorityClasses(dialogService, project, syllabifier.SonorityScale);
			_syllabificationEnabled = true;
		}

		public SspSyllabifierViewModel(IDialogService dialogService, CogProject project)
			: base("Syllabification", project)
		{
			var sonorityScale = new[]
				{
					new SonorityClass(1, new NaturalClass("Stop", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("stop").Symbol("nasal-").Value)),
					new SonorityClass(2, new NaturalClass("Affricate", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("affricate").Value)),
					new SonorityClass(3, new NaturalClass("Fricative", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("fricative").Symbol("lateral-").Value)), 
					new SonorityClass(4, new NaturalClass("Nasal", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("nasal+").Value)),
					new SonorityClass(5, new NaturalClass("Trill", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("trill").Value)),
					new SonorityClass(6, new NaturalClass("Lateral", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("lateral+").Value)),
					new SonorityClass(7, new NaturalClass("Flap", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.ConsonantType).Symbol("flap").Value)),
					new SonorityClass(8, new UnnaturalClass("Glide", new[] {"j", "ɥ", "ɰ", "w"}, true, Project.Segmenter)),
					new SonorityClass(8, new NaturalClass("Non-syllabic vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("syllabic-").Value)),
					new SonorityClass(9, new NaturalClass("Close vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("close-vowel").Symbol("syllabic+").Value)),
					new SonorityClass(10, new NaturalClass("Mid vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("mid-vowel").Symbol("syllabic+").Value)),
					new SonorityClass(11, new NaturalClass("Open vowel", FeatureStruct.New(project.FeatureSystem).Symbol(CogFeatureSystem.VowelType).Symbol("open-vowel").Symbol("syllabic+").Value))
				};
			_sonorityClasses = CreateSonorityClasses(dialogService, project, sonorityScale);
			_syllabificationEnabled = false;
		}

		private SoundClassesViewModel CreateSonorityClasses(IDialogService dialogService, CogProject project, IEnumerable<SonorityClass> sonorityScale)
		{
			var soundClasses = new SoundClassesViewModel(dialogService, project, sonorityScale.Select(sc => new SoundClassViewModel(sc.SoundClass, sc.Sonority)), true);
			soundClasses.PropertyChanged += ChildPropertyChanged;
			return soundClasses;
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
				syllabifier = new SspSyllabifier(_sonorityClasses.SoundClasses.Select(sc => new SonorityClass(sc.Sonority, sc.ModelSoundClass)));
				Project.VarietyProcessors["syllabifier"] = syllabifier;
			}
			else
			{
				syllabifier = null;
				Project.VarietyProcessors.Remove("syllabifier");
			}

			var pipeline = new MultiThreadedPipeline<Variety>(Project.GetVarietyInitProcessors());
			pipeline.Process(Project.Varieties);
			pipeline.WaitForComplete();

			return syllabifier;
		}
	}
}
