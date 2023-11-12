namespace SLThree.Visitors
{
    public interface ILexemVisitor
    {
        void VisitLexem(BaseLexem lexem);

        void VisitLexem(CastLexem lexem);
        void VisitLexem(ChanceChooseLexem lexem);
        void VisitLexem(CreatorArray lexem);
        void VisitLexem(CreatorDictionary lexem);
        void VisitLexem(CreatorTuple lexem);
        void VisitLexem(EqualchanceChooseLexem lexem);
        void VisitLexem(IndexLexem lexem);
        void VisitLexem(InterpolatedString lexem);
        void VisitLexem(InvokeLexem lexem);
        void VisitLexem(LambdaLexem lexem);
        void VisitLexem(NameLexem lexem);
        void VisitLexem(NewLexem lexem);
        void VisitLexem(TypeofLexem lexem);

        void VisitLexem(Literal lexem);
        void VisitLexem(ExpressionUnary lexem);
        void VisitLexem(ExpressionBinary lexem);
        void VisitLexem(ExpressionTernary lexem);

        void VisitLexem(MemberAccess lexem);
    }
}
