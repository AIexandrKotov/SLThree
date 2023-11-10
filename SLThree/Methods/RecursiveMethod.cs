namespace SLThree
{
    public class RecursiveMethod : Method
    {
        public override ExecutionContext GetExecutionContext(object[] arguments, ExecutionContext context = null)
        {
            var ret = new ExecutionContext();
            ret.PreviousContext = context;
            ret.LocalVariables.FillArguments(this, arguments);
            ret.fimp = !imp;
            return ret;
        }
    }
}
