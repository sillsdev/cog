using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using SIL.Extensions;
using SIL.ObjectModel;

namespace SIL.Cog.Application.ViewModels
{
	public class SwitchViewMessage : MessageBase
	{
		private readonly Type _viewModelType;
		private readonly ReadOnlyList<object> _domainModels;

		public SwitchViewMessage(Type viewModelType, params object[] models)
		{
			_viewModelType = viewModelType;
			_domainModels = models.ToReadOnlyList();
		}

		public Type ViewModelType
		{
			get { return _viewModelType; }
		}

		public IReadOnlyList<object> DomainModels
		{
			get { return _domainModels; }
		}
	}
}
