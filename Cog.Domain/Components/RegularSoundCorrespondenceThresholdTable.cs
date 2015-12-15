using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using SIL.Collections;

namespace SIL.Cog.Domain.Components
{
	internal class RegularSoundCorrespondenceThresholdTable
	{
		private readonly Dictionary<Tuple<int, int>, SortedList<int, int>> _thresholds;

		public RegularSoundCorrespondenceThresholdTable()
		{
			_thresholds = new Dictionary<Tuple<int, int>, SortedList<int, int>>();

			Stream tableStream = Assembly.GetAssembly(GetType()).GetManifestResourceStream("SIL.Cog.Domain.Components.RegularSoundCorrespondenceThresholdTable.bin");
			Debug.Assert(tableStream != null);
			using (var reader = new StreamReader(new DeflateStream(tableStream, CompressionMode.Decompress)))
			{
				var sigCounts = new SortedList<int, int>();
				int prevWordlistSize = -1, prevSeg1Count = -1;
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					string[] rec = line.Split('\t');
					int wordlistSize = int.Parse(rec[0]);
					int seg1Count = int.Parse(rec[1]);
					int seg2Count = int.Parse(rec[2]);
					int sigCount = int.Parse(rec[3]);

					if ((prevWordlistSize != -1 && wordlistSize != prevWordlistSize) || (prevSeg1Count != -1 && seg1Count != prevSeg1Count))
					{
						_thresholds[Tuple.Create(prevWordlistSize, prevSeg1Count)] = sigCounts;
						sigCounts = new SortedList<int, int>();
					}
					sigCounts.Add(seg2Count, sigCount);
					prevWordlistSize = wordlistSize;
					prevSeg1Count = seg1Count;
				}
				_thresholds[Tuple.Create(prevWordlistSize, prevSeg1Count)] = sigCounts;
			}
		}

		public bool TryGetThreshold(int wordListSize, int seg1Count, int seg2Count, out int threshold)
		{
			threshold = 0;
			// check if word list size is out of range for the generated table
			if (wordListSize > 1000)
				return false;

			int minCount = Math.Min(seg1Count, seg2Count);
			int maxCount = Math.Max(seg1Count, seg2Count);
			// check if segment count is out of range for the generated table
			if (minCount < 2 || maxCount > wordListSize * 0.3)
				return false;

			// round to the next highest multiple of 10
			wordListSize = (int) Math.Round(wordListSize / 10.0, MidpointRounding.AwayFromZero) * 10;
			SortedList<int, int> sigCounts;
			if (_thresholds.TryGetValue(Tuple.Create(wordListSize, minCount), out sigCounts))
			{
				int index = sigCounts.Keys.BinarySearch(maxCount);
				if (index < 0)
					index = (~index) - 1;
				// don't think this can happen, but handle it just to be safe
				if (index == -1)
					return false;
				threshold = sigCounts.Values[index];
				return true;
			}
			return false;
		}
	}
}
