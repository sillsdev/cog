using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class VarietySenseViewModel : SenseViewModel
	{
		private readonly ObservableList<Word> _domainWords;
		private readonly ReadOnlyMirroredList<Word, WordViewModel> _words;
		private readonly CogProject _project;
		private string _strRep;
		private readonly WordListsVarietyViewModel _variety;
		private readonly ICommand _showInVarietiesCommand;
		private readonly IBusyService _busyService;

		public VarietySenseViewModel(IBusyService busyService, CogProject project, WordListsVarietyViewModel variety, Sense sense, IEnumerable<Word> words)
			: base(sense)
		{
			_busyService = busyService;
			_project = project;
			_variety = variety;

			_domainWords = new ObservableList<Word>(words);
			_words = new ReadOnlyMirroredList<Word, WordViewModel>(_domainWords, word => new WordViewModel(busyService, project, word), vm => vm.DomainWord);
			_domainWords.CollectionChanged += DomainWordsChanged;
			_strRep = string.Join("/", _domainWords.Select(word => word.StrRep));
			_showInVarietiesCommand = new RelayCommand(ShowInVarieties, () => _domainWords.Count > 0);
		}

		private void ShowInVarieties()
		{
			Messenger.Default.Send(new SwitchViewMessage(typeof(VarietiesViewModel), _variety.DomainVariety, DomainSense));
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

		internal IList<Word> DomainWords
		{
			get { return _domainWords; }
		}

		private void DomainWordsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Set(() => StrRep, ref _strRep, string.Join("/", DomainWords.Select(word => word.StrRep)));
		}

		public string StrRep
		{
			get { return _strRep; }
			set
			{
				string val = value.Trim();
				if (_strRep != val)
				{
					_busyService.ShowBusyIndicatorUntilUpdated();
					Messenger.Default.Send(new DomainModelChangingMessage());
					var wordsToRemove = new HashSet<Word>(DomainWords);
					if (!string.IsNullOrEmpty(val))
					{
						int index = 0;
						foreach (string wordStr in val.Split('/').Select(s => s.Trim()).Distinct())
						{
							Word word = wordsToRemove.FirstOrDefault(w => w.StrRep == wordStr);
							if (word != null)
							{
								wordsToRemove.Remove(word);
								int oldIndex = DomainWords.IndexOf(word);
								if (index != oldIndex)
									_domainWords.Move(oldIndex, index);
							}
							else
							{
								var newWord = new Word(wordStr.Normalize(NormalizationForm.FormD), DomainSense);
								_domainWords.Insert(index, newWord);
								_variety.DomainVariety.Words.Add(newWord);
							}
							index++;
						}
					}

					foreach (Word wordToRemove in wordsToRemove)
						_variety.DomainVariety.Words.Remove(wordToRemove);

					var pipeline = new Pipeline<Variety>(_project.GetVarietyInitProcessors());
					pipeline.Process(_variety.DomainVariety.ToEnumerable());
				}
			}
		}
	}
}
