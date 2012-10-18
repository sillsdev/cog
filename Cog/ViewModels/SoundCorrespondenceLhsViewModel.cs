using System;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class SoundCorrespondenceLhsViewModel : ViewModelBase, IEquatable<SoundCorrespondenceLhsViewModel>
	{
		private readonly SoundChangeLhs _lhs;

		public SoundCorrespondenceLhsViewModel(SoundChangeLhs lhs)
		{
			_lhs = lhs;
		}

		public string Target
		{
			get { return _lhs.Target.ToString(); }
		}

		public string Environment
		{
			get
			{
				if (_lhs.LeftEnvironment != null && _lhs.RightEnvironment != null)
					return string.Format(" / [{0}] _ [{1}]", _lhs.LeftEnvironment.Name, _lhs.RightEnvironment.Name);
				if (_lhs.RightEnvironment != null)
					return string.Format(" / _ [{0}]", _lhs.RightEnvironment.Name);
				if (_lhs.LeftEnvironment != null)
					return string.Format(" / [{0}] _", _lhs.LeftEnvironment.Name);
				return "";
			}
		}

		public bool Equals(SoundCorrespondenceLhsViewModel other)
		{
			if (other == null)
				return false;

			return _lhs.Equals(other._lhs);
		}

		public override bool Equals(object obj)
		{
			var lhs = obj as SoundCorrespondenceLhsViewModel;
			return obj != null && Equals(lhs);
		}

		public override int GetHashCode()
		{
			return _lhs.GetHashCode();
		}
	}
}
