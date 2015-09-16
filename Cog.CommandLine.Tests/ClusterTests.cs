using NUnit.Framework;

namespace SIL.Cog.CommandLine.Tests
{
	class ClusterTests : TestBase
	{
		[Test, Sequential]
		public void CheckUpgmaClusterer_WithBasicInput(
			[Values(
				"a b 0.5\na c 0.4\na d 0.999\nb c 0.3\nb d 0.5\nc d 0.4\n",
				"a b 0.5\na c 0.4\na d 0.801\nb c 0.3\nb d 0.5\nc d 0.4\n", // Pair of words, a and d, JUST above the threshhold
				"a b 0.5\na c 0.4\na d 0.799\nb c 0.3\nb d 0.5\nc d 0.4\n"  // Pair of words, a and d, JUST below the threshhold
				)]
			string input,
			[Values(
				"1 b\n2 c\n3 a d\n",
				"1 b\n2 c\n3 a d\n",   // Just above the threshhold: a and d are grouped together
				"1 a\n2 b\n3 c\n4 d\n" // Just below the threshhold: a and d are not grouped
				)]
			string expectedOutput)
		{
			var clusterer = new ClusterVerb() { Method = "upgma", Threshhold = 0.2 };
			CheckVerbOutput(input, expectedOutput, clusterer, false);
		}

		[Test, Sequential]
		public void CheckLsdbcClusterer_WithBasicInput(
			[Values(2, 4, 2, 4)]
			double alpha,
			[Values(5, 5, 2, 2)]
			int k,
			[Values(
				"1 a d b c\n",
				"1 a d b c\n",
				"1 a d b\n2 c\n",
				"1 a d b\n2 c\n"
			)]
			string expectedOutput)
		{
			string input = "a b 0.5\na c 0.4\na d 0.999\nb c 0.3\nb d 0.5\nc d 0.4\n";
			var clusterer = new ClusterVerb() { Method = "lsdbc", Alpha = alpha, K = k };
			CheckVerbOutput(input, expectedOutput, clusterer, false);
		}

		[Test, Sequential]
		public void CheckDbscanClusterer_WithBasicInput(
			[Values(
				"a b 0.5\na c 0.4\na d 0.999\nb c 0.3\nb d 0.5\nc d 0.4\n"
				)]
			string input,
			[Values(
				"1 a b c d\n"
				)]
			string expectedOutput)
		{
			var clusterer = new ClusterVerb() { Method = "dbscan", Epsilon = 0.2, MinWords = 3 };
			CheckVerbOutput(input, expectedOutput, clusterer, false);
		}

		[Test, Sequential]
		public void CheckUpgmaClusterer_WithRealInput(
			[Values(0.1, 0.2, 0.3, 0.4)]
			double threshhold,
			[Values(
				"1 brother\n2 bird\n3 word\n4 cat\n5 bat\n6 ball\n7 bother mother\n8 dog bog\n9 call kill\n",
				"1 bird\n2 word\n3 dog bog\n4 ball call kill\n5 brother bother mother\n6 cat bat\n",
				"1 bird\n2 word\n3 dog bog\n4 ball call kill\n5 brother bother mother\n6 cat bat\n",
				"1 dog bog\n2 ball call kill\n3 brother bother mother\n4 cat bat\n5 bird word\n"
				)]
			string expectedOutput)
		{
			var clusterer = new ClusterVerb() { Method = "upgma", Threshhold = threshhold };
			CheckVerbOutput(InputWithSimilarityScores, expectedOutput, clusterer, false);
		}

		[Test, Sequential]
		public void CheckLsdbcClusterer_WithRealInput(
			[Values(2, 4, 2, 4)]
			double alpha,
			[Values(2, 2, 5, 5)]
			int k,
			[Values(
				"1 call kill ball\n2 bother mother brother\n3 bog dog bat cat\n4 bird word\n",
				"1 call kill ball\n2 bother mother brother\n3 bog dog bat cat\n4 bird word\n",
				"1 bird ball word kill bat call dog bog cat\n2 brother bother mother\n",
				"1 bird ball word kill bat call dog bog cat\n2 brother bother mother\n"
			)]
			string expectedOutput)
		{
			string input = "a b 0.5\na c 0.4\na d 0.999\nb c 0.3\nb d 0.5\nc d 0.4\n";
			var clusterer = new ClusterVerb() { Method = "lsdbc", Alpha = alpha, K = k };
			CheckVerbOutput(InputWithSimilarityScores, expectedOutput, clusterer, false);
		}

