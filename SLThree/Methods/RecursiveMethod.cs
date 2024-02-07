using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class RecursiveMethod : Method
    {
        public override ExecutionContext GetExecutionContext(object[] arguments, ExecutionContext context = null)
        {
            var ret = new ExecutionContext();
            ret.Name = contextName;
            ret.PreviousContext = context;
            ret.LocalVariables.FillArguments(this, arguments);
            ret.@this = DefinitionPlace;
            ret.ForbidImplicit = !Implicit;
            return ret;
        }

        public override object Clone()
        {
            return new RecursiveMethod()
            {
                DefinitionPlace = DefinitionPlace,
                Implicit = Implicit,
                Name = Name,
                ParamNames = ParamNames.CloneArray(),
                Statements = Statements.CloneCast()
            };
        }
    }
}
