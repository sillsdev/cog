using System.ComponentModel;
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
						string seg = _segment == null ? null : _segment.Trim('#');
						if (string.IsNullOrEmpty(seg))
							return "Please specify a segment";
						if (seg != "-")
						{
							Shape shape;
							if (!_project.Segmenter.ToShape(seg, out shape))
								return "This is an invalid segment";
							if (shape.Count > 1)
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
