using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.Clusterers;

namespace SIL.Cog.CommandLine
{
	[Verb("similarity", HelpText = "Computes the lexical similarity for a given set of word lists and cognate sets")]
	public class SimilarityVerb : VerbBase
	{
		[Option('d', "distance", Default = false, HelpText = "Outputs a distance matrix")]
		public bool IsDistance { get; set; }

		protected override ReturnCode DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			SetupProject();
			ReadInput(inputReader);
			WriteOutput(outputWriter);
			return ReturnCode.Okay;
		}

		private void ReadInput(TextReader inputReader)
		{
			var cognateSets = new Dictionary<Meaning, List<HashSet<Variety>>>();
			var meaningCounts = new Dictionary<Variety, int>();
			bool firstLine = true;
			foreach (string line in ReadLines(inputReader))
			{
				string[] tokens = line.Split('\t');
				if (firstLine)
				{
					for (int i = 1; i < tokens.Length; i++)
					{
						string gloss = tokens[i];
						if (string.IsNullOrEmpty(gloss))
							break;
						Project.Meanings.Add(new Meaning(gloss, null));
					}
				}
				else
				{
					var variety = new Variety(tokens[0]);
					Project.Varieties.Add(variety);
					foreach (Variety other in Project.Varieties.Where(v => v != variety))
						Project.VarietyPairs.Add(new VarietyPair(other, variety));
					int meaningCount = 0;
					for (int i = 1; i < tokens.Length; i++)
					{
						string setStr = tokens[i];

						if (string.IsNullOrEmpty(setStr))
							break;

						// ignore meanings that contain question marks
						if (setStr.Contains('?'))
							continue;

						Meaning meaning = Project.Meanings[i - 1];
						List<HashSet<Variety>> sets = cognateSets.GetValue(meaning,
							() => new List<HashSet<Variety>>(Enumerable.Repeat((HashSet<Variety>) null, setStr.Length)));
						var cognateVarieties = new HashSet<Variety>();
						for (int j = 0; j < setStr.Length; j++)
						{
							if (sets[j] == null)
								sets[j] = new HashSet<Variety>();

							if (setStr[j] == '1')
							{
								foreach (Variety cognateVariety in sets[j].Except(cognateVarieties))
								{
									cognateVarieties.Add(cognateVariety);
									VarietyPair pair = variety.VarietyPairs[cognateVariety];
									pair.CognateCount++;
								}
								sets[j].Add(variety);
							}
						}
						meaningCount++;
					}

					meaningCounts[variety] = meaningCount;
				}
				firstLine = false;
			}

			foreach (VarietyPair pair in Project.VarietyPairs)
			{
				int meaningCount = Math.Min(meaningCounts[pair.Variety1], meaningCounts[pair.Variety2]);
				pair.LexicalSimilarityScore = Project.Meanings.Count == 0 ? 0 : (double) pair.CognateCount / meaningCount;
			}
		}

		private void WriteOutput(TextWriter outputWriter)
		{
			var optics = new Optics<Variety>(variety => variety.VarietyPairs
				.Select(pair => Tuple.Create(pair.GetOtherVariety(variety), 1.0 - pair.LexicalSimilarityScore))
				.Concat(Tuple.Create(variety, 0.0)), 2);
			Variety[] varieties = optics.ClusterOrder(Project.Varieties).Select(e => e.DataObject).ToArray();
			foreach (Variety variety in varieties)
			{
				outputWriter.Write("\t");
				outputWriter.Write(variety.Name);
			}
			outputWriter.WriteLine();
			for (int i = 0; i < varieties.Length; i++)
			{
				outputWriter.Write(varieties[i].Name);
				for (int j = 0; j <= i; j++)
				{
					outputWriter.Write("\t");
					if (i == j)
					{
						outputWriter.Write(IsDistance ? "0.00" : "1.00");
					}
					else
					{
						VarietyPair varietyPair = varieties[i].VarietyPairs[varieties[j]];
						double score = IsDistance ? 1.0 - varietyPair.LexicalSimilarityScore : varietyPair.LexicalSimilarityScore;
						outputWriter.Write("{0:0.00}", score);
					}
				}
				outputWriter.WriteLine();
			}
		}
	}
}
