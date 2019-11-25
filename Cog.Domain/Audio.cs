namespace SIL.Cog.Domain
{
	public class Audio
	{
		public Audio(string fileName, int startOffset = 0, int endOffset = -1)
		{
			FileName = fileName;
			StartOffset = startOffset;
			EndOffset = endOffset;
		}

		public string FileName { get; }
		public int StartOffset { get; }
		public int EndOffset { get; }
	}
}
