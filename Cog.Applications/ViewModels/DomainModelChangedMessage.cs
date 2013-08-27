using GalaSoft.MvvmLight.Messaging;

namespace SIL.Cog.Applications.ViewModels
{
	internal class DomainModelChangedMessage : MessageBase
	{
		private readonly bool _affectsComparison;

		public DomainModelChangedMessage(bool affectsComparison)
		{
			_affectsComparison = affectsComparison;
		}

		public bool AffectsComparison
		{
			get { return _affectsComparison; }
		}
	}
}
