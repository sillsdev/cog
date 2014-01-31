using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain.Components
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

		public bool IsMapped(ShapeNode leftNode1, Ngram<Segment> target1, ShapeNode rightNode1, ShapeNode leftNode2, Ngram<Segment> target2, ShapeNode rightNode2)
		{
			if ((target1.Length == 0 || target1.First.Type == CogFeatureSystem.VowelType) && (target2.Length == 0 || target2.First.Type == CogFeatureSystem.VowelType))
				return _vowelMappings.IsMapped(leftNode1, target1, rightNode1, leftNode2, target2, rightNode2);
			if ((target1.Length == 0 || target1.First.Type == CogFeatureSystem.ConsonantType) && (target2.Length == 0 || target2.First.Type == CogFeatureSystem.ConsonantType))
				return _consMappings.IsMapped(leftNode1, target1, rightNode1, leftNode2, target2, rightNode2);
			return false;
		}
	}
}
