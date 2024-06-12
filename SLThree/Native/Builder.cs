using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Native
{
    public static class Builder
    {
        public static DynamicMethod Build(Method method, ExecutionContext context)
        {
            Support.CheckOnSupporting(method);
            var ret_type = default(Type);
            if (method.ReturnType != null)
                ret_type = method.ReturnType.GetValue(context).Cast<Type>();
            else ret_type = TypeInferencer.ReconstructReturnType(method);

            var dyn_method = new DynamicMethod(
                method.Name,
                ret_type,
                method.ParamTypes.ConvertAll(x => x.GetValue(context).Cast<Type>()),
                true);
            var ngen = new NETGenerator(method, context, dyn_method.GetILGenerator());
            ngen.Visit(method);

            return dyn_method;
        }
    }
}
