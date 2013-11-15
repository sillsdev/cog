using System.Diagnostics;
using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain.Components
{
	public class SimpleSyllabifier : IProcessor<Variety>
	{
		private readonly bool _combineVowels;
		private readonly bool _combineConsonants;

		public SimpleSyllabifier(bool combineVowels, bool combineConsonants)
		{
			_combineVowels = combineVowels;
			_combineConsonants = combineConsonants;
		}

		public bool CombineVowels
		{
			get { return _combineVowels; }
		}

		public bool CombineConsonants
		{
			get { return _combineConsonants; }
		}

		public virtual void Process(Variety data)
		{
			foreach (Word word in data.Words)
				Syllabify(word);
		}

		private void Syllabify(Word word)
		{
			if (word.Shape.Count == 0)
				return;

			var newShape = new Shape(word.Shape.SpanFactory, begin => new ShapeNode(word.Shape.SpanFactory, FeatureStruct.New()
				.Symbol(CogFeatureSystem.AnchorType)
				.Feature(CogFeatureSystem.StrRep).EqualTo("#").Value));

			ShapeNode start = null;
			Annotation<ShapeNode> prefix = word.Prefix;
			if (prefix != null)
			{
				SyllabifyAnnotation(word, prefix, newShape);
				newShape.Annotations.Add(newShape.First, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.PrefixType).Value);
				start = newShape.Last;
			}

			SyllabifyAnnotation(word, word.Stem, newShape);
			if (start == null)
				start = newShape.Begin;
			newShape.Annotations.Add(start.Next, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.StemType).Value);
			start = newShape.Last;

			Annotation<ShapeNode> suffix = word.Suffix;
			if (suffix != null)
			{
				SyllabifyAnnotation(word, suffix, newShape);
				newShape.Annotations.Add(start.Next, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.SuffixType).Value);
			}

			newShape.Freeze();
			word.Shape = newShape;
		}

		private void SyllabifyAnnotation(Word word, Annotation<ShapeNode> ann, Shape newShape)
		{
			if (word.Shape.GetNodes(ann.Span).Any(n => n.Type() == CogFeatureSystem.ToneLetterType || n.StrRep() == "."))
			{
				ShapeNode[] annNodes = word.Shape.GetNodes(ann.Span).ToArray();
				int i;
				for (i = 0; i < annNodes.Length && annNodes[i].Type().IsOneOf(CogFeatureSystem.ToneLetterType, CogFeatureSystem.BoundaryType); i++)
					newShape.Add(annNodes[i].DeepClone());
				ShapeNode syllableStart = annNodes[i];
				ShapeNode node = syllableStart.GetNext(n => n.Type().IsOneOf(CogFeatureSystem.ToneLetterType, CogFeatureSystem.BoundaryType));
				while (ann.Span.Contains(node))
				{
					if (syllableStart != node)
						ProcessSyllable(syllableStart, node.Prev, newShape);
					newShape.Add(node.DeepClone());
					syllableStart = node.Next;
					node = node.GetNext(n => n.Type().IsOneOf(CogFeatureSystem.ToneLetterType, CogFeatureSystem.BoundaryType));
				}
				if (ann.Span.Contains(syllableStart))
					ProcessSyllable(syllableStart, ann.Span.End, newShape);
			}
			else
			{
				SyllabifyUnmarkedAnnotation(word, ann, newShape);
			}
		}

		protected virtual void SyllabifyUnmarkedAnnotation(Word word, Annotation<ShapeNode> ann, Shape newShape)
		{
			ProcessSyllable(ann.Span.Start, ann.Span.End, newShape);
		}

		protected void ProcessSyllable(ShapeNode startNode, ShapeNode endNode, Shape newShape)
		{
			ShapeNode newStartNode = null;
			ShapeNode node = startNode;

			while (node.Type() == CogFeatureSystem.BoundaryType && node != endNode.Next)
			{
				ShapeNode newNode = node.DeepClone();
				newShape.Add(newNode);
				if (newStartNode == null)
					newStartNode = newNode;
				node = node.Next;
			}

			ShapeNode onsetStart = node;
			while (node.Type() == CogFeatureSystem.ConsonantType && node != endNode.Next)
				node = node.Next;
			ShapeNode onsetEnd = node.Prev;

			if (onsetStart != node)
			{
				ShapeNode start = Combine(CogFeatureSystem.Onset, newShape, onsetStart, onsetEnd);
				if (newStartNode == null)
					newStartNode = start;
			}

			if (node != endNode.Next)
			{
				ShapeNode nucleusStart = node;
				while (node.Type() == CogFeatureSystem.VowelType && node != endNode.Next)
					node = node.Next;
				ShapeNode nucleusEnd = node.Prev;

				ShapeNode start = Combine(CogFeatureSystem.Nucleus, newShape, nucleusStart, nucleusEnd);
				if (newStartNode == null)
					newStartNode = start;
			}

			if (node != endNode.Next)
			{
				ShapeNode codaStart = node;
				while (node.Type() == CogFeatureSystem.ConsonantType && node != endNode.Next)
					node = node.Next;
				ShapeNode codaEnd = node.Prev;

				Combine(CogFeatureSystem.Coda, newShape, codaStart, codaEnd);
			}

			while (node != endNode.Next)
			{
				newShape.Add(node.DeepClone());
				node = node.Next;
			}
			newShape.Annotations.Add(newStartNode, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.SyllableType).Value);
		}

		protected ShapeNode Combine(FeatureSymbol syllablePosition, Shape newShape, ShapeNode start, ShapeNode end)
		{
			ShapeNode newStart = null;
			if (start == end)
			{
				newStart = start.DeepClone();
				newStart.Annotation.FeatureStruct.AddValue(CogFeatureSystem.SyllablePosition, syllablePosition);
				newShape.Add(newStart);
			}
			else if ((_combineVowels && syllablePosition == CogFeatureSystem.Nucleus) || (_combineConsonants && syllablePosition != CogFeatureSystem.Nucleus))
			{
				var fs = start.Annotation.FeatureStruct.DeepClone();
				var strRep = new StringBuilder();
				var origStrRep = new StringBuilder();
				strRep.Append(start.StrRep());
				origStrRep.Append(start.OriginalStrRep());
				ShapeNode node = start.Next;
				bool isComplex = false;
				while (node != end.Next)
				{
					strRep.Append(node.StrRep());
					origStrRep.Append(node.OriginalStrRep());
					fs.Add(node.Annotation.FeatureStruct);
					node = node.Next;
					isComplex = true;
				}
				fs.AddValue(CogFeatureSystem.StrRep, strRep.ToString());
				fs.AddValue(CogFeatureSystem.OriginalStrRep, origStrRep.ToString());
				fs.AddValue(CogFeatureSystem.SegmentType, isComplex ? CogFeatureSystem.Complex : CogFeatureSystem.Simple);
				fs.AddValue(CogFeatureSystem.SyllablePosition, syllablePosition);
				if (isComplex)
				{
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
					fs.AddValue(CogFeatureSystem.First, firstFS);
				}
				newStart = newShape.Add(fs);
			}
			else
			{
				ShapeNode node = start;
				while (node != end.Next)
				{
					var newNode = node.DeepClone();
					newNode.Annotation.FeatureStruct.AddValue(CogFeatureSystem.SyllablePosition, syllablePosition);
					newShape.Add(newNode);
					if (newStart == null)
						newStart = newNode;
					node = node.Next;
				}
			}

			Debug.Assert(newStart != null);
			return newStart;
		}
	}
}
