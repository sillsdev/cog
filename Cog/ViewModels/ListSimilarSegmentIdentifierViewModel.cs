using System;
using System.Collections.Specialized;
using System.Linq;
using SIL.Cog.Processors;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public class ListSimilarSegmentIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private readonly SimilarSegmentMappingsViewModel _consMappings;
		private readonly SimilarSegmentMappingsViewModel _vowelMappings;
		private bool _generateDiphthongs;

		public ListSimilarSegmentIdentifierViewModel(IDialogService dialogService, IImportService importService, CogProject project)
			: base("List", project)
		{
			_consMappings = new SimilarSegmentMappingsViewModel(dialogService, importService, project);
			_consMappings.Mappings.CollectionChanged += MappingsChanged;
			_vowelMappings = new SimilarSegmentMappingsViewModel(dialogService, importService, project);
			_vowelMappings.Mappings.CollectionChanged += MappingsChanged;
			_generateDiphthongs = true;
		}

		private void MappingsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}
		
		public ListSimilarSegmentIdentifierViewModel(IDialogService dialogService, IImportService importService, CogProject project, ListSimilarSegmentIdentifier similarSegmentIdentifier)
			: base("List", project)
		{
			_consMappings = new SimilarSegmentMappingsViewModel(dialogService, importService, project, similarSegmentIdentifier.ConsonantMappings);
			_consMappings.Mappings.CollectionChanged += MappingsChanged;
			_vowelMappings = new SimilarSegmentMappingsViewModel(dialogService, importService, project, similarSegmentIdentifier.VowelMappings);
			_vowelMappings.Mappings.CollectionChanged += MappingsChanged;
			_generateDiphthongs = similarSegmentIdentifier.GenerateDiphthongs;
		}

		public SimilarSegmentMappingsViewModel ConsonantMappings
		{
			get { return _consMappings; }
		}

		public SimilarSegmentMappingsViewModel VowelMappings
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

		public override void UpdateComponent()
		{
			Project.VarietyPairProcessors["similarSegmentIdentifier"] = new ListSimilarSegmentIdentifier(Project,
				_vowelMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)),
				_consMappings.Mappings.Select(m => Tuple.Create(m.Segment1, m.Segment2)), _generateDiphthongs);
		}
	}
}
