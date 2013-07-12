using System;
using GalaSoft.MvvmLight.Messaging;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	internal class SwitchViewMessage : MessageBase
	{
		private readonly Type _viewModelType;
		private readonly ReadOnlyList<object> _models;

		public SwitchViewMessage(Type viewModelType, params object[] models)
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
