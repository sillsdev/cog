using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Threading;

namespace SIL.Cog.ViewModels
{
	public class VarietySenseViewModelCollection : ListViewModelCollection<ObservableCollection<Sense>, VarietySenseViewModel, Sense>
	{
		public VarietySenseViewModelCollection(ObservableCollection<Sense> senses, WordCollection words, Func<Sense, VarietySenseViewModel> viewModelFactory)
			: base(senses, viewModelFactory)
		{
			words.CollectionChanged += WordsChanged;
		}

		private void WordsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() =>
				{
					switch (e.Action)
					{
						case NotifyCollectionChangedAction.Add:
							foreach (Word word in e.NewItems)
							{
								VarietySenseViewModel vm = this.Single(v => v.ModelSense == word.Sense);
								if (!vm.ModelWords.Contains(word))
									vm.ModelWords.Add(word);
							}
							break;

						case NotifyCollectionChangedAction.Remove:
							foreach (Word word in e.OldItems)
							{
								VarietySenseViewModel vm = this.Single(v => v.ModelSense == word.Sense);
								vm.ModelWords.Remove(word);
							}
							break;

						case NotifyCollectionChangedAction.Reset:
							foreach (VarietySenseViewModel vm in this)
								vm.ModelWords.Clear();
							break;
					}
				});
		}
	}
}
