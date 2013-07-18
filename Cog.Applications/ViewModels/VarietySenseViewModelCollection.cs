using System;
using System.Collections.Specialized;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class VarietySenseViewModelCollection : ReadOnlyMirroredList<Sense, VarietySenseViewModel>
	{
		public VarietySenseViewModelCollection(IObservableList<Sense> senses, WordCollection words, Func<Sense, VarietySenseViewModel> viewModelFactory)
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
						VarietySenseViewModel vm = this[word.Sense];
						if (!vm.DomainWords.Contains(word))
							vm.DomainWords.Add(word);
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					foreach (Word word in e.OldItems)
					{
						VarietySenseViewModel vm = this[word.Sense];
						vm.DomainWords.Remove(word);
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					foreach (VarietySenseViewModel vm in this)
						vm.DomainWords.Clear();
					break;
			}
		}
	}
}
