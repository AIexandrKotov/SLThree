namespace SLThree.Visitors
{
    public interface IExpressionVisitor
    {
        void VisitExpression(BaseExpression expression);

        void VisitExpression(CastExpression expression);
        void VisitExpression(ChanceChooseExpression expression);
        void VisitExpression(CreatorArray expression);
        void VisitExpression(CreatorDictionary expression);
        void VisitExpression(CreatorTuple expression);
        void VisitExpression(EqualchanceChooseExpression expression);
        void VisitExpression(IndexExpression expression);
        void VisitExpression(InterpolatedString expression);
        void VisitExpression(InvokeExpression expression);
        void VisitExpression(LambdaExpression expression);
        void VisitExpression(NameExpression expression);
        void VisitExpression(NewExpression expression);
        void VisitExpression(TypeofExpression expression);

        void VisitExpression(Literal expression);
        void VisitExpression(ExpressionUnary expression);
        void VisitExpression(ExpressionBinary expression);
        void VisitExpression(ExpressionTernary expression);

        void VisitExpression(MemberAccess expression);
    }
}
