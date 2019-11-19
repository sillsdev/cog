using System;
using System.Collections.Specialized;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.ObjectModel;

namespace SIL.Cog.Application.Collections
{
	public class VarietyMeaningViewModelCollection : MirroredBindableList<Meaning, WordListsVarietyMeaningViewModel>
	{
		public VarietyMeaningViewModelCollection(IObservableList<Meaning> meanings, WordCollection words, Func<Meaning, WordListsVarietyMeaningViewModel> viewModelFactory)
			: base(meanings, viewModelFactory, vm => vm.DomainMeaning)
		{
			words.CollectionChanged += WordsChanged;
		}

		private void WordsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (Word word in e.NewItems)
					{
						WordListsVarietyMeaningViewModel vm = this[word.Meaning];
						if (!vm.DomainWords.Contains(word))
							vm.DomainWords.Add(word);
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					foreach (Word word in e.OldItems)
					{
						WordListsVarietyMeaningViewModel vm = this[word.Meaning];
						vm.DomainWords.Remove(word);
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					foreach (WordListsVarietyMeaningViewModel vm in this)
						vm.DomainWords.Clear();
					break;
			}
		}
	}
}
