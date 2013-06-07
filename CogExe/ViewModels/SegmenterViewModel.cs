using SIL.Cog.Services;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class SegmenterViewModel : ComponentSettingsViewModelBase
	{
		private int _maxConsonantLength;
		private int _maxVowelLength;
		private readonly IProgressService _progressService;

		public SegmenterViewModel(IProgressService progressService, CogProject project)
			: base("Segmenter", project)
		{
			_maxConsonantLength = Project.Segmenter.MaxConsonantLength;
			_maxVowelLength = Project.Segmenter.MaxVowelLength;
			_progressService = progressService;
		}

		public int MaxConsonantLength
		{
			get { return _maxConsonantLength; }
			set
			{
				if (Set(() => MaxConsonantLength, ref _maxConsonantLength, value))
					IsChanged = true;
			}
		}

		public int MaxVowelLength
		{
			get { return _maxVowelLength; }
			set
			{
				if (Set(() => MaxVowelLength, ref _maxVowelLength, value))
					IsChanged = true;
			}
		}

		public override object UpdateComponent()
		{
			if (Project.Segmenter.MaxConsonantLength == _maxConsonantLength && Project.Segmenter.MaxVowelLength == _maxVowelLength)
				return Project.Segmenter;

			Project.Segmenter.MaxConsonantLength = _maxConsonantLength;
			Project.Segmenter.MaxVowelLength = _maxVowelLength;
			_progressService.ShowProgress(UpdateAllShapes);
			return Project.Segmenter;
		}

		private void UpdateAllShapes()
		{
			foreach (Variety variety in Project.Varieties)
			{
				foreach (Affix affix in variety.Affixes)
				{
					Shape shape;
					if (!Project.Segmenter.ToShape(affix.StrRep, out shape))
						shape = Project.Segmenter.EmptyShape;
					affix.Shape = shape;
				}

				foreach (Word word in variety.Words)
				{
					Shape shape;
					if (word.Shape.Count == 0)
					{
						if (!Project.Segmenter.ToShape(word.StrRep, out shape))
							shape = Project.Segmenter.EmptyShape;
					}
					else
					{
						Annotation<ShapeNode> prefix = word.Prefix;
						Annotation<ShapeNode> suffix = word.Suffix;
						if (!Project.Segmenter.ToShape(prefix == null ? null : prefix.OriginalStrRep(), word.Stem.OriginalStrRep(), suffix == null ? null : suffix.OriginalStrRep(), out shape))
							shape = Project.Segmenter.EmptyShape;
					}
					word.Shape = shape;
				}
			}
		}
	}
}
