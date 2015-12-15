using NUnit.Framework;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Domain.Tests.Components
{
	[TestFixture]
	public class RegularSoundCorrespondenceThresholdTableTests
	{
		private readonly RegularSoundCorrespondenceThresholdTable _table = new RegularSoundCorrespondenceThresholdTable();

		[Test]
		public void TryGetThreshold_WordListSizeInTableSegmentCountsInTable_ReturnsTrue()
		{
			int threshold;
			Assert.That(_table.TryGetThreshold(100, 10, 10, out threshold), Is.True);
			Assert.That(threshold, Is.EqualTo(4));
		}

		[Test]
		public void TryGetThreshold_WordListSizeNotInTableButInRangeSegmentCountsInTable_ReturnsTrue()
		{
			int threshold;
			Assert.That(_table.TryGetThreshold(95, 10, 10, out threshold), Is.True);
			Assert.That(threshold, Is.EqualTo(4));
		}

		[Test]
		public void TryGetThreshold_WordListSizeInTableSegmentCountsNotInTableButInRange_ReturnsTrue()
		{
			int threshold;
			Assert.That(_table.TryGetThreshold(100, 10, 11, out threshold), Is.True);
			Assert.That(threshold, Is.EqualTo(4));
		}

		[Test]
		public void TryGetThreshold_WordListSizeInTableSegmentCountsOutOfRange_ReturnsFalse()
		{
			int threshold;
			Assert.That(_table.TryGetThreshold(100, 10, 31, out threshold), Is.False);
			Assert.That(_table.TryGetThreshold(100, 10, 1, out threshold), Is.False);
		}

		[Test]
		public void TryGetThreshold_WordListSizeOutOfRange_ReturnsFalse()
		{
			int threshold;
			Assert.That(_table.TryGetThreshold(1001, 10, 10, out threshold), Is.False);
		}
	}
}
