using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Services
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
