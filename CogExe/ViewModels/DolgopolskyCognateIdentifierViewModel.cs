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
					new UnnaturalClass("K", new[] {"t͡s", "d͡z", "t͡ɕ", "d͡ʑ", "t͡ʃ", "d͡ʒ", "c", "ɟ", "t͡θ", "t͡ʂ", "d͡ʐ", "k", "g", "q", "ɢ", "ɡ", "ɠ", "x", "ɣ", "χ"}, true, project.Segmenter),
					new UnnaturalClass("P", new[] {"ɸ", "β", "f", "p͡f", "p", "b", "ɓ"}, true, project.Segmenter),
					new UnnaturalClass("Ø", new[] {"ʔ", "ħ", "ʕ", "h", "ɦ", "-", "#ŋ"}, true, project.Segmenter),
					new UnnaturalClass("J", new[] {"j", "ɥ", "ɰ"}, true, project.Segmenter),
					new UnnaturalClass("M", new[] {"m", "ɱ", "ʍ"}, true, project.Segmenter),
 					new UnnaturalClass("N", new[] {"n", "ɳ", "ŋ", "ɴ", "ɲ"}, true, project.Segmenter),
 					new UnnaturalClass("S", new[] {"s", "z", "ʃ", "ʒ", "ʂ", "ʐ", "ç", "ʝ", "ɕ", "ʑ", "ɧ"}, true, project.Segmenter),
					new UnnaturalClass("R", new[] {"ɹ", "ɻ", "ʀ", "ɾ", "r", "ʁ", "ɽ", "l", "ɭ", "ʎ", "ʟ", "ɬ", "ɮ", "ɫ", "ł"}, true, project.Segmenter),
					new UnnaturalClass("T", new[] {"t", "d", "ɗ", "ʈ", "ɖ", "θ", "ð"}, true, project.Segmenter),
 					new UnnaturalClass("W", new[] {"w", "ʋ", "v", "ʙ"}, true, project.Segmenter)
				};
			_soundClasses = new SoundClassesViewModel(dialogService, project, soundClasses.Select(sc => new SoundClassViewModel(sc)), false);
			_soundClasses.PropertyChanged += ChildPropertyChanged;
		}

		public DolgopolskyCognateIdentifierViewModel(IDialogService dialogService, CogProject project, DolgopolskyCognateIdentifier cognateIdentifier)
			: base("Dolgopolsky", project)
		{
			_initialEquivalenceThreshold = cognateIdentifier.InitialEquivalenceThreshold;
			_soundClasses = new SoundClassesViewModel(dialogService, project, cognateIdentifier.SoundClasses.Select(sc => new SoundClassViewModel(sc)), false);
			_soundClasses.PropertyChanged += ChildPropertyChanged;
		}

		public int InitialEquivalenceThreshold
		{
			get { return _initialEquivalenceThreshold; }
			set { SetChanged(() => InitialEquivalenceThreshold, ref _initialEquivalenceThreshold, value); }
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			_soundClasses.AcceptChanges();
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
