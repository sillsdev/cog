using System;
using System.Linq;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Applications.ViewModels
{
	public class ListSimilarSegmentMappingsViewModel : ComponentSettingsViewModelBase
	{
		private readonly Segmenter _segmenter;
		private readonly SegmentMappingsViewModel _consMappings;
		private readonly SegmentMappingsViewModel _vowelMappings;
		private bool _generateDiphthongs;

		public ListSimilarSegmentMappingsViewModel(IDialogService dialogService, IImportService importService, Segmenter segmenter)
			: base("List")
		{
			_segmenter = segmenter;
			_consMappings = new SegmentMappingsViewModel(dialogService, importService, _segmenter);
			_consMappings.PropertyChanged += ChildPropertyChanged;
			_vowelMappings = new SegmentMappingsViewModel(dialogService, importService, _segmenter);
			_vowelMappings.PropertyChanged += ChildPropertyChanged;
			_generateDiphthongs = true;
		}
		
		public ListSimilarSegmentMappingsViewModel(IDialogService dialogService, IImportService importService, Segmenter segmenter, TypeSegmentMappings similarSegmentMappings)
			: base("List")
		{
			_segmenter = segmenter;
			var consMappings = (ListSegmentMappings) similarSegmentMappings.ConsonantMappings;
			_consMappings = new SegmentMappingsViewModel(dialogService, importService, _segmenter, consMappings.Mappings);
			_consMappings.PropertyChanged += ChildPropertyChanged;
			var vowelMappings = (ListSegmentMappings) similarSegmentMappings.VowelMappings;
			_vowelMappings = new SegmentMappingsViewModel(dialogService, importService, _segmenter, vowelMappings.Mappings);
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
				new ListSegmentMappings(_segmenter, _vowelMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), _generateDiphthongs),
				new ListSegmentMappings(_segmenter, _consMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), false));
		}
	}
}
