using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using GalaSoft.MvvmLight;
using ObservableObject = SIL.ObjectModel.ObservableObject;

namespace SIL.Cog.Application.ViewModels
{
	public class WrapperViewModel : ViewModelBase
	{
		/// <summary>
		/// Stores all properties of the object we are wrapping
		/// </summary>
		private static readonly Dictionary<Type, Collection<string>> PropertyDictionary;

		/// <summary>
		/// Initialises the Property Dictionary
		/// </summary>
		static WrapperViewModel()
		{
			PropertyDictionary = new Dictionary<Type, Collection<string>>();
		}

		private readonly Type _type;
		private ObservableObject _wrappedObject;

		public WrapperViewModel()
			: this(null)
		{
		}

		public WrapperViewModel(ObservableObject wrappedObject)
		{
			_type = GetType();
			//if we have already added the properties for this object dont readd them
			if (!PropertyDictionary.ContainsKey(_type))
			{
				//add an entry to the dictionary
				PropertyDictionary.Add(_type, new Collection<string>());

				foreach (PropertyInfo prop in _type.GetProperties())
				{
					//only add the public properties
					if (prop.PropertyType.IsPublic)
						PropertyDictionary[_type].Add(prop.Name);
				}
			}
			WrappedObject = wrappedObject;
		}

		/// <summary>
		/// Gets and Sets the Wrapped Object
		/// </summary>
		protected ObservableObject WrappedObject
		{
			get { return _wrappedObject; }
			set
			{
				if (_wrappedObject != value)
				{
					//if we currently have one
					if (_wrappedObject != null)
					{
						//unsubscribe to notification changed
						_wrappedObject.PropertyChanging -= OnPropertyChanging;
						_wrappedObject.PropertyChanged -= OnPropertyChanged;
					}

					//assign
					_wrappedObject = value;

					if (_wrappedObject != null)
					{
						//subscribe to notification changed
						_wrappedObject.PropertyChanging += OnPropertyChanging;
						_wrappedObject.PropertyChanged += OnPropertyChanged;
					}
				}
			}
		}

		private void OnPropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			//if this object, and any that derive from it have a property by the same name as the one
			//in the NotifyPropertyChanged EventArgs, then raise Property Changed with the same PropertyName
			if (PropertyDictionary[_type].Contains(e.PropertyName))
				RaisePropertyChanging(e.PropertyName);
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//if this object, and any that derive from it have a property by the same name as the one
			//in the NotifyPropertyChanged EventArgs, then raise Property Changed with the same PropertyName
			if (PropertyDictionary[_type].Contains(e.PropertyName))
				RaisePropertyChanged(e.PropertyName);
		}
	}
}
