using System.Linq;
using NSubstitute;
using NUnit.Framework;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Tests.ViewModels
{
	[TestFixture]
	public class WordListsVarietyMeaningViewModelTests
	{
		[Test]
		public void Words_AddWord_StrRepAndWordsUpdated()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				CogProject project = env.OpenProject(new[] {new Meaning("gloss1", "cat1")}, new[] {new Variety("variety1")});
				WordListsVarietyMeaningViewModel varietyMeaning = env.WordLists.Varieties[0].Meanings[0];

				project.Varieties[0].Words.Add(new Word("hɛ.loʊ", project.Meanings[0]));
				Assert.That(varietyMeaning.StrRep, Is.EqualTo("hɛ.loʊ"));
				Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.loʊ"}));

				project.Varieties[0].Words.Add(new Word("gu.gəl", project.Meanings[0]));

				Assert.That(varietyMeaning.StrRep, Is.EqualTo("hɛ.loʊ,gu.gəl"));
				Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.loʊ", "gu.gəl"}));
			}
		}

		[Test]
		public void StrRep_SetStrRep_WordsUpdated()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				env.OpenProject(new[] {new Meaning("gloss1", "cat1")}, new[] {new Variety("variety1")});
				WordListsVarietyMeaningViewModel varietyMeaning = env.WordLists.Varieties[0].Meanings[0];

				varietyMeaning.StrRep = "hɛ.lp,gu.gəl";
				Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp", "gu.gəl"}));

				varietyMeaning.StrRep = "hɛ.lp";
				Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp"}));
			}
		}

		[Test]
		public void StrRep_SetStrRepEmpty_WordsEmpty()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				CogProject project = env.OpenProject(new[] {new Meaning("gloss1", "cat1")}, new[] {new Variety("variety1")});
				WordListsVarietyMeaningViewModel varietyMeaning = env.WordLists.Varieties[0].Meanings[0];

				project.Varieties[0].Words.Add(new Word("hɛ.loʊ", project.Meanings[0]));

				varietyMeaning.StrRep = "";
				Assert.That(varietyMeaning.Words, Is.Empty);
			}
		}

		[Test]
		public void StrRep_SetStrRepNull_WordsEmpty()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				CogProject project = env.OpenProject(new[] {new Meaning("gloss1", "cat1")}, new[] {new Variety("variety1")});
				WordListsVarietyMeaningViewModel varietyMeaning = env.WordLists.Varieties[0].Meanings[0];

				project.Varieties[0].Words.Add(new Word("hɛ.loʊ", project.Meanings[0]));

				varietyMeaning.StrRep = null;
				Assert.That(varietyMeaning.Words, Is.Empty);
			}
		}

		[Test]
		public void StrRep_SetStrRepWithSpaces_WordsUpdated()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				env.OpenProject(new[] {new Meaning("gloss1", "cat1")}, new[] {new Variety("variety1")});
				WordListsVarietyMeaningViewModel varietyMeaning = env.WordLists.Varieties[0].Meanings[0];

				varietyMeaning.StrRep = " hɛ.lp,gu.gəl ,gu.fi ";
				Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp", "gu.gəl", "gu.fi"}));
				Assert.That(varietyMeaning.StrRep, Is.EqualTo("hɛ.lp,gu.gəl,gu.fi"));
			}
		}

		[Test]
		public void StrRep_TruncateStrRep_WordsUpdated()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				env.OpenProject(new[] {new Meaning("gloss1", "cat1")}, new[] {new Variety("variety1")});
				WordListsVarietyMeaningViewModel varietyMeaning = env.WordLists.Varieties[0].Meanings[0];

				varietyMeaning.StrRep = " hɛ.lp,gu.gəl,gu.fi";
				Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp", "gu.gəl", "gu.fi"}));
				Assert.That(varietyMeaning.StrRep, Is.EqualTo("hɛ.lp,gu.gəl,gu.fi"));

				varietyMeaning.StrRep = "hɛ.lp,gu.gəl,gu";
				Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"hɛ.lp", "gu.gəl", "gu"}));
			}
		}

		[Test]
		public void StrRep_SetStrRep_VarietyIsValidUpdated()
		{
			using (var env = new WordListsViewModelTestEnvironment())
			{
				var segmenter = new Segmenter(env.SpanFactory)
				{
					Consonants = {"b", "t"},
					Vowels = {"a"}
				};
				env.AnalysisService.Segment(Arg.Do<Variety>(variety => segmenter.Segment(variety.Words.First())));
				env.OpenProject(new[] {new Meaning("gloss1", "cat1")}, new[] {new Variety("variety1")});
				WordListsVarietyMeaningViewModel varietyMeaning = env.WordLists.Varieties[0].Meanings[0];

				varietyMeaning.StrRep = "cat";
				Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"cat"}));
				Assert.That(env.WordLists.Varieties[0].IsValid, Is.False);

				varietyMeaning.StrRep = "bat";
				Assert.That(varietyMeaning.Words.Select(w => w.StrRep), Is.EqualTo(new[] {"bat"}));
				Assert.That(env.WordLists.Varieties[0].IsValid, Is.True);
			}
		}
	}
}
