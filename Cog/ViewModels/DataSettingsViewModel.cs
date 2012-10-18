using SIL.Cog.Processors;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class DataSettingsViewModel : SettingsWorkspaceViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 

		public DataSettingsViewModel(SpanFactory<ShapeNode> spanFactory)
		{
			_spanFactory = spanFactory;
		}

		protected override void CreateComponents()
		{
			Components.Add(new UnsupervisedAffixIdentifierViewModel(_spanFactory, Project, (UnsupervisedAffixIdentifier) Project.VarietyProcessors["affixIdentifier"]));
		}
	}
}
