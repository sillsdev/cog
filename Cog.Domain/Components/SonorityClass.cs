namespace SIL.Cog.Domain.Components
{
	public class SonorityClass
	{
		private readonly int _sonority;
		private readonly SoundClass _soundClass;

		public SonorityClass(int sonority, SoundClass soundClass)
		{
			_sonority = sonority;
			_soundClass = soundClass;
		}

		public int Sonority
		{
			get { return _sonority; }
		}

		public SoundClass SoundClass
		{
			get { return _soundClass; }
		}
	}
}
