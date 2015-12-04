using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public class PerformingComparisonMessage : MessageBase
	{
		private readonly VarietyPair _varietyPair;

		public PerformingComparisonMessage(VarietyPair varietyPair = null)
		{
			_varietyPair = varietyPair;
		}

		public VarietyPair VarietyPair
		{
			get { return _varietyPair; }
		}
	}
}
