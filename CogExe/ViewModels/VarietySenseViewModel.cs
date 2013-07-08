using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Components;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class VarietySenseViewModel : SenseViewModel
	{
		private readonly ObservableList<Word> _modelWords;
		private readonly ReadOnlyMirroredList<Word, WordViewModel> _words;
		private readonly CogProject _project;
		private string _strRep;
		private readonly WordListsVarietyViewModel _variety;
		private readonly ICommand _showInVarietiesCommand;

		public VarietySenseViewModel(CogProject project, WordListsVarietyViewModel variety, Sense sense, IEnumerable<Word> words)
			: base(sense)
		{
			_project = project;
			_variety = variety;

			_modelWords = new ObservableList<Word>(words);
			_words = new ReadOnlyMirroredList<Word, WordViewModel>(_modelWords, word => new WordViewModel(project, word), vm => vm.ModelWord);
			_modelWords.CollectionChanged += ModelWordsChanged;
			_strRep = string.Join("/", _modelWords.Select(word => word.StrRep));
			_showInVarietiesCommand = new RelayCommand(ShowInVarieties, () => _modelWords.Count > 0);
		}

		private void ShowInVarieties()
		{
			Messenger.Default.Send(new Message(MessageType.SwitchView, new SwitchViewData(typeof(VarietiesViewModel), _variety.ModelVariety, ModelSense)));
		}

		public ReadOnlyObservableList<WordViewModel> Words
		{
			get { return _words; }
		}

		public WordListsVarietyViewModel Variety
		{
			get { return _variety; }
		}

		public ICommand ShowInVarietiesCommand
		{
			get { return _showInVarietiesCommand; }
		}

		internal IList<Word> ModelWords
		{
			get { return _modelWords; }
		}

		private void ModelWordsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Set(() => StrRep, ref _strRep, string.Join("/", ModelWords.Select(word => word.StrRep)));
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
								_modelWords.Move(oldIndex, index);
						}
						else
						{
							var newWord = new Word(wordStr.Normalize(NormalizationForm.FormD), ModelSense);
							_modelWords.Insert(index, newWord);
							_variety.ModelVariety.Words.Add(newWord);

							var pipeline = new Pipeline<Variety>(_project.GetVarietyInitProcessors());
							pipeline.Process(_variety.ModelVariety.ToEnumerable());
						}
						index++;
					}
				}

				foreach (Word wordToRemove in wordsToRemove)
					_variety.ModelVariety.Words.Remove(wordToRemove);
				IsChanged = true;
			}
		}
	}
}
