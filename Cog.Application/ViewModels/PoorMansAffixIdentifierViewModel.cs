using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.ViewModels
{
	public class PoorMansAffixIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly SegmentPool _segmentPool;
		private int _threshold;
		private int _maxAffixLength;

		public PoorMansAffixIdentifierViewModel(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, IProjectService projectService)
			: base("Automatic stemmer")
		{
			_spanFactory = spanFactory;
			_segmentPool = segmentPool;
			_projectService = projectService;
		}

		public override void Setup()
		{
			var identifier = (PoorMansAffixIdentifier) _projectService.Project.VarietyProcessors[ComponentIdentifiers.AffixIdentifier];
			Set(() => Threshold, ref _threshold, (int) identifier.Threshold);
			Set(() => MaxAffixLength, ref _maxAffixLength, identifier.MaxAffixLength);
		}

		public int Threshold
		{
			get { return _threshold; }
			set { SetChanged(() => Threshold, ref _threshold, value); }
		}

		public int MaxAffixLength
		{
			get { return _maxAffixLength; }
			set { SetChanged(() => MaxAffixLength, ref _maxAffixLength, value); }
		}

		public override object UpdateComponent()
		{
			var affixIdentifier = new PoorMansAffixIdentifier(_spanFactory, _segmentPool, _threshold, _maxAffixLength);
			_projectService.Project.VarietyProcessors[ComponentIdentifiers.AffixIdentifier] = affixIdentifier;
			return affixIdentifier;
		}
	}
}
