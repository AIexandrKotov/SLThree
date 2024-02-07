using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class RecursiveMethod : Method
    {
        public RecursiveMethod(string name, string[] paramNames, StatementListStatement statements, TypenameExpression[] paramTypes, TypenameExpression returnType, ExecutionContext.ContextWrap definitionPlace, bool @implicit) : base(name, paramNames, statements, paramTypes, returnType, definitionPlace, @implicit)
        {
        }

        protected internal RecursiveMethod()
        {
        }

        public override ExecutionContext GetExecutionContext(object[] arguments, ExecutionContext super_context = null)
        {
            var ret = new ExecutionContext();
            ret.Name = contextName;
            ret.PreviousContext = super_context;
            ret.LocalVariables.FillArguments(this, arguments);
            ret.@this = definitionplace;
            ret.ForbidImplicit = !Implicit;
            return ret;
        }

        public override Method CloneWithNewName(string name)
        {
            return new RecursiveMethod(name, ParamNames?.CloneArray(), Statements.CloneCast(), ParamTypes?.CloneArray(), ReturnType.CloneCast(), definitionplace, Implicit);
        }
    }
}
