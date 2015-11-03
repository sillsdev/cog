using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class WordsViewModelTests
	{
		private IBusyService _busyService;
		private IAnalysisService _analysisService;
		private WordsViewModel _wordsViewModel;
		private ObservableList<WordViewModel> _words;
		private Meaning _meaning;

		[SetUp]
		public void SetUp()
		{
			_busyService = Substitute.For<IBusyService>();
			_analysisService = Substitute.For<IAnalysisService>();
			_words = new ObservableList<WordViewModel>
			{
				new WordViewModel(_busyService, _analysisService, new Word("valid", _meaning)) {IsValid = true},
				new WordViewModel(_busyService, _analysisService, new Word("invalid", _meaning)) {IsValid = false}
			};
			_wordsViewModel = new WordsViewModel(_busyService, new ReadOnlyBindableList<WordViewModel>(_words));
			_meaning = new Meaning("gloss", "category");
		}

		[Test]
		public void ValidWordCount_ValidWordAdded_Updated()
		{
			Assert.That(_wordsViewModel.ValidWordCount, Is.EqualTo(1));
			_words.Add(new WordViewModel(_busyService, _analysisService, new Word("valid2", _meaning)) {IsValid = true});
			Assert.That(_wordsViewModel.ValidWordCount, Is.EqualTo(2));
		}

		[Test]
		public void ValidWordCount_ValidWordRemoved_Updated()
		{
			Assert.That(_wordsViewModel.ValidWordCount, Is.EqualTo(1));
			_words.RemoveAt(0);
			Assert.That(_wordsViewModel.ValidWordCount, Is.EqualTo(0));
		}

		[Test]
		public void ValidWordCount_WordsCleared_Updated()
		{
			Assert.That(_wordsViewModel.ValidWordCount, Is.EqualTo(1));
			_words.Clear();
			Assert.That(_wordsViewModel.ValidWordCount, Is.EqualTo(0));
		}

		[Test]
		public void InvalidWordCount_InvalidWordAdded_Updated()
		{
			Assert.That(_wordsViewModel.InvalidWordCount, Is.EqualTo(1));
			_words.Add(new WordViewModel(_busyService, _analysisService, new Word("invalid2", _meaning)) {IsValid = false});
			Assert.That(_wordsViewModel.InvalidWordCount, Is.EqualTo(2));
		}

		[Test]
		public void InvalidWordCount_InvalidWordRemoved_Updated()
		{
			Assert.That(_wordsViewModel.InvalidWordCount, Is.EqualTo(1));
			_words.RemoveAt(1);
			Assert.That(_wordsViewModel.InvalidWordCount, Is.EqualTo(0));
		}

		[Test]
		public void InvalidWordCount_WordsCleared_Updated()
		{
			Assert.That(_wordsViewModel.InvalidWordCount, Is.EqualTo(1));
			_words.Clear();
			Assert.That(_wordsViewModel.InvalidWordCount, Is.EqualTo(0));
		}

		[Test]
		public void InvalidWordCount_IsValidUpdated_Updated()
		{
			Assert.That(_wordsViewModel.InvalidWordCount, Is.EqualTo(1));
			_words[1].IsValid = true;
			Assert.That(_wordsViewModel.ValidWordCount, Is.EqualTo(2));
			Assert.That(_wordsViewModel.InvalidWordCount, Is.EqualTo(0));
		}

		[Test]
		public void ValidWordCount_IsValidUpdated_Updated()
		{
			Assert.That(_wordsViewModel.ValidWordCount, Is.EqualTo(1));
			_words[0].IsValid = false;
			Assert.That(_wordsViewModel.ValidWordCount, Is.EqualTo(0));
			Assert.That(_wordsViewModel.InvalidWordCount, Is.EqualTo(2));
		}
	}
}