		[Test, Sequential]
		public void CheckDbscanClusterer_WithRealInput(
			[Values(0.1, 0.1, 0.1, 0.1, 0.2, 0.2, 0.2, 0.2)]
			double epsilon,
			[Values(2, 3, 4, 5, 2, 3, 4, 5)]
			int minPoints,
			[Values(
				"1 brother bother dog bog bird word mother cat call bat ball kill\n",
				"1 brother bother dog bog bird word mother cat call bat ball kill\n",
				"1 brother bother dog bog bird word mother cat call bat ball kill\n",
				"1 brother bother dog bog bird word mother cat call bat ball kill\n",
				"1 mother brother\n2 kill ball\n3 dog bog bird word cat bat\n",
				"1 brother bother dog bog bird word mother cat call bat ball kill\n",
				"1 brother bother dog bog bird word mother cat call bat ball kill\n",
				"1 brother bother dog bog bird word mother cat call bat ball kill\n"
				)]
			string expectedOutput)
		{
			var clusterer = new ClusterVerb() { Method = "dbscan", Epsilon = epsilon, MinWords = minPoints };
			CheckVerbOutput(InputWithSimilarityScores, expectedOutput, clusterer, false);
		}

		[Test, Sequential]
		public void CheckErrors(
			[Values(
				"a\n",
				"a\na\n", // Should produce two separate errors
				"a\n\n\na\n", // Blank lines should not produce any extra errors
				"a b c"
				)]
			string input,
			[Values(
				"Operation \"cluster\" produced 1 error:\nEach line should contain two words and one score, separated by spaces.\n  This was caused by the line: \"a\"\n",
				"Operation \"cluster\" produced 2 errors:\nEach line should contain two words and one score, separated by spaces.\n  This was caused by the line: \"a\"\n" +
				"Each line should contain two words and one score, separated by spaces.\n  This was caused by the line: \"a\"\n",
				"Operation \"cluster\" produced 2 errors:\nEach line should contain two words and one score, separated by spaces.\n  This was caused by the line: \"a\"\n" +
				"Each line should contain two words and one score, separated by spaces.\n  This was caused by the line: \"a\"\n",
				"Operation \"cluster\" produced 1 error:\nCould not parse score \"c\". Scores should be a number between 0 and 1.\n  This was caused by the line: \"a b c\"\n"
				)]
			string expectedErrors)
		{
			var clusterer = new ClusterVerb() { Method = "upgma", Threshhold = 0.2 };
			CheckVerbOutput(input, "", expectedErrors, clusterer, false);
		}

		// This is what would be output by running a simple word list through PairVerb and DistanceVerb
		public readonly string InputWithSimilarityScores = @"
brother bother 0.816326530612245
brother dog 0.163265306122449
brother bog 0.195918367346939
brother bird 0.210204081632653
brother word 0.175510204081633
brother mother 0.795918367346939
brother cat 0.104081632653061
brother call 0.144897959183673
brother bat 0.189795918367347
brother ball 0.230612244897959
brother kill 0.140816326530612
bother dog 0.238095238095238
bother bog 0.276190476190476
bother bird 0.292857142857143
bother word 0.214285714285714
bother mother 0.976190476190476
bother cat 0.169047619047619
bother call 0.216666666666667
bother bat 0.269047619047619
bother ball 0.316666666666667
bother kill 0.211904761904762
dog bog 0.923809523809524
dog bird 0.425
dog word 0.435714285714286
dog mother 0.214285714285714
dog cat 0.538095238095238
dog call 0.260714285714286
dog bat 0.585714285714286
dog ball 0.296428571428571
dog kill 0.253571428571429
bog bird 0.482142857142857
bog word 0.378571428571429
bog mother 0.252380952380952
bog cat 0.461904761904762
bog call 0.203571428571429
bog bat 0.661904761904762
bog ball 0.353571428571429
bog kill 0.196428571428571
bird word 0.675
bird mother 0.269047619047619
bird cat 0.421428571428571
bird call 0.564285714285714
bird bat 0.571428571428571
bird ball 0.714285714285714
bird kill 0.607142857142857
word mother 0.19047619047619
word cat 0.375
word call 0.517857142857143
word bat 0.296428571428571
word ball 0.439285714285714
word kill 0.567857142857143
mother cat 0.169047619047619
mother call 0.192857142857143
mother bat 0.245238095238095
mother ball 0.292857142857143
mother kill 0.188095238095238
cat call 0.535714285714286
cat bat 0.8
cat ball 0.385714285714286
cat kill 0.435714285714286
call bat 0.385714285714286
call ball 0.85
call kill 0.9
bat ball 0.535714285714286
bat kill 0.285714285714286
ball kill 0.75".TrimStart();
	}
}
