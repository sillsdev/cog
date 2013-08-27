using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain.Components
{
	public class SspSyllabifier : SimpleSyllabifier
	{
		private readonly SegmentPool _segmentPool;
		private readonly List<SonorityClass> _sonorityScale;
		private readonly ThreadLocal<HashSet<string>> _initialOnsets; 

		public SspSyllabifier(SegmentPool segmentPool, IEnumerable<SonorityClass> sonorityScale)
		{
			_segmentPool = segmentPool;
			_sonorityScale = sonorityScale.ToList();
			_initialOnsets = new ThreadLocal<HashSet<string>>();
		}

		public IEnumerable<SonorityClass> SonorityScale
		{
			get { return _sonorityScale; }
		}

		public override void Process(Variety data)
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
			_initialOnsets.Value = initialOnsets;

			base.Process(data);

			_initialOnsets.Value = null;
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

		protected override void SyllabifyUnmarkedAnnotation(Word word, Annotation<ShapeNode> ann, Shape newShape)
		{
			ShapeNode[] annNodes = word.Shape.GetNodes(ann.Span).ToArray();
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
						ProcessSyllableWithMaximalOnset(syllableStart, node.Prev, newShape);
						syllableStart = null;
					}
				}
				if (syllableStart == null)
					syllableStart = node;
				prevSonority = curSonority;
				curSonority = nextSonority;
			}
			ProcessSyllableWithMaximalOnset(syllableStart, ann.Span.End, newShape);
		}

		private void ProcessSyllableWithMaximalOnset(ShapeNode startNode, ShapeNode endNode, Shape newShape)
		{
			ShapeNode node = startNode;
			ShapeNode onsetStart = node;
			while (node.Type() == CogFeatureSystem.ConsonantType && node != endNode.Next)
				node = node.Next;
			ShapeNode onsetEnd = node.Prev;
			if (onsetStart != node && onsetStart != onsetEnd)
			{
				ShapeNode n = onsetStart;
				if (onsetStart != onsetEnd.List.First)
				{
					for (; n != onsetEnd.Next; n = n.Next)
					{
						string onsetStr = n.GetNodes(onsetEnd).StrRep();
						if (_initialOnsets.Value.Contains(onsetStr))
							break;
					}

					// TODO: ambiguous onset, what should we do? For now, we just assume maximal onset
					if (n == onsetEnd.Next)
						n = onsetStart;
				}
				if (n != onsetStart)
				{
					if (onsetStart.Prev.Type() == CogFeatureSystem.ConsonantType)
					{
						CombineWith(newShape.GetLast(nd => nd.Type() == CogFeatureSystem.ConsonantType), onsetStart, n.Prev);
					}
					else
					{
						newShape.Add(onsetStart != n.Prev ? Combine(newShape.SpanFactory, onsetStart, n.Prev) : onsetStart.DeepClone());
						Annotation<ShapeNode> prevSyllableAnn = newShape.Annotations.Last(ann => ann.Type() == CogFeatureSystem.SyllableType);
						prevSyllableAnn.Remove();
						newShape.Annotations.Add(prevSyllableAnn.Span.Start, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.SyllableType).Value);
					}
					startNode = n;
				}
			}

			ProcessSyllable(startNode, endNode, newShape);
		}

		private void CombineWith(ShapeNode node, ShapeNode start, ShapeNode end)
		{
			var strRep = new StringBuilder();
			var origStrRep = new StringBuilder();
			strRep.Append(node.StrRep());
			origStrRep.Append(node.OriginalStrRep());
			ShapeNode n = start;
			while (n != end.Next)
			{
				strRep.Append(n.StrRep());
				origStrRep.Append(n.OriginalStrRep());
				node.Annotation.FeatureStruct.Add(n.Annotation.FeatureStruct);
				n = n.Next;
			}
			node.Annotation.FeatureStruct.AddValue(CogFeatureSystem.StrRep, strRep.ToString());
			node.Annotation.FeatureStruct.AddValue(CogFeatureSystem.OriginalStrRep, origStrRep.ToString());
			node.Annotation.FeatureStruct.AddValue(CogFeatureSystem.SegmentType, CogFeatureSystem.Complex);
		}

		private int GetSonority(ShapeNode node)
		{
			ShapeNode prevNode = node.Prev;
			Ngram target = _segmentPool.Get(node);
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
