using System;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class SwitchViewData
	{
		private readonly Type _viewModelType;
		private readonly ReadOnlyList<object> _models;

		public SwitchViewData(Type viewModelType, params object[] models)
		{
			_viewModelType = viewModelType;
			_models = models.ToReadOnlyList();
		}

		public Type ViewModelType
		{
			get { return _viewModelType; }
		}

		public IReadOnlyList<object> Models
		{
			get { return _models; }
		}
	}
}
