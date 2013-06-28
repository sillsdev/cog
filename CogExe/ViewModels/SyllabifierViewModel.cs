using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public class SyllabifierViewModel : ComponentSettingsViewModelBase
	{
		private bool _syllabificationEnabled;
		private readonly SoundClassesViewModel _sonorityClasses;

		public SyllabifierViewModel(IDialogService dialogService, CogProject project)
			: base("Syllabification", project)
		{
			_sonorityClasses = new SoundClassesViewModel(dialogService, project, project.Syllabifier.SonorityScale.Select(sc =>
				{
					var vm = new SoundClassViewModel(sc.SoundClass, sc.Sonority);
					vm.PropertyChanged += SonorityClassChanged;
					return vm;
				}), true);
			_sonorityClasses.SoundClasses.CollectionChanged += SonorityClassesChanged;
			_syllabificationEnabled = project.Syllabifier.Enabled;
		}

		private void SonorityClassChanged(object sender, PropertyChangedEventArgs e)
		{
			IsChanged = true;
		}

		private void SonorityClassesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		public bool SyllabificationEnabled
		{
			get { return _syllabificationEnabled; }
			set
			{
				if (Set(() => SyllabificationEnabled, ref _syllabificationEnabled, value))
					IsChanged = true;
			}
		}

		public SoundClassesViewModel SonorityClasses
		{
			get { return _sonorityClasses; }
		}

		public override object UpdateComponent()
		{
			Project.Syllabifier.Enabled = _syllabificationEnabled;
			Project.Syllabifier.SonorityScale.Clear();
			foreach (SoundClassViewModel sonorityClass in _sonorityClasses.SoundClasses)
				Project.Syllabifier.SonorityScale.Add(new SonorityClass(sonorityClass.Sonority, sonorityClass.ModelSoundClass));

			foreach (Variety variety in Project.Varieties)
				Project.Syllabifier.Syllabify(variety);

			return Project.Syllabifier;
		}
	}
}
