using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain.Components
{
	public class SspSyllabifier : IProcessor<Variety>
	{
		private readonly List<SonorityClass> _sonorityScale;

		public SspSyllabifier(IEnumerable<SonorityClass> sonorityScale)
		{
			_sonorityScale = sonorityScale.ToList();
		}

		public IEnumerable<SonorityClass> SonorityScale
		{
			get { return _sonorityScale; }
		}

		public void Process(Variety data)
		{
			var initialOnsets = new HashSet<string>();
			foreach (Word word in data.Words.Where(w => w.IsValid))
			{
				string prefixOnset = GetInitialOnset(word.Prefix);
				if (prefixOnset != null)
					initialOnsets.Add(prefixOnset);
				string stemOnset = GetInitialOnset(word.Stem);
				if (stemOnset != null)
					initialOnsets.Add(stemOnset);
			}

			foreach (Word word in data.Words)
				Syllabify(initialOnsets, word);
		}

		private string GetInitialOnset(Annotation<ShapeNode> ann)
		{
			if (ann == null)
				return null;

			ShapeNode node = ann.Span.Start;
			while (node.Type() == CogFeatureSystem.ConsonantType && ann.Span.Contains(node))
				node = node.Next;

			if (node == ann.Span.Start)
				return null;

			return ann.Span.Start.GetNodes(node.Prev).StrRep();
		}

		private void Syllabify(HashSet<string> initialOnsets, Word word)
		{
			Shape newShape = null;
			ShapeNode start = null;
			IEnumerable<ShapeNode> nodes;
			if (SyllabifyAnnotation(initialOnsets, word, word.Prefix, out nodes))
			{
				newShape = CreateEmptyShape(word.Shape.SpanFactory);
				newShape.AddRange(nodes);
				newShape.Annotations.Add(newShape.First, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.PrefixType).Value);
				start = newShape.Last;
			}
			if (SyllabifyAnnotation(initialOnsets, word, word.Stem, out nodes))
			{
				if (newShape == null)
				{
					newShape = CreateEmptyShape(word.Shape.SpanFactory);
					if (word.Prefix != null)
						word.Shape.CopyTo(word.Prefix.Span, newShape);
					start = newShape.End.Prev;
				}
				newShape.AddRange(nodes);
				newShape.Annotations.Add(start.Next, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.StemType).Value);
				start = newShape.Last;
			}
			else if (newShape != null)
			{
				word.Shape.CopyTo(word.Stem.Span, newShape);
			}
			if (SyllabifyAnnotation(initialOnsets, word, word.Suffix, out nodes))
			{
				if (newShape == null)
				{
					newShape = CreateEmptyShape(word.Shape.SpanFactory);
					if (word.Prefix != null)
						word.Shape.CopyTo(word.Prefix.Span, newShape);
					word.Shape.CopyTo(word.Stem.Span, newShape);
					start = newShape.End.Prev;
				}
				newShape.AddRange(nodes);
				newShape.Annotations.Add(start.Next, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.SuffixType).Value);
			}
			else if (word.Suffix != null && newShape != null)
			{
				word.Shape.CopyTo(word.Suffix.Span, newShape);
			}

			if (newShape != null)
			{
				newShape.Freeze();
				word.Shape = newShape;
			}
		}

		private static Shape CreateEmptyShape(SpanFactory<ShapeNode> spanFactory)
		{
			return new Shape(spanFactory, begin => new ShapeNode(spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Feature(CogFeatureSystem.StrRep).EqualTo("#").Value));
		}

		private bool SyllabifyAnnotation(HashSet<string> initialOnsets, Word word, Annotation<ShapeNode> ann, out IEnumerable<ShapeNode> nodes)
		{
			if (ann == null)
			{
				nodes = null;
				return false;
			}

			var clusters = new List<Span<ShapeNode>>();
			ShapeNode[] annNodes = word.Shape.GetNodes(ann.Span).ToArray();
			if (annNodes.Any(n => n.Type().IsOneOf(CogFeatureSystem.ToneLetterType, CogFeatureSystem.BoundaryType)))
			{
				ShapeNode syllableStart = annNodes[0];
				ShapeNode node = syllableStart.GetNext(n => n.Type().IsOneOf(CogFeatureSystem.ToneLetterType, CogFeatureSystem.BoundaryType));
				while (ann.Span.Contains(node))
				{
					ProcessSyllable(initialOnsets, word, clusters, syllableStart, node.Prev);
					syllableStart = node.Next;
					node = node.GetNext(n => n.Type().IsOneOf(CogFeatureSystem.ToneLetterType, CogFeatureSystem.BoundaryType));
				}
				ProcessSyllable(initialOnsets, word, clusters, syllableStart, ann.Span.End);
			}
			else
			{
				ShapeNode syllableStart = null;
				int prevSonority = -1, curSonority = -1, nextSonority = -1;
				foreach (ShapeNode node in annNodes)
				{
					ShapeNode nextNode = node.Next;
					if (ann.Span.Contains(nextNode))
					{
						nextSonority = GetSonority(word.Variety.SegmentPool, nextNode);
						if (curSonority == -1)
						{
							curSonority = GetSonority(word.Variety.SegmentPool, node);
						}
						else if ((curSonority < prevSonority && curSonority < nextSonority) || (curSonority == prevSonority))
						{
							ProcessSyllable(initialOnsets, word, clusters, syllableStart, node.Prev);
							syllableStart = null;
						}
					}
					if (syllableStart == null)
						syllableStart = node;
					prevSonority = curSonority;
					curSonority = nextSonority;
				}
				ProcessSyllable(initialOnsets, word, clusters, syllableStart, ann.Span.End);
			}

			if (clusters.Count > 0)
			{
				var nodesList = new List<ShapeNode>();
				ShapeNode node = ann.Span.Start;
				foreach (Span<ShapeNode> cluster in clusters)
				{
					while (node != cluster.Start)
					{
						nodesList.Add(node.DeepClone());
						node = node.Next;
					}
					nodesList.Add(Combine(cluster));
					node = cluster.End.Next;
				}

				while (node != ann.Span.End.Next)
				{
					nodesList.Add(node.DeepClone());
					node = node.Next;
				}

				nodes = nodesList;
				return true;
			}

			nodes = null;
			return false;
		}

		private void ProcessSyllable(HashSet<string> initialOnsets, Word word, List<Span<ShapeNode>> clusters, ShapeNode startNode, ShapeNode endNode)
		{
			ShapeNode node = startNode;
			ShapeNode onsetStart = node;
			while (node.Type() == CogFeatureSystem.ConsonantType && node != endNode.Next)
				node = node.Next;
			ShapeNode onsetEnd = node.Prev;
			SpanFactory<ShapeNode> spanFactory = word.Shape.SpanFactory;

			if (onsetStart != node && onsetStart != onsetEnd)
			{
				ShapeNode n = onsetStart;
				if (onsetStart != onsetEnd.List.First)
				{
					for (; n != onsetEnd.Next; n = n.Next)
					{
						string onsetStr = n.GetNodes(onsetEnd).StrRep();
						if (initialOnsets.Contains(onsetStr))
							break;
					}

					// TODO: ambiguous onset, what should we do?
					if (n == onsetEnd.Next)
						n = onsetStart;
				}
				if (n != onsetStart)
				{
					if (onsetStart.Prev.Type() == CogFeatureSystem.ConsonantType)
					{
						if (clusters.Count > 0 && clusters[clusters.Count - 1].End == onsetStart.Prev)
							clusters[clusters.Count - 1] = spanFactory.Create(clusters[clusters.Count - 1].Start, n.Prev);
						else
							clusters.Add(spanFactory.Create(onsetStart.Prev, n.Prev));
					}
					else if (n.Prev != onsetStart)
					{
						clusters.Add(spanFactory.Create(onsetStart, n.Prev));
					}
				}
				if (n != onsetEnd)
					clusters.Add(spanFactory.Create(n, onsetEnd));
			}

			if (node != endNode.Next)
			{
				ShapeNode nucleusStart = node;
				while (node.Type() == CogFeatureSystem.VowelType && node != endNode.Next)
					node = node.Next;
				ShapeNode nucleusEnd = node.Prev;

				if (nucleusStart != nucleusEnd)
					clusters.Add(spanFactory.Create(nucleusStart, nucleusEnd));
			}

			if (node != endNode.Next)
			{
				ShapeNode codaStart = node;
				while (node.Type() == CogFeatureSystem.ConsonantType && node != endNode.Next)
					node = node.Next;
				ShapeNode codaEnd = node.Prev;

				if (codaStart != codaEnd)
					clusters.Add(spanFactory.Create(codaStart, codaEnd));
			}
		}

		private ShapeNode Combine(Span<ShapeNode> span)
		{
			var fs = span.Start.Annotation.FeatureStruct.DeepClone();
			var strRep = new StringBuilder();
			var origStrRep = new StringBuilder();
			strRep.Append(span.Start.StrRep());
			origStrRep.Append(span.Start.OriginalStrRep());
			ShapeNode node = span.Start.Next;
			while (node != span.End.Next)
			{
				strRep.Append(node.StrRep());
				origStrRep.Append(node.OriginalStrRep());
				fs.Add(node.Annotation.FeatureStruct);
				node = node.Next;
			}
			fs.AddValue(CogFeatureSystem.StrRep, strRep.ToString());
			fs.AddValue(CogFeatureSystem.OriginalStrRep, origStrRep.ToString());
			return new ShapeNode(span.SpanFactory, fs);
		}

		private int GetSonority(SegmentPool segmentPool, ShapeNode node)
		{
			ShapeNode prevNode = node.Prev;
			Ngram target = segmentPool.Get(node);
			ShapeNode nextNode = node.Next;

			foreach (SonorityClass level in _sonorityScale)
			{
				if (level.SoundClass.Matches(prevNode, target, nextNode))
					return level.Sonority;
			}
			return 0;
		}
	}
}
