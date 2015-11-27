using System;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class WordListsViewModelTests
	{
		private CogProject SetupProjectWithThreeVarieties(WordListsViewModelTestEnvironment env)
		{
			var project = new CogProject(env.SpanFactory)
			{
				Varieties = {new Variety("variety1"), new Variety("variety2"), new Variety("variety3")}
			};
			env.OpenProject(project);
			return project;
		}

		private CogProject SetupProjectWithThreeMeanings(WordListsViewModelTestEnvironment env)
		{
			var project = new CogProject(env.SpanFactory)
			{
				Meanings = {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")}
			};
			env.OpenProject(project);
			return project;
		}

		private void SetupProjectWithWords(WordListsViewModelTestEnvironment env)
		{
			var project = new CogProject(env.SpanFactory)
				{
					Meanings = {new Meaning("gloss1", "cat1"), new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")},
					Varieties = {new Variety("variety1"), new Variety("variety2")}
				};
			project.Varieties[0].Words.AddRange(new[] {new Word("hello", project.Meanings[0]), new Word("good", project.Meanings[1]), new Word("bad", project.Meanings[2])});
			project.Varieties[1].Words.AddRange(new[] {new Word("help", project.Meanings[0]), new Word("google", project.Meanings[1]), new Word("batter", project.Meanings[2])});

			env.OpenProject(project);
		}

		[Test]
		public void Varieties_OpenProject_VarietiesPopulated()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithThreeVarieties(env);

				Assert.That(env.WordListsViewModel.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2", "variety3"}));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.False);
			}
		}

		[Test]
		public void Varieties_Remove_VarietyRemoved()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				CogProject project = SetupProjectWithThreeVarieties(env);

				project.Varieties.RemoveAt(0);
				Assert.That(env.WordListsViewModel.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety2", "variety3"}));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.False);
			}
		}

		[Test]
		public void Varieties_Add_VarietyAdded()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				CogProject project = new CogProject(env.SpanFactory)
				{
					Varieties = {new Variety("variety2"), new Variety("variety3")}
				};
				env.OpenProject(project);

				project.Varieties.Add(new Variety("variety1"));
				Assert.That(env.WordListsViewModel.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety2", "variety3", "variety1"}));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.False);
			}
		}

		[Test]
		public void Varieties_Clear_AllVarietiesRemoved()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				CogProject project = SetupProjectWithThreeVarieties(env);

				project.Varieties.Clear();
				Assert.That(env.WordListsViewModel.Varieties.Count, Is.EqualTo(0));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.True);
			}
		}

		[Test]
		public void Varieties_ReopenProject_VarietiesPopulated()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithThreeVarieties(env);

				Assert.That(env.WordListsViewModel.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2", "variety3"}));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.False);

				var project = new CogProject(env.SpanFactory) {Varieties = {new Variety("variety1")}};
				env.OpenProject(project);

				Assert.That(env.WordListsViewModel.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1"}));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.False);
			}
		}

		[Test]
		public void Varieties_DomainModelChangedMessage_CheckForErrors()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				var project = new CogProject(env.SpanFactory);
				env.OpenProject(project);

				project.Meanings.Add(new Meaning("gloss1", "cat1"));
				var bat = new Word("bat", project.Meanings[0]);
				project.Varieties.Add(new Variety("variety1") {Words = {bat}});

				Assert.That(env.WordListsViewModel.Varieties[0].IsValid, Is.False);

				var segmenter = new Segmenter(env.SpanFactory)
				{
					Consonants = {"b", "t"},
					Vowels = {"a"}
				};
				segmenter.Segment(bat);

				Messenger.Default.Send(new DomainModelChangedMessage(true));
				Assert.That(env.WordListsViewModel.Varieties[0].IsValid, Is.True);
			}
		}

		[Test]
		public void Meanings_OpenProject_MeaningsPopulated()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithThreeMeanings(env);

				Assert.That(env.WordListsViewModel.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.False);
			}
		}

		[Test]
		public void Meanings_Remove_MeaningRemoved()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				CogProject project = SetupProjectWithThreeMeanings(env);

				project.Meanings.RemoveAt(0);
				Assert.That(env.WordListsViewModel.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss2", "gloss3"}));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.False);
			}
		}

		[Test]
		public void Meanings_Add_MeaningAdded()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				CogProject project = new CogProject(env.SpanFactory)
				{
					Meanings = {new Meaning("gloss2", "cat2"), new Meaning("gloss3", "cat3")}
				};
				env.OpenProject(project);

				project.Meanings.Add(new Meaning("gloss1", "cat1"));
				Assert.That(env.WordListsViewModel.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss2", "gloss3", "gloss1"}));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.False);
			}
		}

		[Test]
		public void Meanings_Clear_AllMeaningsRemoved()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				CogProject project = SetupProjectWithThreeMeanings(env);

				project.Meanings.Clear();
				Assert.That(env.WordListsViewModel.Meanings.Count, Is.EqualTo(0));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.True);
			}
		}

		[Test]
		public void Meanings_ReopenProject_MeaningsPopulated()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithThreeMeanings(env);

				Assert.That(env.WordListsViewModel.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1", "gloss2", "gloss3"}));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.False);

				CogProject project = new CogProject(env.SpanFactory)
				{
					Meanings = {new Meaning("gloss1", "cat1")}
				};
				env.OpenProject(project);

				Assert.That(env.WordListsViewModel.Meanings.Select(s => s.Gloss), Is.EqualTo(new[] {"gloss1"}));
				Assert.That(env.WordListsViewModel.IsEmpty, Is.False);
			}
		}

		[Test]
		public void FindCommand_DialogOpen_NotOpenedAgain()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.DialogService.ClearReceivedCalls();
				env.WordListsViewModel.FindCommand.Execute(null);
				env.DialogService.DidNotReceive().ShowModelessDialog(env.WordListsViewModel, Arg.Any<FindViewModel>(), Arg.Any<Action>());
			}
		}

		[Test]
		public void FindCommand_FormNothingSelectedNoMatches_NoWordSelected()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Form;
				env.FindViewModel.String = "fall";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.Null);
			}
		}

		[Test]
		public void FindCommand_FormNothingSelectedMatches_CorrectWordsSelected()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Form;
				env.FindViewModel.String = "he";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[0].Meanings[0]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[1].Meanings[0]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[1].Meanings[0]));
			}
		}

		[Test]
		public void FindCommand_FormFirstWordSelectedMatches_CorrectWordsSelected()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Form;
				env.WordListsViewModel.SelectedVarietyMeaning = env.WordListsViewModel.Varieties[0].Meanings[0];
				env.FindViewModel.String = "o";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[0].Meanings[1]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[1].Meanings[1]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[0].Meanings[0]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[0].Meanings[0]));
				// start search over
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[0].Meanings[1]));
			}
		}

		[Test]
		public void FindCommand_FormLastWordSelectedMatches_CorrectWordsSelected()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Form;
				env.WordListsViewModel.SelectedVarietyMeaning = env.WordListsViewModel.Varieties[1].Meanings[2];
				env.FindViewModel.String = "ba";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[0].Meanings[2]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[1].Meanings[2]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[1].Meanings[2]));
			}
		}

		[Test]
		public void FindCommand_FormLastWordSelectedChangeSelectedWord_CorrectWordsSelected()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Form;
				env.WordListsViewModel.SelectedVarietyMeaning = env.WordListsViewModel.Varieties[1].Meanings[2];
				env.FindViewModel.String = "ba";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[0].Meanings[2]));
				env.WordListsViewModel.SelectedVarietyMeaning = env.WordListsViewModel.Varieties[0].Meanings[0];
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[0].Meanings[2]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[1].Meanings[2]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[1].Meanings[2]));
			}
		}

		[Test]
		public void FindCommand_GlossNothingSelectedNoMatches_NoWordSelected()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Gloss;
				env.FindViewModel.String = "gloss4";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.Null);
			}
		}

		[Test]
		public void FindCommand_GlossNothingSelectedMatches_CorrectWordsSelected()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Gloss;
				env.FindViewModel.String = "gloss2";
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[0].Meanings[1]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[0].Meanings[1]));
			}
		}

		[Test]
		public void FindCommand_GlossWordSelectedMatches_CorrectWordsSelected()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				SetupProjectWithWords(env);
				env.OpenFindDialog();

				env.FindViewModel.Field = FindField.Gloss;
				env.FindViewModel.String = "gloss";
				env.WordListsViewModel.SelectedVarietyMeaning = env.WordListsViewModel.Varieties[1].Meanings[1];
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[1].Meanings[2]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[1].Meanings[0]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[1].Meanings[1]));
				env.FindViewModel.FindNextCommand.Execute(null);
				Assert.That(env.WordListsViewModel.SelectedVarietyMeaning, Is.EqualTo(env.WordListsViewModel.Varieties[1].Meanings[1]));
			}
		}

		[Test]
		public void TaskAreas_AddVarietyCommandExecuted_VarietyAdded()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				CogProject project = SetupProjectWithThreeVarieties(env);

				var commonTasks = (TaskAreaItemsViewModel) env.WordListsViewModel.TaskAreas[0];

				// add a new variety
				var addVariety = (TaskAreaCommandViewModel) commonTasks.Items[0];
				env.DialogService.ShowModalDialog(env.WordListsViewModel, Arg.Do<EditVarietyViewModel>(vm => vm.Name = "variety4")).Returns(true);
				addVariety.Command.Execute(null);

				Assert.That(project.Varieties.Select(v => v.Name), Is.EqualTo(new[] {"variety1", "variety2", "variety3", "variety4"}));
			}
		}
	}
}
