using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Extensions;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.ViewModels
{
	public class NewSegmentMappingViewModel : ViewModelBase, IDataErrorInfo
	{
		public delegate NewSegmentMappingViewModel Factory();

		private readonly IProjectService _projectService;
		private string _segment1;
		private string _leftEnvironment1;
		private string _rightEnvironment1;
		private string _segment2;
		private string _leftEnvironment2;
		private string _rightEnvironment2;
		private bool _segmentsEnabled;
		private readonly ReadOnlyCollection<string> _environmentOptions; 

		public NewSegmentMappingViewModel(IProjectService projectService)
		{
			_projectService = projectService;
			_environmentOptions = new ReadOnlyCollection<string>(new[] {"", "#", "C", "V"});
		}

		public string Segment1
		{
			get { return _segment1; }
			set { Set(() => Segment1, ref _segment1, value); }
		}

		public string LeftEnvironment1
		{
			get { return _leftEnvironment1; }
			set { Set(() => LeftEnvironment1, ref _leftEnvironment1, value); }
		}

		public string RightEnvironment1
		{
			get { return _rightEnvironment1; }
			set { Set(() => RightEnvironment1, ref _rightEnvironment1, value); }
		}

		public string Segment2
		{
			get { return _segment2; }
			set { Set(() => Segment2, ref _segment2, value); }
		}

		public string LeftEnvironment2
		{
			get { return _leftEnvironment2; }
			set { Set(() => LeftEnvironment2, ref _leftEnvironment2, value); }
		}

		public string RightEnvironment2
		{
			get { return _rightEnvironment2; }
			set { Set(() => RightEnvironment2, ref _rightEnvironment2, value); }
		}

		public bool SegmentsEnabled
		{
			get { return _segmentsEnabled; }
			set { Set(() => SegmentsEnabled, ref _segmentsEnabled, value); }
		}

		public ReadOnlyCollection<string> EnvironmentOptions
		{
			get { return _environmentOptions; }
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

			if (!segment.IsOneOf("-", "_"))
			{
				Shape shape;
				if (!_projectService.Project.Segmenter.TrySegment(segment, out shape))
					return "This is an invalid segment.";
				if (shape.Any(n => n.Type() != shape.First.Type()))
					return "Please specify only one segment.";
			}
			return null;
		}

		string IDataErrorInfo.Error
		{
			get { return null; }
		}
	}
}
