using System;
using System.Collections.Generic;
using System.Windows;
using GraphSharp.Algorithms.Layout;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Factorization;
using MathNet.Numerics.LinearAlgebra.Generic;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;

namespace SIL.Cog.Applications.GraphAlgorithms
{
	public class StressMajorizationLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, StressMajorizationLayoutParameters>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
	{

		public StressMajorizationLayoutAlgorithm(TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions,
			StressMajorizationLayoutParameters oldParameters)
			: base(visitedGraph, vertexPositions, oldParameters)
		{
		}

		protected override void InternalCompute()
		{
			double[,] d, w;
			GetDistancesAndWeights(out d, out w);

			var x = new DenseMatrix(VisitedGraph.VertexCount, 2);
			x[0, 0] = 0;
			x[0, 1] = 0;
			var rand = new Random();
			for (int i = 1; i < VisitedGraph.VertexCount; i++)
			{
				x[i, 0] = Math.Max(double.Epsilon, rand.NextDouble() * Parameters.Width);
				x[i, 1] = Math.Max(double.Epsilon, rand.NextDouble() * Parameters.Height);
			}

			DenseMatrix l = ComputeL(w);

			double s0 = CalcStress(x, d, w);

			var cholesky = new DenseCholesky((DenseMatrix) l.SubMatrix(1, l.RowCount - 1, 1, l.ColumnCount - 1));

			for (int i = 0; i < Parameters.MaxIterations; i++)
			{
				DenseMatrix lx = ComputeLZ(d, w, x);
				Matrix<double> b = lx.Multiply(x);

				Matrix<double> rx = cholesky.Solve(b.SubMatrix(1, b.RowCount - 1, 0, b.ColumnCount));
				x.SetSubMatrix(1, x.RowCount - 1, 0, x.ColumnCount, rx);

				double s1 = CalcStress(x, d, w);
				if (Math.Abs(s0 - s1) < 1e-4 * s0)
					break;
				s0 = s1;
			}

			SetPositions(x);
		}

		private void SetPositions(Matrix<double> x)
		{
			int i = 0;
			foreach (TVertex vertex in VisitedGraph.Vertices)
			{
				VertexPositions[vertex] = new Point(x[i, 0], x[i, 1]);
				i++;
			}
		}

		private void GetDistancesAndWeights(out double[,] d, out double[,] w)
		{
			d = new double[VisitedGraph.VertexCount, VisitedGraph.VertexCount];
			w = new double[VisitedGraph.VertexCount, VisitedGraph.VertexCount];

			double maxCost = 0;
			var undirected = new UndirectedBidirectionalGraph<TVertex, TEdge>(VisitedGraph);
			int i = 0;
			foreach (TVertex source in undirected.Vertices)
			{
				var spa = new UndirectedDijkstraShortestPathAlgorithm<TVertex, TEdge>(undirected, edge => edge is IWeightedEdge<TVertex> ? Parameters.WeightAdjustment - (((IWeightedEdge<TVertex>) edge).Weight) : 1.0);
				spa.Compute(source);
				int j = 0;
				foreach (TVertex target in undirected.Vertices)
				{
					double cost;
					if (spa.TryGetDistance(target, out cost))
					{
						d[i, j] = cost;
						if (cost > maxCost)
							maxCost = cost;
					}
					else
					{
						d[i, j] = double.NaN;
					}
					j++;
				}
				i++;
			}

			double idealEdgeLength = (Math.Min(Parameters.Width, Parameters.Height) / maxCost) * Parameters.LengthFactor;
			double disconnectedCost = maxCost * Parameters.DisconnectedMultiplier;
			for (i = 0; i < VisitedGraph.VertexCount; i++)
			{
				for (int j = 0; j < VisitedGraph.VertexCount; j++)
				{
					if (double.IsNaN(d[i, j]))
						d[i, j] = disconnectedCost;
					else
						d[i, j] *= idealEdgeLength;
					w[i, j] = Math.Pow(Math.Max(d[i, j], 0.0001), -Parameters.Alpha);
				}
			}
		}

		private DenseMatrix ComputeL(double[,] w)
		{
			var l = new DenseMatrix(VisitedGraph.VertexCount, VisitedGraph.VertexCount);
			for (int i = 0; i < VisitedGraph.VertexCount; i++)
			{
				double wii = 0;
				for (int j = 0; j < VisitedGraph.VertexCount; j++)
				{
					if (i == j)
						continue;
					wii += w[i, j];
					l[i, j] = -w[i, j];
				}
				l[i, i] = wii;
			}

			return l;
		}

		private DenseMatrix ComputeLZ(double[,] d, double[,] w, Matrix<double> z)
		{
			var lz = new DenseMatrix(VisitedGraph.VertexCount, VisitedGraph.VertexCount);

			for (int i = 0; i < VisitedGraph.VertexCount; i++)
			{
				double lzii = 0;
				for (int j = 0; j < VisitedGraph.VertexCount; j++)
				{
					if (i == j)
						continue;

					double norm = Math.Sqrt(Math.Pow(z[i, 0] - z[j, 0], 2) + Math.Pow(z[i, 1] - z[j, 1], 2));
					double lzij = -(w[i, j] * d[i, j]) * (norm < double.Epsilon ? 0 : 1 / norm);
					lzii += lzij;
					lz[i, j] = lzij;
				}

				lz[i, i] = -lzii;
			}

			return lz;
		}

		private double CalcStress(Matrix<double> x, double[,] d, double[,] w)
		{
			double stress = 0;
			for (int i = 0; i < VisitedGraph.VertexCount; i++)
			{
				for (int j = 0; j < i; j++)
				{
					double norm = Math.Sqrt(Math.Pow(x[i, 0] - x[j, 0], 2) + Math.Pow(x[i, 1] - x[j, 1], 2));
					stress += w[i, j] * Math.Pow(norm - d[i, j], 2);
				}
			}

			return stress;
		}
	}
}
