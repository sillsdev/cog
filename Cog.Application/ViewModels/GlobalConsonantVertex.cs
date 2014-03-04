namespace SIL.Cog.Application.ViewModels
{
	public class GlobalConsonantVertex : GlobalSegmentVertex
	{
		private readonly ConsonantPlace _place;
		private readonly ConsonantManner _manner;
		private readonly bool _voiced;

		public GlobalConsonantVertex(ConsonantPlace place, ConsonantManner manner, bool voiced)
		{
			_place = place;
			_manner = manner;
			_voiced = voiced;
		}

		public ConsonantPlace Place
		{
			get { return _place; }
		}

		public ConsonantManner Manner
		{
			get { return _manner; }
		}

		public bool Voiced
		{
			get { return _voiced; }
		}
	}
}
