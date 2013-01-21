using System;
using GalaSoft.MvvmLight.Messaging;

namespace SIL.Cog.ViewModels
{
	public class SwitchViewMessage : MessageBase
	{
		private readonly Type _viewModelType;
		private readonly object _model;

		public SwitchViewMessage(Type viewModelType)
			: this(viewModelType, null)
		{
		}

		public SwitchViewMessage(Type viewModelType, object model)
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
