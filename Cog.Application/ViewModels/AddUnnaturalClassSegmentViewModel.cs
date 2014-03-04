using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.ViewModels
{
	public class AddUnnaturalClassSegmentViewModel : ViewModelBase, IDataErrorInfo
	{
		private string _segment;
		private readonly Segmenter _segmenter;

		public AddUnnaturalClassSegmentViewModel(Segmenter segmenter)
		{
			_segmenter = segmenter;
		}

		public string Segment
		{
			get { return _segment; }
			set { Set(() => Segment, ref _segment, value); }
		}

		string IDataErrorInfo.this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "Segment":
						if (string.IsNullOrEmpty(_segment))
							return "Please specify a segment.";
						string seg = _segment.Trim('#');
						if (seg.Length > 0 && seg != "-")
						{
							Shape shape;
							if (!_segmenter.TrySegment(seg, out shape))
								return "This is an invalid segment.";
							if (shape.Any(n => n.Type() != shape.First.Type()))
								return "Please specify only one segment.";
						}
						break;
				}

				return null;
			}
		}

		string IDataErrorInfo.Error
		{
			get { return null; }
		}
	}
}
