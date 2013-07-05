using System;

namespace SIL.Cog.ViewModels
{
	public class SwitchViewData
	{
		private readonly Type _viewModelType;
		private readonly object _model;

		public SwitchViewData(Type viewModelType)
			: this(viewModelType, null)
		{
		}

		public SwitchViewData(Type viewModelType, object model)
		{
			_viewModelType = viewModelType;
			_model = model;
		}

		public Type ViewModelType
		{
			get { return _viewModelType; }
		}

		public object Model
		{
			get { return _model; }
		}
	}
}
