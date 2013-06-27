using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Threading;
using SIL.Collections;

namespace SIL.Cog.Views
{
	public class ConcurrentList<T> : ReadOnlyMirroredList<T, T>
	{
		public ConcurrentList(IObservableList<T> source)
			: base(source, item => item, item => item)
		{
		}

		protected override void MirrorInsert(int index, IEnumerable<T> items)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() => base.MirrorInsert(index, items));
		}

		protected override void MirrorReplace(int index, int count, IEnumerable<T> items)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() => base.MirrorReplace(index, count, items));
		}

		protected override void MirrorMove(int oldIndex, int count, int newIndex)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() => base.MirrorMove(oldIndex, count, newIndex));
		}

		protected override void MirrorRemove(int index, int count)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() => base.MirrorRemove(index, count));
		}

		protected override void MirrorReset(IEnumerable<T> source)
		{
			T[] sourceArray = source.ToArray();
			DispatcherHelper.CheckBeginInvokeOnUI(() => base.MirrorReset(sourceArray));
		}
	}
}
