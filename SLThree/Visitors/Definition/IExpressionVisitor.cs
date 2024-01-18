namespace SLThree.Visitors
{
    public interface IExpressionVisitor
    {
        void VisitExpression(BaseExpression expression);

        void VisitExpression(CastExpression expression);
        void VisitExpression(ChanceChooseExpression expression);
        void VisitExpression(CreatorList expression);
        void VisitExpression(CreatorDictionary expression);
        void VisitExpression(CreatorTuple expression);
        void VisitExpression(CreatorUsing expression);
        void VisitExpression(EqualchanceChooseExpression expression);
        void VisitExpression(IndexExpression expression);
        void VisitExpression(InterpolatedString expression);
        void VisitExpression(InvokeExpression expression);
        void VisitExpression(LambdaExpression expression);
        void VisitExpression(NameExpression expression);
        void VisitExpression(NewExpression expression);
        void VisitExpression(ReflectionExpression expression);
        void VisitExpression(TypenameExpression expression);

        void VisitExpression(Literal expression);
        void VisitExpression(UnaryOperator expression);
        void VisitExpression(BinaryOperator expression);
        void VisitExpression(TernaryOperator expression);

        void VisitExpression(MemberAccess expression);
    }
}
