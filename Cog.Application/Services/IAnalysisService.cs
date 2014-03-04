using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Services
{
	public interface IAnalysisService
	{
		void SegmentAll();
		void Segment(Variety variety);

		void StemAll(object ownerViewModel, StemmingMethod method);
		void Stem(StemmingMethod method, Variety variety);

		void CompareAll(object ownerViewModel);
		void Compare(VarietyPair varietyPair);
	}
}
