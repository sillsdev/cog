using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using QuickGraph;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public static class CogExtensions
	{
		public static string OriginalStrRep(this ShapeNode node)
		{
			return (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.OriginalStrRep);
		}

		public static string OriginalStrRep(this Annotation<ShapeNode> ann)
		{
			return ann.Span.Start.GetNodes(ann.Span.End).OriginalStrRep();
		}

		public static string OriginalStrRep(this IEnumerable<ShapeNode> nodes)
		{
			return string.Concat(nodes.Select(node => node.OriginalStrRep()));
		}

		public static string StrRep(this ShapeNode node)
		{
			return (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
		}

		public static string StrRep(this Annotation<ShapeNode> ann)
		{
			return ann.Span.Start.GetNodes(ann.Span.End).StrRep();
		}

		public static string StrRep(this IEnumerable<ShapeNode> nodes)
		{
			return string.Concat(nodes.Select(node => node.StrRep()));
		}

		public static FeatureSymbol Type(this ShapeNode node)
		{
			return (FeatureSymbol) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.Type);
		}

		public static FeatureSymbol Type(this Annotation<ShapeNode> ann)
		{
			return (FeatureSymbol) ann.FeatureStruct.GetValue(CogFeatureSystem.Type);
		}

		public static SoundContext ToSoundContext(this ShapeNode node, Variety variety, IEnumerable<SoundClass> soundClasses)
		{
			ShapeNode prevNode = node.GetPrev(NodeFilter);
			SoundClass leftEnv = prevNode.GetMatchingSoundClass(variety, soundClasses);
			Ngram target = node.ToNgram(variety);
			ShapeNode nextNode = node.GetNext(NodeFilter);
			SoundClass rightEnv = nextNode.GetMatchingSoundClass(variety, soundClasses);
			return new SoundContext(leftEnv, target, rightEnv);
		}

		public static SoundClass GetMatchingSoundClass(this ShapeNode node, Variety variety, IEnumerable<SoundClass> soundClasses)
		{
			Annotation<ShapeNode> stemAnn = ((Shape) node.List).Annotations.First(ann => ann.Type() == CogFeatureSystem.StemType);
			Segment left = null;
			if (stemAnn.Span.Contains(node) || node.Annotation.CompareTo(stemAnn) > 0)
			{
				ShapeNode leftNode = node.GetPrev(NodeFilter);
				if (leftNode != null)
					left = stemAnn.Span.Contains(leftNode) ? variety.Segments[leftNode] : Segment.Anchor;
			}

			Ngram target = stemAnn.Span.Contains(node) ? node.ToNgram(variety) : new Ngram(Segment.Anchor);

			Segment right = null;
			if (stemAnn.Span.Contains(node) || node.Annotation.CompareTo(stemAnn) < 0)
			{
				ShapeNode rightNode = node.GetNext(NodeFilter);
				if (rightNode != null)
					right = stemAnn.Span.Contains(rightNode) ? variety.Segments[rightNode] : Segment.Anchor;
			}

			return soundClasses.FirstOrDefault(sc => sc.Matches(left, target, right));
		}

		public static SoundClass GetMatchingSoundClass(this Alignment<Word, ShapeNode> alignment, int seq, int col, Word word, IEnumerable<SoundClass> soundClasses)
		{
			ShapeNode leftNode = GetLeftNode(alignment, word, seq, col);
			Segment left = leftNode == null ? null : word.Variety.Segments[leftNode];
			Ngram target = alignment[seq, col].ToNgram(word.Variety);
			ShapeNode rightNode = GetRightNode(alignment, word, seq, col);
			Segment right = rightNode == null ? null : word.Variety.Segments[rightNode];
			return soundClasses.FirstOrDefault(sc => sc.Matches(left, target, right));
		}

		private static bool NodeFilter(ShapeNode node)
		{
			return node.Type().IsOneOf(CogFeatureSystem.VowelType, CogFeatureSystem.ConsonantType, CogFeatureSystem.AnchorType);
		}

		public static SoundContext ToSoundContext(this Alignment<Word, ShapeNode> alignment, int seq, int col, Word word, IEnumerable<SoundClass> soundClasses)
		{
			ShapeNode leftNode = GetLeftNode(alignment, word, seq, col);
			SoundClass leftEnv = leftNode == null ? null : leftNode.GetMatchingSoundClass(word.Variety, soundClasses);
			Ngram target = alignment[seq, col].ToNgram(word.Variety);
			ShapeNode rightNode = GetRightNode(alignment, word, seq, col);
			SoundClass rightEnv = rightNode == null ? null : rightNode.GetMatchingSoundClass(word.Variety, soundClasses);
			return new SoundContext(leftEnv, target, rightEnv);
		}

		private static ShapeNode GetLeftNode(Alignment<Word, ShapeNode> alignment, Word word, int seq, int col)
		{
			AlignmentCell<ShapeNode> cell = alignment[seq, col];
			ShapeNode leftNode;
			if (cell.IsNull)
			{
				int index = col - 1;
				while (index >= 0 && alignment[seq, index].Count == 0)
					index--;
				if (index >= 0)
				{
					leftNode = alignment[seq, index].Last;
					if (!NodeFilter(leftNode))
						leftNode = leftNode.GetPrev(NodeFilter);
				}
				else
				{
					leftNode = word.Shape.Begin;
				}
			}
			else
			{
				leftNode = cell.First.GetPrev(NodeFilter);
			}
			return leftNode;
		}

		private static ShapeNode GetRightNode(Alignment<Word, ShapeNode> alignment, Word word, int seq, int col)
		{
			AlignmentCell<ShapeNode> cell = alignment[seq, col];
			ShapeNode rightNode;
			if (cell.IsNull)
			{
				int index = col + 1;
				while (index < alignment.ColumnCount && alignment[seq, index].Count == 0)
					index++;
				if (index < alignment.ColumnCount)
				{
					rightNode = alignment[seq, index].First;
					if (!NodeFilter(rightNode))
						rightNode = rightNode.GetNext(NodeFilter);
				}
				else
				{
					rightNode = word.Shape.End;
				}
			}
			else
			{
				rightNode = cell.Last.GetNext(NodeFilter);
			}
			return rightNode;
		}

		public static Ngram ToNgram(this ShapeNode node, Variety variety)
		{
			return new Ngram(variety.Segments[node]);
		}

		public static Ngram ToNgram(this AlignmentCell<ShapeNode> nodes, Variety variety)
		{
			if (nodes.Count == 0)
				return new Ngram(Segment.Null);
			return new Ngram(nodes.Select(node => variety.Segments[node]));
		}

		public static string ToString(this Alignment<Word, ShapeNode> alignment, IEnumerable<string> notes)
		{
			var sb = new StringBuilder();
			List<string> notesList = notes.ToList();
			bool hasNotes = notesList.Count > 0;
			if (hasNotes)
			{
				while (notesList.Count < alignment.ColumnCount)
					notesList.Add("");
			}

			int maxPrefixLen = alignment.Prefixes.Select(p => p.StrRep()).Concat("").Max(s => s.DisplayLength());
			int[] maxColLens = Enumerable.Range(0, alignment.ColumnCount).Select(c => Enumerable.Range(0, alignment.SequenceCount)
				.Select(s => alignment[s, c].StrRep()).Concat(notesList[c]).Max(s => s.DisplayLength())).ToArray();
			int maxSuffixLen = alignment.Suffixes.Select(s => s.StrRep()).Concat("").Max(s => s.DisplayLength());
			for (int s = 0; s < alignment.SequenceCount; s++)
			{
				AppendSequence(sb, alignment.Prefixes[s].StrRep(), maxPrefixLen, Enumerable.Range(0, alignment.ColumnCount).Select(c => alignment[s, c].StrRep()), maxColLens,
					alignment.Suffixes[s].StrRep(), maxSuffixLen);
			}
			if (hasNotes)
				AppendSequence(sb, "", maxPrefixLen, notesList, maxColLens, "", maxSuffixLen);

			return sb.ToString();
		}

		private static void AppendSequence(StringBuilder sb, string prefix, int maxPrefixLen, IEnumerable<string> columns, int[] maxColLens, string suffix, int maxSuffixLen)
		{
			if (maxPrefixLen > 0)
			{
				sb.Append(prefix.PadRight(maxPrefixLen));
				sb.Append(" ");
			}

			sb.Append("|");
			int index = 0;
			foreach (string col in columns)
			{
				if (index > 0)
					sb.Append(" ");
				sb.Append(col.PadRight(maxColLens[index]));
				index++;
			}
			sb.Append("|");

			if (maxSuffixLen > 0)
			{
				sb.Append(" ");
				sb.Append(suffix.PadRight(maxSuffixLen));
			}
			sb.AppendLine();
		}

		public static int DisplayLength(this string str)
		{
			int len = 0;
			foreach (char c in str)
			{
				switch (CharUnicodeInfo.GetUnicodeCategory(c))
				{
					case UnicodeCategory.NonSpacingMark:
					case UnicodeCategory.SpacingCombiningMark:
					case UnicodeCategory.EnclosingMark:
						break;

					default:
						len++;
						break;
				}
			}
			return len;
		}

		public static IEnumerable<T> GetAllDataObjects<T>(this IBidirectionalGraph<Cluster<T>, ClusterEdge<T>> tree, Cluster<T> cluster)
		{
			if (tree.IsOutEdgesEmpty(cluster))
				return cluster.DataObjects;
			return tree.OutEdges(cluster).Aggregate((IEnumerable<T>) cluster.DataObjects, (res, edge) => res.Concat(tree.GetAllDataObjects(edge.Target)));
		}

		private static void GetMidpoint<T>(IUndirectedGraph<Cluster<T>, ClusterEdge<T>> tree, out ClusterEdge<T> midpointEdge, out double pointOnEdge, out Cluster<T> firstCluster)
		{
			Cluster<T> cluster1;
			IEnumerable<ClusterEdge<T>> path;
			GetLongestPath(tree, null, tree.Vertices.First(), 0, Enumerable.Empty<ClusterEdge<T>>(), out cluster1, out path);
			Cluster<T> cluster2;
			double deepestLen = GetLongestPath(tree, null, cluster1, 0, Enumerable.Empty<ClusterEdge<T>>(), out cluster2, out path);
			double midpoint = deepestLen / 2;

			firstCluster = cluster1;
			double totalLen = 0;
			midpointEdge = null;
			foreach (ClusterEdge<T> edge in path)
			{
				totalLen += edge.Length;
				if (totalLen > midpoint)
				{
					midpointEdge = edge;
					break;
				}
				firstCluster = edge.GetOtherVertex(firstCluster);
			}
			Debug.Assert(midpointEdge != null);

			double diff = totalLen - midpoint;
			pointOnEdge = midpointEdge.Length - diff;
		}

		public static Cluster<T> GetCenter<T>(this IUndirectedGraph<Cluster<T>, ClusterEdge<T>> tree)
		{
			ClusterEdge<T> midpointEdge;
			double pointOnEdge;
			Cluster<T> firstCluster;
			GetMidpoint(tree, out midpointEdge, out pointOnEdge, out firstCluster);
			return pointOnEdge < midpointEdge.Length - pointOnEdge ? firstCluster : midpointEdge.GetOtherVertex(firstCluster);
		}

		public static IBidirectionalGraph<Cluster<T>, ClusterEdge<T>> ToRootedTree<T>(this IUndirectedGraph<Cluster<T>, ClusterEdge<T>> tree)
		{
			ClusterEdge<T> midpointEdge;
			double pointOnEdge;
			Cluster<T> firstCluster;
			GetMidpoint(tree, out midpointEdge, out pointOnEdge, out firstCluster);

			var rootedTree = new BidirectionalGraph<Cluster<T>, ClusterEdge<T>>(false);
			if (pointOnEdge < double.Epsilon)
			{
				rootedTree.AddVertex(firstCluster);
				GenerateRootedTree(tree, null, firstCluster, rootedTree);
			}
			else
			{
				var root = new Cluster<T>();
				rootedTree.AddVertex(root);
				Cluster<T> otherCluster = midpointEdge.GetOtherVertex(firstCluster);
				rootedTree.AddVertex(otherCluster);
				rootedTree.AddEdge(new ClusterEdge<T>(root, otherCluster, midpointEdge.Length - pointOnEdge));
				GenerateRootedTree(tree, firstCluster, otherCluster, rootedTree);
				rootedTree.AddVertex(firstCluster);
				rootedTree.AddEdge(new ClusterEdge<T>(root, firstCluster, pointOnEdge));
				GenerateRootedTree(tree, otherCluster, firstCluster, rootedTree);
			}
			return rootedTree;
		}

		private static void GenerateRootedTree<T>(IUndirectedGraph<Cluster<T>, ClusterEdge<T>> unrootedTree, Cluster<T> parent, Cluster<T> node, BidirectionalGraph<Cluster<T>, ClusterEdge<T>> rootedTree)
		{
			foreach (ClusterEdge<T> edge in unrootedTree.AdjacentEdges(node).Where(e => e.GetOtherVertex(node) != parent))
			{
				Cluster<T> otherCluster = edge.GetOtherVertex(node);
				rootedTree.AddVertex(otherCluster);
				rootedTree.AddEdge(new ClusterEdge<T>(node, otherCluster, edge.Length));
				GenerateRootedTree(unrootedTree, node, otherCluster, rootedTree);
			}
		}

		private static double GetLongestPath<T>(IUndirectedGraph<Cluster<T>, ClusterEdge<T>> tree, Cluster<T> parent, Cluster<T> node, double len, IEnumerable<ClusterEdge<T>> path,
			out Cluster<T> deepestNode, out IEnumerable<ClusterEdge<T>> deepestPath)
		{
			deepestNode = node;
			deepestPath = path;
		    double maxDepth = 0;
		    foreach (ClusterEdge<T> childEdge in tree.AdjacentEdges(node).Where(e => e.GetOtherVertex(node) != parent))
		    {
		        Cluster<T> cdn;
			    IEnumerable<ClusterEdge<T>> cdp;
		        double depth = GetLongestPath(tree, node, childEdge.GetOtherVertex(node), childEdge.Length, path.Concat(childEdge), out cdn, out cdp);
		        if (depth > maxDepth)
		        {
		            deepestNode = cdn;
		            maxDepth = depth;
			        deepestPath = cdp;
		        }
		    }
		    return maxDepth + len;
		}
	}
}
