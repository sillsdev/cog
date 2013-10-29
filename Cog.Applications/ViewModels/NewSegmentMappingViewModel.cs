using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Machine;

namespace SIL.Cog.Applications.ViewModels
{
	public class NewSegmentMappingViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly Segmenter _segmenter;
		private string _segment1;
		private string _segment2;

		public NewSegmentMappingViewModel(Segmenter segmenter)
		{
			_segmenter = segmenter;
		}

		public string Segment1
		{
			get { return _segment1; }
			set { Set(() => Segment1, ref _segment1, value); }
		}

		public string Segment2
		{
			get { return _segment2; }
			set { Set(() => Segment2, ref _segment2, value); }
		}

		string IDataErrorInfo.this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "Segment1":
						return GetErrorInfo(_segment1);

					case "Segment2":
						return GetErrorInfo(_segment2);
				}

				return null;
			}
		}

		private string GetErrorInfo(string segment)
		{
			if (string.IsNullOrEmpty(segment))
				return "Please specify a segment.";

			if (segment == "#")
				return "This is an invalid segment.";
			if (segment.StartsWith("#"))
				segment = segment.Remove(0, 1);
			else if (segment.EndsWith("#"))
				segment = segment.Remove(segment.Length - 1, 1);
			Shape shape;
			if (!_segmenter.TrySegment(segment, out shape))
				return "This is an invalid segment.";
			if (shape.Any(n => n.Type() != shape.First.Type()))
				return "Please specify only one segment.";
			return null;
		}

		string IDataErrorInfo.Error
		{
			get { return null; }
		}
	}
}
