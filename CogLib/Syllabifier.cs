using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class Syllabifier : NotifyPropertyChangedBase
	{
		private readonly ObservableList<SonorityClass> _sonorityScale;
		private bool _enabled;

		public Syllabifier()
		{
			_sonorityScale = new ObservableList<SonorityClass>();
		}

		public ObservableList<SonorityClass> SonorityScale
		{
			get { return _sonorityScale; }
		}

		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;
				OnPropertyChanged("Enabled");
			}
		}

		public void Syllabify(Variety variety)
		{
			if (!_enabled)
				return;

			foreach (Word word in variety.Words)
				Syllabify(word);
		}

		public void Syllabify(Word word)
		{
			if (!_enabled)
				return;

			Shape newShape = null;
			ShapeNode start = null;
			IEnumerable<ShapeNode> nodes;
			if (SyllabifyAnnotation(word, word.Prefix, out nodes))
			{
				newShape = CreateEmptyShape(word.Shape.SpanFactory);
				newShape.AddRange(nodes);
				newShape.Annotations.Add(newShape.First, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.PrefixType).Value);
				start = newShape.Last;
			}
			if (SyllabifyAnnotation(word, word.Stem, out nodes))
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
			if (SyllabifyAnnotation(word, word.Suffix, out nodes))
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

		private bool SyllabifyAnnotation(Word word, Annotation<ShapeNode> ann, out IEnumerable<ShapeNode> nodes)
		{
			if (ann == null)
			{
				nodes = null;
				return false;
			}

			var clusters = new List<Span<ShapeNode>>();
			ShapeNode[] annNodes = word.Shape.GetNodes(ann.Span).ToArray();
			if (annNodes.Any(n => n.Type() == CogFeatureSystem.ToneLetterType || n.StrRep() == "."))
			{
				ShapeNode syllableStart = annNodes[0];
				ShapeNode node = syllableStart.GetNext(n => n.Type() == CogFeatureSystem.ToneLetterType || n.StrRep() == ".");
				while (ann.Span.Contains(node))
				{
					ProcessSyllable(word, clusters, syllableStart, node.Prev);
					syllableStart = node.Next;
					node = node.GetNext(n => n.Type() == CogFeatureSystem.ToneLetterType || n.StrRep() == ".");
				}
				ProcessSyllable(word, clusters, syllableStart, ann.Span.End);
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
						nextSonority = GetSonority(nextNode);
						if (curSonority == -1)
						{
							curSonority = GetSonority(node);
						}
						else if ((curSonority < prevSonority && curSonority < nextSonority) || (curSonority == prevSonority))
						{
							ProcessSyllable(word, clusters, syllableStart, node.Prev);
							syllableStart = null;
						}
					}
					if (syllableStart == null)
						syllableStart = node;
					prevSonority = curSonority;
					curSonority = nextSonority;
				}
				ProcessSyllable(word, clusters, syllableStart, ann.Span.End);
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

		private void ProcessSyllable(Word word, List<Span<ShapeNode>> clusters, ShapeNode startNode, ShapeNode endNode)
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
						if (word.Variety.Words.Any(w => w.Shape.StrRep().StartsWith(onsetStr)))
							break;
					}

					// TODO: ambiguous onset, what should we do?
					if (n == onsetEnd.Next)
						n = onsetStart.Next;
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

			if (node != endNode.Next && node != endNode)
				clusters.Add(spanFactory.Create(node, endNode));
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
				fs.Union(node.Annotation.FeatureStruct);
				node = node.Next;
			}
			fs.AddValue(CogFeatureSystem.StrRep, strRep.ToString());
			fs.AddValue(CogFeatureSystem.OriginalStrRep, origStrRep.ToString());
			return new ShapeNode(span.SpanFactory, fs);
		}

		private int GetSonority(ShapeNode node)
		{
			ShapeNode prevNode = node.Prev;
			Segment left = prevNode.Type() == CogFeatureSystem.AnchorType ? Segment.Anchor : new Segment(prevNode.Annotation.FeatureStruct);
			var target = new Ngram(new Segment(node.Annotation.FeatureStruct));
			ShapeNode nextNode = node.Next;
			Segment right = nextNode.Type() == CogFeatureSystem.AnchorType ? Segment.Anchor : new Segment(nextNode.Annotation.FeatureStruct);

			foreach (SonorityClass level in _sonorityScale)
			{
				if (level.SoundClass.Matches(left, target, right))
					return level.Sonority;
			}
			return 0;
		}
	}
}
