using System;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.ViewModels
{
	public class SoundChangeLhsViewModel : ViewModelBase, IEquatable<SoundChangeLhsViewModel>, IComparable<SoundChangeLhsViewModel>, IComparable
	{
		private readonly SoundContext _lhs;

		public SoundChangeLhsViewModel(SoundContext lhs)
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

		public bool Equals(SoundChangeLhsViewModel other)
		{
			if (other == null)
				return false;

			return _lhs.Equals(other._lhs);
		}

		public int CompareTo(SoundChangeLhsViewModel other)
		{
			return string.Compare(Target + Environment, other.Target + other.Environment, StringComparison.Ordinal);
		}

		public override bool Equals(object obj)
		{
			var lhs = obj as SoundChangeLhsViewModel;
			return obj != null && Equals(lhs);
		}

		public override int GetHashCode()
		{
			return _lhs.GetHashCode();
		}

		public int CompareTo(object obj)
		{
			if (!(obj is SoundChangeLhsViewModel))
				throw new ArgumentException("The type of the specified argument is incorrect.", "obj");
			return CompareTo((SoundChangeLhsViewModel) obj);
		}
	}
}
