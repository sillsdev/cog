using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain.Components
{
	public class SspSyllabifier : SimpleSyllabifier
	{
		private readonly SegmentPool _segmentPool;
		private readonly List<SonorityClass> _sonorityScale;
		private readonly ThreadLocal<HashSet<string>> _initialOnsets; 

		public SspSyllabifier(bool combineSegments, SegmentPool segmentPool, IEnumerable<SonorityClass> sonorityScale)
			: base(combineSegments)
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
			bool useMaximalOnset = false;
			foreach (ShapeNode node in annNodes)
			{
				ShapeNode nextNode = node.Next;
				if (ann.Span.Contains(nextNode))
				{
					nextSonority = GetSonority(nextNode);
					// stress markers indicate a syllable break
					if (syllableStart != null && node.StrRep().IsOneOf("ˈ", "ˌ"))
					{
						if (useMaximalOnset)
							ProcessSyllableWithMaximalOnset(syllableStart, node.Prev, newShape);
						else
							ProcessSyllable(syllableStart, node.Prev, newShape);
						useMaximalOnset = false;
						curSonority = -1;
						syllableStart = null;
					}
					else if (curSonority == -1)
					{
						curSonority = GetSonority(node);
					}
					else if ((curSonority < prevSonority && curSonority < nextSonority) || (curSonority == prevSonority))
					{
						if (useMaximalOnset)
							ProcessSyllableWithMaximalOnset(syllableStart, node.Prev, newShape);
						else
							ProcessSyllable(syllableStart, node.Prev, newShape);
						useMaximalOnset = true;
						syllableStart = null;
					}
				}
				if (syllableStart == null)
					syllableStart = node;
				prevSonority = curSonority;
				curSonority = nextSonority;
			}
			if (useMaximalOnset)
				ProcessSyllableWithMaximalOnset(syllableStart, ann.Span.End, newShape);
			else
				ProcessSyllable(syllableStart, ann.Span.End, newShape);
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
						Combine(CogFeatureSystem.Coda, newShape, onsetStart, n.Prev);
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
			if (CombineSegments)
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

				FeatureStruct firstFS;
				if (start.IsComplex())
				{
					firstFS = start.Annotation.FeatureStruct.GetValue(CogFeatureSystem.First);
				}
				else
				{
					firstFS = new FeatureStruct();
					foreach (Feature feature in start.Annotation.FeatureStruct.Features.Where(f => !CogFeatureSystem.Instance.ContainsFeature(f)))
						firstFS.AddValue(feature, start.Annotation.FeatureStruct.GetValue(feature));
				}
				node.Annotation.FeatureStruct.AddValue(CogFeatureSystem.First, firstFS);
			}
			else
			{
				ShapeNode n = start;
				while (n != end.Next)
				{
					var newNode = n.DeepClone();
					node.AddAfter(newNode);
					node = newNode;
					n = n.Next;
				}
			}
		}

		private int GetSonority(ShapeNode node)
		{
			ShapeNode prevNode = node.Prev;
			Ngram<Segment> target = _segmentPool.Get(node);
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
