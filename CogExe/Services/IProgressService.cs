using System;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Services
{
	public interface IProgressService
	{
		bool ShowProgress(object ownerViewModel, ProgressViewModel progressViewModel);
		void ShowProgress(Action action);
	}
}
