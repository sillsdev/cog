using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain.Components
{
	public class SimpleSyllabifier : IProcessor<Variety>
	{
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
			if (word.Shape.GetNodes(ann.Span).Any(n => n.Type().IsOneOf(CogFeatureSystem.ToneLetterType, CogFeatureSystem.BoundaryType)))
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
			ShapeNode newStartNode = null;
			foreach (ShapeNode node in word.Shape.GetNodes(ann.Span))
			{
				ShapeNode newNode = node.DeepClone();
				if (newStartNode == null)
					newStartNode = newNode;
				newShape.Add(newNode);
			}
			newShape.Annotations.Add(newStartNode, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.SyllableType).Value);
		}

		protected void ProcessSyllable(ShapeNode startNode, ShapeNode endNode, Shape newShape)
		{
			SpanFactory<ShapeNode> spanFactory = newShape.SpanFactory;
			ShapeNode newStartNode = null;
			ShapeNode node = startNode;
			ShapeNode onsetStart = node;
			while (node.Type() == CogFeatureSystem.ConsonantType && node != endNode.Next)
				node = node.Next;
			ShapeNode onsetEnd = node.Prev;

			if (onsetStart != node)
			{
				ShapeNode onset = onsetStart != onsetEnd ? Combine(spanFactory, onsetStart, onsetEnd) : onsetStart.DeepClone();
				newShape.Add(onset);
				newStartNode = onset;
			}

			if (node != endNode.Next)
			{
				ShapeNode nucleusStart = node;
				while (node.Type() == CogFeatureSystem.VowelType && node != endNode.Next)
					node = node.Next;
				ShapeNode nucleusEnd = node.Prev;

				ShapeNode nucleus = nucleusStart != nucleusEnd ? Combine(spanFactory, nucleusStart, nucleusEnd) : nucleusStart.DeepClone();
				newShape.Add(nucleus);
				if (newStartNode == null)
					newStartNode = nucleus;
			}

			if (node != endNode.Next)
			{
				ShapeNode codaStart = node;
				while (node.Type() == CogFeatureSystem.ConsonantType && node != endNode.Next)
					node = node.Next;
				ShapeNode codaEnd = node.Prev;

				newShape.Add(codaStart != codaEnd ? Combine(spanFactory, codaStart, codaEnd) : codaStart.DeepClone());
			}
			newShape.Annotations.Add(newStartNode, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.SyllableType).Value);
		}

		protected ShapeNode Combine(SpanFactory<ShapeNode> spanFactory, ShapeNode start, ShapeNode end)
		{
			var fs = start.Annotation.FeatureStruct.DeepClone();
			var strRep = new StringBuilder();
			var origStrRep = new StringBuilder();
			strRep.Append(start.StrRep());
			origStrRep.Append(start.OriginalStrRep());
			ShapeNode node = start.Next;
			while (node != end.Next)
			{
				strRep.Append(node.StrRep());
				origStrRep.Append(node.OriginalStrRep());
				fs.Add(node.Annotation.FeatureStruct);
				node = node.Next;
			}
			fs.AddValue(CogFeatureSystem.StrRep, strRep.ToString());
			fs.AddValue(CogFeatureSystem.OriginalStrRep, origStrRep.ToString());
			return new ShapeNode(spanFactory, fs);
		}
	}
}
