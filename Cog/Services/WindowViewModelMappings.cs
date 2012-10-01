using System;
using System.Collections.Generic;
using SIL.Cog.ViewModels;
using SIL.Cog.Views;

namespace SIL.Cog.Services
{
	public class WindowViewModelMappings : IWindowViewModelMappings
	{
		private readonly IDictionary<Type, Type> _mappings;

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowViewModelMappings"/> class.
		/// </summary>
		public WindowViewModelMappings()
		{
			_mappings = new Dictionary<Type, Type>
			{
				{typeof(NewAffixViewModel), typeof(NewAffixDialog)},
				{typeof(EditVarietyViewModel), typeof(EditVarietyDialog)},
				{typeof(EditSenseViewModel), typeof(EditSenseDialog)},
				{typeof(RunStemmerViewModel), typeof(RunStemmerDialog)},
				{typeof(ProgressViewModel), typeof(ProgressDialog)}
			};
		}


		/// <summary>
		/// Gets the window type based on registered ViewModel type.
		/// </summary>
		/// <param name="viewModelType">The type of the ViewModel.</param>
		/// <returns>The window type based on registered ViewModel type.</returns>
		public Type GetWindowTypeFromViewModelType(Type viewModelType)
		{
			return _mappings[viewModelType];
		}
	}
}
