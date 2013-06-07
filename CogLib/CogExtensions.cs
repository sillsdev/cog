using System.Collections.Generic;
using System.Linq;
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
			return string.Concat(ann.Span.Start.GetNodes(ann.Span.End).Select(node => node.OriginalStrRep()));
		}

		public static string StrRep(this ShapeNode node)
		{
			return (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
		}

		public static string StrRep(this Annotation<ShapeNode> ann)
		{
			return string.Concat(ann.Span.Start.GetNodes(ann.Span.End).Select(node => node.StrRep()));
		}

		public static FeatureSymbol Type(this ShapeNode node)
		{
			return (FeatureSymbol) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.Type);
		}

		public static FeatureSymbol Type(this Annotation<ShapeNode> ann)
		{
			return (FeatureSymbol) ann.FeatureStruct.GetValue(CogFeatureSystem.Type);
		}

		public static SoundContext Sound(this ShapeNode node, Variety variety, IEnumerable<SoundClass> soundClasses)
		{
			Ngram target = node.Ngram(variety);

			SoundClass leftEnv = null, rightEnv = null;
			Annotation<ShapeNode> prev = node.GetPrev(n => node.Type() != CogFeatureSystem.NullType).Annotation;
			Annotation<ShapeNode> next = node.GetNext(n => node.Type() != CogFeatureSystem.NullType).Annotation;
			foreach (SoundClass soundClass in soundClasses)
			{
				if (leftEnv == null && soundClass.Matches(prev))
					leftEnv = soundClass;
				if (rightEnv == null && soundClass.Matches(next))
					rightEnv = soundClass;
				if (leftEnv != null && rightEnv != null)
					break;
			}

			return new SoundContext(leftEnv, target, rightEnv);
		}

		public static SoundContext Sound(this Annotation<ShapeNode> ann, Variety variety, IEnumerable<SoundClass> soundClasses)
		{
			Ngram target = ann.Ngram(variety);

			SoundClass leftEnv = null, rightEnv = null;
			Annotation<ShapeNode> prev = ann.Span.Start.GetPrev(n => n.Type() != CogFeatureSystem.NullType).Annotation;
			Annotation<ShapeNode> next = ann.Span.End.GetNext(n => n.Type() != CogFeatureSystem.NullType).Annotation;
			foreach (SoundClass soundClass in soundClasses)
			{
				if (leftEnv == null && soundClass.Matches(prev))
					leftEnv = soundClass;
				if (rightEnv == null && soundClass.Matches(next))
					rightEnv = soundClass;
				if (leftEnv != null && rightEnv != null)
					break;
			}
			return new SoundContext(leftEnv, target, rightEnv);
		}

		public static Ngram Ngram(this ShapeNode node, Variety variety)
		{
			return node.Type() == CogFeatureSystem.NullType ? new Ngram(Segment.Null) : new Ngram(variety.Segments[node]);
		}

		public static Ngram Ngram(this Annotation<ShapeNode> ann, Variety variety)
		{
			return ann.Type() == CogFeatureSystem.NullType ? new Ngram(Segment.Null)
				: new Ngram(ann.Span.Start.GetNodes(ann.Span.End).Select(node => variety.Segments[node]));
		}
	}
}
