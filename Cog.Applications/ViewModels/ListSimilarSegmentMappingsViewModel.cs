using System;
using System.Linq;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Applications.ViewModels
{
	public class ListSimilarSegmentMappingsViewModel : ComponentSettingsViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly SegmentMappingsViewModel _consMappings;
		private readonly SegmentMappingsViewModel _vowelMappings;
		private bool _generateDiphthongs;
		
		public ListSimilarSegmentMappingsViewModel(IProjectService projectService, SegmentMappingsViewModel consMappings, SegmentMappingsViewModel vowelMappings)
			: base("List")
		{
			_projectService = projectService;
			_consMappings = consMappings;
			_consMappings.PropertyChanged += ChildPropertyChanged;
			_vowelMappings = vowelMappings;
			_vowelMappings.PropertyChanged += ChildPropertyChanged;
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			_consMappings.AcceptChanges();
			_vowelMappings.AcceptChanges();
		}

		public SegmentMappingsViewModel ConsonantMappings
		{
			get { return _consMappings; }
		}

		public SegmentMappingsViewModel VowelMappings
		{
			get { return _vowelMappings; }
		}

		public bool GenerateDiphthongs
		{
			get { return _generateDiphthongs; }
			set { SetChanged(() => GenerateDiphthongs, ref _generateDiphthongs, value); }
		}

		public TypeSegmentMappings SegmentMappings { get; set; }

		public override void Setup()
		{
			_consMappings.SelectedMapping = null;
			_consMappings.Mappings.Clear();
			_vowelMappings.SelectedMapping = null;
			_vowelMappings.Mappings.Clear();

			if (SegmentMappings == null || !(SegmentMappings.VowelMappings is ListSegmentMappings))
			{
				Set(() => GenerateDiphthongs, ref _generateDiphthongs, true);
			}
			else
			{
				var consMappings = (ListSegmentMappings) SegmentMappings.ConsonantMappings;
				foreach (Tuple<string, string> mapping in consMappings.Mappings)
					_consMappings.Mappings.Add(new SegmentMappingViewModel(_projectService.Project.Segmenter, mapping.Item1, mapping.Item2));
				var vowelMappings = (ListSegmentMappings) SegmentMappings.VowelMappings;
				foreach (Tuple<string, string> mapping in vowelMappings.Mappings)
					_vowelMappings.Mappings.Add(new SegmentMappingViewModel(_projectService.Project.Segmenter, mapping.Item1, mapping.Item2));
				Set(() => GenerateDiphthongs, ref _generateDiphthongs, vowelMappings.GenerateDigraphs);
			}
		}

		public override object UpdateComponent()
		{
			return new TypeSegmentMappings(
				new ListSegmentMappings(_projectService.Project.Segmenter, _vowelMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), _generateDiphthongs),
				new ListSegmentMappings(_projectService.Project.Segmenter, _consMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), false));
		}
	}
}
