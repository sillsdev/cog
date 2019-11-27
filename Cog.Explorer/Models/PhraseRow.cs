using System.Collections.Generic;

namespace SIL.Cog.Explorer.Models
{
	public class PhraseRow
	{
		public string Phrase { get; set; }
		public string LeftContext { get; set; }
		public string RightContext { get; set; }
		public Dictionary<string, string> Segments { get; set; }
		public string Gloss { get; set; }
		public AudioSegment AudioSegment { get; set; }
		public string Participants { get; set; }
	}
}
