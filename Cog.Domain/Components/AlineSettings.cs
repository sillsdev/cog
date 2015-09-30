namespace SIL.Cog.Domain.Components
{
	public sealed class AlineSettings : WordPairAlignerSettings
	{
		private bool _soundChangeScoringEnabled = true;
		private bool _syllablePositionCostEnabled = true;

		public bool SoundChangeScoringEnabled 
		{
			get { return _soundChangeScoringEnabled; }
			set
			{
				CheckReadOnly();
				_soundChangeScoringEnabled = value;
			}
		}

		public bool SyllablePositionCostEnabled
		{
			get { return _syllablePositionCostEnabled; }
			set
			{
				CheckReadOnly();
				_syllablePositionCostEnabled = value;
			}
		}
	}
}
