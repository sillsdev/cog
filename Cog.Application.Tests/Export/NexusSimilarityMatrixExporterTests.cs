using System.IO;
using System.Text;
using NUnit.Framework;
using SIL.Cog.Application.Export;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.Tests.Export
{
	[TestFixture]
	public class NexusSimilarityMatrixExporterTests
	{
		private CogProject CreateProject()
		{
			var project = new CogProject();
			var variety1 = new Variety("variety1");
			project.Varieties.Add(variety1);
			var variety2 = new Variety("variety2");
			project.Varieties.Add(variety2);
			var variety3 = new Variety("variety3");
			project.Varieties.Add(variety3);
			var generator = new VarietyPairGenerator();
			generator.Process(project);
			variety1.VarietyPairs[variety2].LexicalSimilarityScore = 0.9;
			variety1.VarietyPairs[variety2].PhoneticSimilarityScore = 0.95;
			variety1.VarietyPairs[variety3].LexicalSimilarityScore = 0.8;
			variety1.VarietyPairs[variety3].PhoneticSimilarityScore = 0.85;
			variety2.VarietyPairs[variety3].LexicalSimilarityScore = 0.7;
			variety2.VarietyPairs[variety3].PhoneticSimilarityScore = 0.75;
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
BEGIN Distances;
	DIMENSIONS NTax=3;
	FORMAT Triangle=LOWER Diagonal Labels Missing=?;
	MATRIX
		variety1 0.00
		variety2 {0:0.00} 0.00
		variety3 {1:0.00} {2:0.00} 0.00;
END;
";

		[Test]
		public void Export_LexicalSimilarity()
		{
			CogProject project = CreateProject();
			var exporter = new NexusSimilarityMatrixExporter();
			using (var stream = new MemoryStream())
			{
				exporter.Export(stream, project, SimilarityMetric.Lexical);
				Assert.That(Encoding.UTF8.GetString(stream.ToArray()), Is.EqualTo(string.Format(NexusFileTemplate, 0.1, 0.2, 0.3)));
			}
		}

		[Test]
		public void Export_PhoneticSimilarity()
		{
			CogProject project = CreateProject();
			var exporter = new NexusSimilarityMatrixExporter();
			using (var stream = new MemoryStream())
			{
				exporter.Export(stream, project, SimilarityMetric.Phonetic);
				Assert.That(Encoding.UTF8.GetString(stream.ToArray()), Is.EqualTo(string.Format(NexusFileTemplate, 0.05, 0.15, 0.25)));
			}
		}

		[Test]
		public void Export_EmptyProject()
		{
			var project = new CogProject();
			var exporter = new NexusSimilarityMatrixExporter();
			using (var stream = new MemoryStream())
			{
				exporter.Export(stream, project, SimilarityMetric.Lexical);
				Assert.That(Encoding.UTF8.GetString(stream.ToArray()), Is.EqualTo(@"#NEXUS
BEGIN Taxa;
	DIMENSIONS NTax=0;
	TAXLABELS;
END;
BEGIN Distances;
	DIMENSIONS NTax=0;
	FORMAT Triangle=LOWER Diagonal Labels Missing=?;
	MATRIX;
END;
"));
			}
		}
	}
}
