using System;
using System.Collections.Specialized;
using System.Linq;
using SIL.Cog.Components;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public class ListSimilarSegmentMappingsViewModel : ComponentSettingsViewModelBase
	{
		private readonly SegmentMappingsViewModel _consMappings;
		private readonly SegmentMappingsViewModel _vowelMappings;
		private bool _generateDiphthongs;

		public ListSimilarSegmentMappingsViewModel(IDialogService dialogService, IImportService importService, CogProject project)
			: base("List", project)
		{
			_consMappings = new SegmentMappingsViewModel(dialogService, importService, project);
			_consMappings.Mappings.CollectionChanged += MappingsChanged;
			_vowelMappings = new SegmentMappingsViewModel(dialogService, importService, project);
			_vowelMappings.Mappings.CollectionChanged += MappingsChanged;
			_generateDiphthongs = true;
		}

		private void MappingsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}
		
		public ListSimilarSegmentMappingsViewModel(IDialogService dialogService, IImportService importService, CogProject project, TypeSegmentMappings similarSegmentMappings)
			: base("List", project)
		{
			var consMappings = (ListSegmentMappings) similarSegmentMappings.ConsonantMappings;
			_consMappings = new SegmentMappingsViewModel(dialogService, importService, project, consMappings.Mappings);
			_consMappings.Mappings.CollectionChanged += MappingsChanged;
			var vowelMappings = (ListSegmentMappings) similarSegmentMappings.VowelMappings;
			_vowelMappings = new SegmentMappingsViewModel(dialogService, importService, project, vowelMappings.Mappings);
			_vowelMappings.Mappings.CollectionChanged += MappingsChanged;
			_generateDiphthongs = vowelMappings.GenerateDigraphs;
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
			set
			{
				Set(() => GenerateDiphthongs, ref _generateDiphthongs, value);
				IsChanged = true;
			}
		}

		public override object UpdateComponent()
		{
			return new TypeSegmentMappings(
				new ListSegmentMappings(Project, _vowelMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), _generateDiphthongs),
				new ListSegmentMappings(Project, _consMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), false));
		}
	}
}
