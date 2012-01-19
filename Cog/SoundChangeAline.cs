using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class SoundChangeAline : Aline
	{
		private const int MaxSoundChangeScore = 20 * Precision;

		private readonly IDBearerSet<NaturalClass> _naturalClasses;

		public SoundChangeAline(SpanFactory<ShapeNode> spanFactory, IEnumerable<SymbolicFeature> relevantVowelFeatures,
			IEnumerable<SymbolicFeature> relevantConsFeatures, IEnumerable<NaturalClass> naturalClasses)
			: base(spanFactory, relevantVowelFeatures, relevantConsFeatures)
		{
			_naturalClasses = new IDBearerSet<NaturalClass>(naturalClasses);
		}

		public IEnumerable<NaturalClass> NaturalClasses
		{
			get { return _naturalClasses; }
		}

		public override int SigmaSubstitution(WordPair wordPair, ShapeNode p, ShapeNode q)
		{
			return base.SigmaSubstitution(wordPair, p, q) + SoundChange(wordPair, p, p, q, q);
		}

		public override int SigmaExpansion(WordPair wordPair, ShapeNode p, ShapeNode q1, ShapeNode q2)
		{
			return base.SigmaExpansion(wordPair, p, q1, q2) + SoundChange(wordPair, p, p, q1, q2);
		}

		public override int SigmaCompression(WordPair wordPair, ShapeNode p1, ShapeNode p2, ShapeNode q)
		{
			return base.SigmaCompression(wordPair, p1, p2, q) + SoundChange(wordPair, p1, p2, q, q);
		}

		public override int GetMaxScore(WordPair wordPair, ShapeNode node)
		{
			return base.GetMaxScore(wordPair, node) + (int) (MaxSoundChangeScore * 0.85);
		}

		private int SoundChange(WordPair wordPair, ShapeNode pStartNode, ShapeNode pEndNode, ShapeNode qStartNode, ShapeNode qEndNode)
		{
			string pStrRep = GetStrRep(pStartNode, pEndNode);
			string qStrRep = GetStrRep(qStartNode, qEndNode);
			NaturalClass leftEnv = _naturalClasses.FirstOrDefault(constraint =>
				constraint.FeatureStruct.IsUnifiable(pStartNode.GetPrev(node => node.Annotation.Type != CogFeatureSystem.NullType).Annotation.FeatureStruct));
			NaturalClass rightEnv = _naturalClasses.FirstOrDefault(constraint =>
				constraint.FeatureStruct.IsUnifiable(pEndNode.GetNext(node => node.Annotation.Type != CogFeatureSystem.NullType).Annotation.FeatureStruct));
			double prob = wordPair.VarietyPair.GetCorrespondenceProbability(leftEnv, pStrRep, rightEnv, qStrRep);
			return (int)(MaxSoundChangeScore * prob);
		}

		private string GetStrRep(ShapeNode startNode, ShapeNode endNode)
		{
			return startNode.GetNodes(endNode).Aggregate(new StringBuilder(),
				(sb, node) => sb.Append((string)node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep))).ToString();
		}
	}
}
