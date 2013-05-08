using SIL.Cog.Components;
using SIL.Cog.Services;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class DataSettingsViewModel : SettingsWorkspaceViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly IProgressService _progressService;

		public DataSettingsViewModel(SpanFactory<ShapeNode> spanFactory, IProgressService progressService)
		{
			_spanFactory = spanFactory;
			_progressService = progressService;
		}

		protected override void CreateComponents()
		{
			Components.Add(new SegmenterViewModel(_progressService, Project));
			Components.Add(new UnsupervisedAffixIdentifierViewModel(_spanFactory, Project, (UnsupervisedAffixIdentifier) Project.VarietyProcessors["affixIdentifier"]));
		}
	}
}
