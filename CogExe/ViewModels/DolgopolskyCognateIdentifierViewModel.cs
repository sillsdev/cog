using System.Collections.Specialized;
using System.Linq;
using SIL.Cog.Components;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public class DolgopolskyCognateIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private int _initialEquivalenceThreshold;
		private readonly SoundClassesViewModel _soundClasses;

		public DolgopolskyCognateIdentifierViewModel(IDialogService dialogService, CogProject project)
			: base("Dolgopolsky", project)
		{
			_initialEquivalenceThreshold = 2;
			var soundClasses = new SoundClass[]
				{
					new UnnaturalClass(project.Segmenter, "K", new[] {"t͡s", "d͡z", "t͡ɕ", "d͡ʑ", "t͡ʃ", "d͡ʒ", "c", "ɟ", "t͡θ", "t͡ʂ", "d͡ʐ", "k", "g", "q", "ɢ", "ɡ", "ɠ", "x", "ɣ", "χ"}, true),
					new UnnaturalClass(project.Segmenter, "P", new[] {"ɸ", "β", "f", "p͡f", "p", "b", "ɓ"}, true),
					new UnnaturalClass(project.Segmenter, "Ø", new[] {"ʔ", "ħ", "ʕ", "h", "ɦ", "-", "#ŋ"}, true),
					new UnnaturalClass(project.Segmenter, "J", new[] {"j", "ɥ", "ɰ"}, true),
					new UnnaturalClass(project.Segmenter, "M", new[] {"m", "ɱ", "ʍ"}, true),
 					new UnnaturalClass(project.Segmenter, "N", new[] {"n", "ɳ", "ŋ", "ɴ", "ɲ"}, true),
 					new UnnaturalClass(project.Segmenter, "S", new[] {"s", "z", "ʃ", "ʒ", "ʂ", "ʐ", "ç", "ʝ", "ɕ", "ʑ", "ɧ"}, true),
					new UnnaturalClass(project.Segmenter, "R", new[] {"ɹ", "ɻ", "ʀ", "ɾ", "r", "ʁ", "ɽ", "l", "ɭ", "ʎ", "ʟ", "ɬ", "ɮ", "ɫ", "ł"}, true),
					new UnnaturalClass(project.Segmenter, "T", new[] {"t", "d", "ɗ", "ʈ", "ɖ", "θ", "ð"}, true),
 					new UnnaturalClass(project.Segmenter, "W", new[] {"w", "ʋ", "v", "ʙ"}, true)
				};
			_soundClasses = new SoundClassesViewModel(dialogService, project, soundClasses);
			_soundClasses.SoundClasses.CollectionChanged += SoundClassesChanged;
		}

		public DolgopolskyCognateIdentifierViewModel(IDialogService dialogService, CogProject project, DolgopolskyCognateIdentifier cognateIdentifier)
			: base("Dolgopolsky", project)
		{
			_initialEquivalenceThreshold = cognateIdentifier.InitialEquivalenceThreshold;
			_soundClasses = new SoundClassesViewModel(dialogService, project, cognateIdentifier.SoundClasses);
			_soundClasses.SoundClasses.CollectionChanged += SoundClassesChanged;
		}

		private void SoundClassesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		public int InitialEquivalenceThreshold
		{
			get { return _initialEquivalenceThreshold; }
			set
			{
				Set(() => InitialEquivalenceThreshold, ref _initialEquivalenceThreshold, value);
				IsChanged = true;
			}
		}

		public SoundClassesViewModel SoundClasses
		{
			get { return _soundClasses; }
		}

		public override object UpdateComponent()
		{
			var cognateIdentifier = new DolgopolskyCognateIdentifier(Project, _soundClasses.SoundClasses.Select(nc => nc.ModelSoundClass),
				_initialEquivalenceThreshold, "primary");
			Project.VarietyPairProcessors["cognateIdentifier"] = cognateIdentifier;
			return cognateIdentifier;
		}
	}
}
