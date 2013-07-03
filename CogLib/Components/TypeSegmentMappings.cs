namespace SIL.Cog.Components
{
	public class TypeSegmentMappings : ISegmentMappings
	{
		private readonly ISegmentMappings _vowelMappings;
		private readonly ISegmentMappings _consMappings;

		public TypeSegmentMappings(ISegmentMappings vowelMappings, ISegmentMappings consMappings)
		{
			_vowelMappings = vowelMappings;
			_consMappings = consMappings;
		}

		public ISegmentMappings VowelMappings
		{
			get { return _vowelMappings; }
		}

		public ISegmentMappings ConsonantMappings
		{
			get { return _consMappings; }
		}

		public bool IsMapped(Segment seg1, Segment seg2)
		{
			if ((seg1 == null || seg1.Type == CogFeatureSystem.VowelType) && (seg2 == null || seg2.Type == CogFeatureSystem.VowelType))
				return _vowelMappings.IsMapped(seg1, seg2);
			if ((seg1 == null || seg1.Type == CogFeatureSystem.ConsonantType) && (seg2 == null || seg2.Type == CogFeatureSystem.ConsonantType))
				return _consMappings.IsMapped(seg1, seg2);
			return false;
		}
	}
}
