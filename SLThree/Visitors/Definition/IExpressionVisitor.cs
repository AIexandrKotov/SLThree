namespace SLThree.Visitors
{
    public interface IExpressionVisitor
    {
        void VisitExpression(BaseExpression expression);

        void VisitExpression(CreatorNewArray expression);
        void VisitExpression(CreatorRange expression);
        void VisitExpression(CreatorTuple expression);
        void VisitExpression(CreatorUsing expression);
        void VisitExpression(CreatorContext expression);
        void VisitExpression(BaseInstanceCreator expression);
        void VisitExpression(CreatorDictionary expression);
        void VisitExpression(CreatorCollection expression);
        void VisitExpression(CreatorInstance expression);

        void VisitExpression(CastExpression expression);
        void VisitExpression(ConditionExpression expression);
        void VisitExpression(IndexExpression expression);
        void VisitExpression(InterpolatedString expression);
        void VisitExpression(InvokeExpression expression);
        void VisitExpression(InvokeGenericExpression expression);
        void VisitExpression(InvokeTemplateExpression expression);
        void VisitExpression(FunctionDefinition expression);
        void VisitExpression(MemberAccess expression);
        void VisitExpression(NameExpression expression);
        void VisitExpression(ReflectionExpression expression);
        void VisitExpression(TypenameExpression expression);
        void VisitExpression(MatchExpression expression);
        void VisitExpression(StaticExpression expression);
        void VisitExpression(UsingExpression expression);
        void VisitExpression(BlockExpression expression);
        void VisitExpression(ConstraintExpression expression);

        void VisitExpression(Special expression);
        void VisitExpression(Literal expression);
        void VisitExpression(UnaryOperator expression);
        void VisitExpression(BinaryOperator expression);
        void VisitExpression(TernaryOperator expression);

        void VisitExpression(FunctionArgument expression);
        void VisitExpression(InvokeTemplateExpression.GenericMakingDefinition expression);
    }
}
