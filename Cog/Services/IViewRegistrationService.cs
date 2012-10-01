using System.Collections.Generic;
using System.Windows;

namespace SIL.Cog.Services
{
	public interface IViewRegistrationService
	{
		IEnumerable<FrameworkElement> Views { get; } 

		void Register(FrameworkElement view);
		void Unregister(FrameworkElement view);
	}
}
