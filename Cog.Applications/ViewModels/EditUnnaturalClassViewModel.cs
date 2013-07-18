using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class EditUnnaturalClassViewModel : EditSoundClassViewModel
	{
		private readonly IDialogService _dialogService;
		private bool _ignoreModifiers;
		private readonly Segmenter _segmenter;
		private readonly BindableList<string> _segments;
		private string _currentSegment;
		private readonly ICommand _addSegmentCommand;
		private readonly ICommand _removeSegmentCommand;

		public EditUnnaturalClassViewModel(IDialogService dialogService, Segmenter segmenter, IEnumerable<SoundClass> soundClasses)
			: base("New Unnatural Class", soundClasses)
		{
			_dialogService = dialogService;
			_segmenter = segmenter;
			_segments = new BindableList<string>();
			_addSegmentCommand = new RelayCommand(AddSegment);
			_removeSegmentCommand = new RelayCommand(RemoveSegment, CanRemoveSegment);
		}

		public EditUnnaturalClassViewModel(IDialogService dialogService, Segmenter segmenter, IEnumerable<SoundClass> soundClasses, UnnaturalClass unnaturalClass)
			: base("Edit Unnatural Class", soundClasses, unnaturalClass)
		{
			_dialogService = dialogService;
			_segmenter = segmenter;
			_ignoreModifiers = unnaturalClass.IgnoreModifiers;
			_segments = new BindableList<string>(unnaturalClass.Segments);
			_addSegmentCommand = new RelayCommand(AddSegment);
			_removeSegmentCommand = new RelayCommand(RemoveSegment, CanRemoveSegment);
		}

		private void AddSegment()
		{
			var vm = new AddUnnaturalClassSegmentViewModel(_segmenter);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				_segments.Add(vm.Segment);
				CurrentSegment = vm.Segment;
			}
		}

		private bool CanRemoveSegment()
		{
			return _currentSegment != null;
		}

		private void RemoveSegment()
		{
			int index = _segments.IndexOf(_currentSegment);
			_segments.RemoveAt(index);
			if (_segments.Count == 0)
				CurrentSegment = null;
			else
				CurrentSegment = index >= _segments.Count ? _segments[index - 1] : _segments[index];
		}

		public string CurrentSegment
		{
			get { return _currentSegment; }
			set { Set(() => CurrentSegment, ref _currentSegment, value); }
		}

		public bool IgnoreModifiers
		{
			get { return _ignoreModifiers; }
			set { Set(() => IgnoreModifiers, ref _ignoreModifiers, value); }
		}

		public ObservableList<string> Segments
		{
			get { return _segments; }
		}

		public ICommand AddSegmentCommand
		{
			get { return _addSegmentCommand; }
		}

		public ICommand RemoveSegmentCommand
		{
			get { return _removeSegmentCommand; }
		}
	}
}
