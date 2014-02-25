using System;
using System.Collections.Specialized;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.Collections
{
	public class VarietySenseViewModelCollection : MirroredBindableList<Sense, WordListsVarietySenseViewModel>
	{
		public VarietySenseViewModelCollection(IObservableList<Sense> senses, WordCollection words, Func<Sense, WordListsVarietySenseViewModel> viewModelFactory)
			: base(senses, viewModelFactory, vm => vm.DomainSense)
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
						WordListsVarietySenseViewModel vm = this[word.Sense];
						if (!vm.DomainWords.Contains(word))
							vm.DomainWords.Add(word);
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					foreach (Word word in e.OldItems)
					{
						WordListsVarietySenseViewModel vm = this[word.Sense];
						vm.DomainWords.Remove(word);
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					foreach (WordListsVarietySenseViewModel vm in this)
						vm.DomainWords.Clear();
					break;
			}
		}
	}
}
