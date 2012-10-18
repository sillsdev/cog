using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class VarietySenseViewModel : SenseViewModel
	{
		private readonly ObservableCollection<Word> _modelWords;
		private readonly ListViewModelCollection<ObservableCollection<Word>, WordViewModel, Word> _words;
		private readonly CogProject _project;
		private string _strRep;
		private readonly Variety _variety;

		public VarietySenseViewModel(CogProject project, Variety variety, Sense sense, IEnumerable<Word> words)
			: base(sense)
		{
			_project = project;
			_variety = variety;

			_modelWords = new ObservableCollection<Word>(words);
			_words = new ListViewModelCollection<ObservableCollection<Word>, WordViewModel, Word>(_modelWords, word => new WordViewModel(project, this, word));
			_modelWords.CollectionChanged += ModelWordsChanged;
			_strRep = _modelWords.Select(word => word.StrRep).ConcatStrings("/");
		}

		public ObservableCollection<WordViewModel> Words
		{
			get { return _words; }
		}

		public ObservableCollection<Word> ModelWords
		{
			get { return _modelWords; }
		}

		private void ModelWordsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Set("StrRep", ref _strRep, ModelWords.Select(word => word.StrRep).ConcatStrings("/"));
		}

		public string StrRep
		{
			get { return _strRep; }
			set
			{
				string val = value.Trim();
				var wordsToRemove = new HashSet<Word>(ModelWords);
				if (!string.IsNullOrEmpty(val))
				{
					int index = 0;
					foreach (string wordStr in val.Split('/').Select(s => s.Trim()).Distinct())
					{
						Word word = wordsToRemove.FirstOrDefault(w => w.StrRep == wordStr);
						if (word != null)
						{
							wordsToRemove.Remove(word);
							int oldIndex = ModelWords.IndexOf(word);
							if (index != oldIndex)
								ModelWords.Move(oldIndex, index);
						}
						else
						{
							Shape shape;
							if (!_project.Segmenter.ToShape(null, wordStr, null, out shape))
								shape = _project.Segmenter.EmptyShape;
							var newWord = new Word(wordStr, shape, ModelSense);
							ModelWords.Insert(index, newWord);
							_variety.Words.Add(newWord);
						}
						index++;
					}
				}

				foreach (Word wordToRemove in wordsToRemove)
					_variety.Words.Remove(wordToRemove);
				IsChanged = true;
			}
		}
	}
}
