using Pegasus.Common;
using SLThree.Extensions;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace SLThree
{
    public class IndexLexem : BaseLexem
    {
        public BaseLexem Lexem;
        public BaseLexem[] Arguments;

        public IndexLexem(BaseLexem lexem, BaseLexem[] arguments, Cursor cursor) : base(cursor)
        {
            Lexem = lexem;
            Arguments = arguments;
        }

        private int Mode = 0; // 1 - array, 2 - list, 3 - tuple, 4 - any
        private PropertyInfo PropertyInfo;

        public override object GetValue(ExecutionContext context)
        {
            var o = Lexem.GetValue(context);

            if (o == null) return null;

            if (Mode == 0)
            {
                var type = o.GetType();
                Mode = type.IsArray ? 1 : 0;
                if (Mode == 0)
                {
                    Mode = type.IsList() ? 1 : 0;
                    if (Mode == 0)
                    {
                        while (PropertyInfo == null && type != null)
                        {
                            PropertyInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(x => x.GetIndexParameters().Length == Arguments.Length);
                            if (PropertyInfo == null) type = type.BaseType;
                        }
                        if (PropertyInfo == null) return null;
                        Mode = 4;
                    }
                }
            }

            switch (Mode)
            {
                case 1:
                    {
                        return o.Cast<IList>()[context.fimp ? Arguments[0].GetValue(context).Cast<int>() : Arguments[0].GetValue(context).CastToType(typeof(int)).Cast<int>()];
                    }
                case 4: return PropertyInfo.GetValue(o, Arguments.ConvertAll(x => x.GetValue(context)));
            }
            return null;
        }

        public override string ToString() => $"{Lexem}[{Arguments.JoinIntoString(", ")}]";
    }
}
