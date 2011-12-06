using System;

namespace SIL.Cog
{
	public static class Stats
	{
		public static double RootLogLikelihoodRatio(int k11, int k12, int k21, int k22)
		{
			double llr = LogLikelihoodRatio(k11, k12, k21, k22);
			return Math.Sign(((double) k11 / (k11 + k12)) - ((double) k21 / (k21 + k22))) * Math.Sqrt(llr);
		}

		public static double LogLikelihoodRatio(int k11, int k12, int k21, int k22)
		{
			double rowEntropy = Entropy(k11, k12) + Entropy(k21, k22);
			double columnEntropy = Entropy(k11, k21) + Entropy(k12, k22);
			double matrixEntropy = Entropy(k11, k12, k21, k22);
			if (rowEntropy + columnEntropy > matrixEntropy)
				return 0.0;
			return 2.0 * (matrixEntropy - rowEntropy - columnEntropy);
		}

		private static double Entropy(params int[] elements)
		{
			double sum = 0.0;
			double result = 0.0;
			foreach (int element in elements)
			{
				result += element * Math.Log(element);
				sum += element;
			}
			result -= sum * Math.Log(sum);
			return -result;
		}
	}
}
