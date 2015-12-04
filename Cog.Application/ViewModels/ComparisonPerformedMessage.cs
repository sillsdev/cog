using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public class ComparisonPerformedMessage : MessageBase
	{
		private readonly VarietyPair _varietyPair;

		public ComparisonPerformedMessage(VarietyPair varietyPair = null)
		{
			_varietyPair = varietyPair;
		}

		public VarietyPair VarietyPair
		{
			get { return _varietyPair; }
		}
	}
}
