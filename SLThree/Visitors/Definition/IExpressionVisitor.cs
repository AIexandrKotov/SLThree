namespace SLThree.Visitors
{
    public interface IExpressionVisitor
    {
        void VisitExpression(BaseExpression expression);

        void VisitExpression(CreatorArray expression);
        void VisitExpression(CreatorNewArray expression);
        void VisitExpression(CreatorDictionary expression);
        void VisitExpression(CreatorList expression);
        void VisitExpression(CreatorRange expression);
        void VisitExpression(CreatorTuple expression);
        void VisitExpression(CreatorUsing expression);
        void VisitExpression(CreatorInstance expression);

        void VisitExpression(CastExpression expression);
        void VisitExpression(ConditionExpression expression);
        void VisitExpression(IndexExpression expression);
        void VisitExpression(InterpolatedString expression);
        void VisitExpression(InvokeExpression expression);
        void VisitExpression(InvokeGenericExpression expression);
        void VisitExpression(FunctionDefinition expression);
        void VisitExpression(MemberAccess expression);
        void VisitExpression(NameExpression expression);
        void VisitExpression(ReflectionExpression expression);
        void VisitExpression(TypenameExpression expression);
        void VisitExpression(MatchExpression expression);
        void VisitExpression(StaticExpression expression);

        void VisitExpression(Special expression);
        void VisitExpression(Literal expression);
        void VisitExpression(UnaryOperator expression);
        void VisitExpression(BinaryOperator expression);
        void VisitExpression(TernaryOperator expression);

    }
}
