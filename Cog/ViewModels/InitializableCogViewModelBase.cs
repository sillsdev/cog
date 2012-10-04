using System;

namespace SIL.Cog.ViewModels
{
	public abstract class InitializableCogViewModelBase : CogViewModelBase
	{
		protected InitializableCogViewModelBase(string displayName)
			: base(displayName)
		{
		}

		public abstract void Initialize(CogProject project);

		public abstract bool SwitchView(Type viewType, object model);
	}
}
