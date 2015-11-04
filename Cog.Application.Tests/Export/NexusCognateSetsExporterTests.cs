using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SIL.Cog.Application.Export;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.Tests.Export
{
	[TestFixture]
	public class NexusCognateSetsExporterTests
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();

		private CogProject CreateProject()
		{
			var project = new CogProject(_spanFactory);
			var variety1 = new Variety("variety1");
			project.Varieties.Add(variety1);
			var variety2 = new Variety("variety2");
			project.Varieties.Add(variety2);
			var variety3 = new Variety("variety3");
			project.Varieties.Add(variety3);
			var meaning1 = new Meaning("meaning1", null);
			project.Meanings.Add(meaning1);
			var meaning2 = new Meaning("meaning2", null);
			project.Meanings.Add(meaning2);
			var meaning3 = new Meaning("meaning3", null);
			project.Meanings.Add(meaning3);

			variety1.Words.Add(new Word("word1", meaning1));
			variety1.Words.Add(new Word("word2", meaning2));
			variety1.Words.Add(new Word("word3", meaning3));

			variety2.Words.Add(new Word("word4", meaning1));
			variety2.Words.Add(new Word("word5", meaning2));
			variety2.Words.Add(new Word("word6", meaning3));

			variety3.Words.Add(new Word("word7", meaning1));
			variety3.Words.Add(new Word("word8", meaning2));
			variety3.Words.Add(new Word("word9", meaning3));

			var vpGenerator = new VarietyPairGenerator();
			vpGenerator.Process(project);

			double score = 1.0;
			foreach (VarietyPair vp in project.VarietyPairs)
			{
				foreach (Meaning meaning in project.Meanings)
				{
					Word w1 = vp.Variety1.Words[meaning].First();
					Word w2 = vp.Variety2.Words[meaning].First();
					WordPair wp = vp.WordPairs.Add(w1, w2);
					wp.CognacyScore = score;
					wp.AreCognatePredicted = true;
					score -= 0.1;
				}
			}
			return project;
		}

		private const string NexusFileTemplate = @"#NEXUS
BEGIN Taxa;
	DIMENSIONS NTax=3;
	TAXLABELS
		variety1
		variety2
		variety3;
END;
BEGIN Characters;
	DIMENSIONS NChar=3;
	FORMAT Datatype=STANDARD Missing=? Symbols=""0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"";
	MATRIX
		variety1 {0}
		variety2 {1}
		variety3 {2};
END;
";

		[Test]
		public void Export()
		{
			CogProject project = CreateProject();
			var exporter = new NexusCognateSetsExporter();
			using (var stream = new MemoryStream())
			{
				exporter.Export(stream, project);
				Assert.That(Encoding.UTF8.GetString(stream.ToArray()), Is.EqualTo(string.Format(NexusFileTemplate, "112", "112", "111")));
			}
		}

		[Test]
		public void Export_EmptyProject()
		{
			var project = new CogProject(_spanFactory);
			var exporter = new NexusCognateSetsExporter();
			using (var stream = new MemoryStream())
			{
				exporter.Export(stream, project);
				Assert.That(Encoding.UTF8.GetString(stream.ToArray()), Is.EqualTo(@"#NEXUS
BEGIN Taxa;
	DIMENSIONS NTax=0;
	TAXLABELS;
END;
BEGIN Characters;
	DIMENSIONS NChar=0;
	FORMAT Datatype=STANDARD Missing=? Symbols=""0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"";
	MATRIX;
END;
"));
			}
		}
	}
}
