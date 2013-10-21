using System;
using System.Linq;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Applications.ViewModels
{
	public class ListSimilarSegmentMappingsViewModel : ComponentSettingsViewModelBase
	{
		public delegate ListSimilarSegmentMappingsViewModel Factory(SoundType soundType);

		private readonly IProjectService _projectService;
		private readonly SegmentMappingsViewModel _mappings;
		private readonly SoundType _soundType;
		private bool _generateDigraphs;
		
		public ListSimilarSegmentMappingsViewModel(IProjectService projectService, SegmentMappingsViewModel mappings, SoundType soundType)
			: base("List")
		{
			_projectService = projectService;
			_mappings = mappings;
			_mappings.PropertyChanged += ChildPropertyChanged;
			_soundType = soundType;
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			_mappings.AcceptChanges();
		}

		public SegmentMappingsViewModel Mappings
		{
			get { return _mappings; }
		}

		public bool GenerateDigraphs
		{
			get { return _generateDigraphs; }
			set { SetChanged(() => GenerateDigraphs, ref _generateDigraphs, value); }
		}

		public SoundType SoundType
		{
			get { return _soundType; }
		}

		public ListSegmentMappings SegmentMappings { get; set; }

		public override void Setup()
		{
			_mappings.SelectedMapping = null;
			_mappings.Mappings.Clear();

			if (SegmentMappings == null)
			{
				Set(() => GenerateDigraphs, ref _generateDigraphs, _soundType == SoundType.Vowel);
			}
			else
			{
				foreach (Tuple<string, string> mapping in SegmentMappings.Mappings)
					_mappings.Mappings.Add(new SegmentMappingViewModel(_projectService.Project.Segmenter, mapping.Item1, mapping.Item2));
				Set(() => GenerateDigraphs, ref _generateDigraphs, SegmentMappings.GenerateDigraphs);
			}
		}

		public override object UpdateComponent()
		{
			return new ListSegmentMappings(_projectService.Project.Segmenter, _mappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), _generateDigraphs);
		}
	}
}
