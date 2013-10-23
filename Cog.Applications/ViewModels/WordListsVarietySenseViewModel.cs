using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordListsVarietySenseViewModel : SenseViewModel
	{
		public delegate WordListsVarietySenseViewModel Factory(WordListsVarietyViewModel variety, Sense sense);

		private readonly ObservableList<Word> _domainWords;
		private readonly ReadOnlyMirroredList<Word, WordViewModel> _words;
		private string _strRep;
		private readonly WordListsVarietyViewModel _variety;
		private readonly ICommand _showInVarietiesCommand;
		private readonly IBusyService _busyService;
		private readonly IAnalysisService _analysisService;

		public WordListsVarietySenseViewModel(IBusyService busyService, IAnalysisService analysisService, WordViewModel.Factory wordFactory, WordListsVarietyViewModel variety, Sense sense)
			: base(sense)
		{
			_busyService = busyService;
			_analysisService = analysisService;
			_variety = variety;

			_domainWords = new ObservableList<Word>(variety.DomainVariety.Words[sense]);
			_words = new ReadOnlyMirroredList<Word, WordViewModel>(_domainWords, word => wordFactory(word), vm => vm.DomainWord);
			_domainWords.CollectionChanged += DomainWordsChanged;
			_strRep = string.Join(",", _domainWords.Select(word => word.StrRep));
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
			Set(() => StrRep, ref _strRep, string.Join(",", DomainWords.Select(word => word.StrRep)));
		}

		public string StrRep
		{
			get { return _strRep; }
			set
			{
				string val = value == null ? "" : value.Trim();
				if (_strRep != val)
				{
					_busyService.ShowBusyIndicatorUntilFinishDrawing();
					var wordsToRemove = new HashSet<Word>(DomainWords);
					if (!string.IsNullOrEmpty(val))
					{
						int index = 0;
						foreach (string wordStr in val.Split(',').Select(s => s.Trim()).Distinct())
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
								var newWord = new Word(wordStr, DomainSense);
								_domainWords.Insert(index, newWord);
								_variety.DomainVariety.Words.Add(newWord);
							}
							index++;
						}
					}

					foreach (Word wordToRemove in wordsToRemove)
						_variety.DomainVariety.Words.Remove(wordToRemove);

					_analysisService.Segment(_variety.DomainVariety);
					Messenger.Default.Send(new DomainModelChangedMessage(true));
				}
			}
		}

		public override string ToString()
		{
			return StrRep;
		}
	}
}
