using System.ComponentModel;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class AddUnnaturalClassSegmentViewModel : CogViewModelBase, IDataErrorInfo
	{
		private string _segment;
		private readonly CogProject _project;

		public AddUnnaturalClassSegmentViewModel(CogProject project)
			: base("Add Segment")
		{
			_project = project;
		}

		public string Segment
		{
			get { return _segment; }
			set { Set(() => Segment, ref _segment, value); }
		}

		public string this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "Segment":
						if (string.IsNullOrEmpty(_segment))
							return "Please specify a segment";
						string seg = _segment.Trim('#');
						if (seg.Length > 0 && seg != "-")
						{
							Shape shape;
							if (!_project.Segmenter.ToShape(seg, out shape))
								return "This is an invalid segment";
							if (shape.Any(n => n.Type() != shape.First.Type()))
								return "Please specify only one segment";
						}
						break;
				}

				return null;
			}
		}

		public string Error
		{
			get { return null; }
		}
	}
}
