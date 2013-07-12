using System;
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
			_consMappings.PropertyChanged += ChildPropertyChanged;
			_vowelMappings = new SegmentMappingsViewModel(dialogService, importService, project);
			_vowelMappings.PropertyChanged += ChildPropertyChanged;
			_generateDiphthongs = true;
		}
		
		public ListSimilarSegmentMappingsViewModel(IDialogService dialogService, IImportService importService, CogProject project, TypeSegmentMappings similarSegmentMappings)
			: base("List", project)
		{
			var consMappings = (ListSegmentMappings) similarSegmentMappings.ConsonantMappings;
			_consMappings = new SegmentMappingsViewModel(dialogService, importService, project, consMappings.Mappings);
			_consMappings.PropertyChanged += ChildPropertyChanged;
			var vowelMappings = (ListSegmentMappings) similarSegmentMappings.VowelMappings;
			_vowelMappings = new SegmentMappingsViewModel(dialogService, importService, project, vowelMappings.Mappings);
			_vowelMappings.PropertyChanged += ChildPropertyChanged;
			_generateDiphthongs = vowelMappings.GenerateDigraphs;
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

		public override object UpdateComponent()
		{
			return new TypeSegmentMappings(
				new ListSegmentMappings(Project, _vowelMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), _generateDiphthongs),
				new ListSegmentMappings(Project, _consMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), false));
		}
	}
}
