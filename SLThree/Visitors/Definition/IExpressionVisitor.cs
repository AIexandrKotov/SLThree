namespace SLThree.Visitors
{
    public interface IExpressionVisitor
    {
        void VisitExpression(BaseExpression expression);

        void VisitExpression(CreatorArray expression);
        void VisitExpression(CreatorNewArray expression);
        void VisitExpression(CreatorContext expression);
        void VisitExpression(CreatorDictionary expression);
        void VisitExpression(CreatorList expression);
        void VisitExpression(CreatorRange expression);
        void VisitExpression(CreatorTuple expression);
        void VisitExpression(CreatorUsing expression);

        void VisitExpression(CastExpression expression);
        void VisitExpression(IndexExpression expression);
        void VisitExpression(InterpolatedString expression);
        void VisitExpression(InvokeExpression expression);
        void VisitExpression(InvokeGenericExpression expression);
        void VisitExpression(LambdaExpression expression);
        void VisitExpression(MemberAccess expression);
        void VisitExpression(NameExpression expression);
        void VisitExpression(NewExpression expression);
        void VisitExpression(ReflectionExpression expression);
        void VisitExpression(TypenameExpression expression);

        void VisitExpression(Literal expression);
        void VisitExpression(UnaryOperator expression);
        void VisitExpression(BinaryOperator expression);
        void VisitExpression(TernaryOperator expression);

    }
}
