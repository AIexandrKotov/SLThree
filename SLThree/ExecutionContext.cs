using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SLThree
{

#pragma warning disable IDE1006 // Стили именования
    public class ExecutionContext
    {
        public interface IExecutable : ICloneable
        {
            object GetValue(ExecutionContext context);
        }

        public T GetCasted<T>(string name) => LocalVariables.GetValue(name).Item1.Cast<T>();

        /// <summary>
        /// Имя данного контекста
        /// </summary>
        public string Name = $"@{Convert.ToString(context_count++, 16).ToUpper().PadLeft(4, '0')}";
        private static long context_count = 0;

        /// <summary>
        /// Запрещает implicit в контексте
        /// </summary>
        public bool ForbidImplicit = false;

        internal bool Returned;
        internal bool Broken;
        internal bool Continued;
        internal int Creations = 0;

        public object ReturnedValue;

        public static readonly ContextWrap global = new ContextWrap(new ExecutionContext(false) { ForbidImplicit = false, Name = "global" });

        static ExecutionContext()
        {

        }

        public ExecutionContext(bool assign_to_global, bool create_private = true, LocalVariablesContainer localVariables = null)
        {
            LocalVariables = localVariables ?? new LocalVariablesContainer();
            @this = new ContextWrap(this);
            wrap = new ContextWrap(this);
            if (create_private)
                @private = new ContextWrap(new ExecutionContext(this, false));
            if (assign_to_global) SuperContext = global.Context;
        }
        public ExecutionContext(ExecutionContext context, bool create_private = true, LocalVariablesContainer localVariables = null)
        {
            LocalVariables = localVariables ?? new LocalVariablesContainer();
            @this = new ContextWrap(this);
            wrap = new ContextWrap(this);
            if (create_private)
                @private = new ContextWrap(new ExecutionContext(this, false));
            if (context != null) SuperContext = context;
        }
        public ExecutionContext() : this(true, true, null) { }

        public ContextWrap @this;

        internal ContextWrap PreviousContext;
        //public ContextWrap pred => new ContextWrap(PreviousContext);
        public readonly ContextWrap wrap;
        public ContextWrap super;
        public ContextWrap @private;
        public ContextWrap parent;
        internal ExecutionContext SuperContext { get => super?.Context; set => super = new ContextWrap(value); }

        public IEnumerable<ExecutionContext> GetHierarchy()
        {
            var context = this;
            while (context.SuperContext != null)
            {
                yield return context.SuperContext;
                context = context.SuperContext;
            }
        }

        private int cycles = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void StartCycle()
        {
            cycles += 1;
            Broken = Continued = false;
        }

        public bool InCycle() => cycles > 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EndCycle()
        {
            cycles -= 1;
            Broken = Continued = false;
        }

        public void Return() { Returned = true; ReturnedValue = null; }
        public void Return(object o) { Returned = true; ReturnedValue = o; }
        public void Break() { Broken = true; }
        public void Continue() { Continued = true; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrepareToInvoke() { Returned = Broken = Continued = false; }
        public void DefaultEnvironment()
        {

        }

        public ExecutionContext CreateInstanceTemplate(ExecutionContext definitioncontext, TemplateMethod constructor, (TemplateMethod.GenericMaking, object)[] generic_args, object[] args)
        {
            var ret = new ExecutionContext(definitioncontext);
            ret.Name = $"{Name}@{Convert.ToString(Creations++, 16).ToUpper().PadLeft(4, '0')}";
            ret.parent = wrap;
            constructor.@this = ret.wrap;
            constructor.MakeGenericMethod(generic_args).GetValue(ret, args);
            return ret;
        }
        public ExecutionContext CreateInstanceTemplate((TemplateMethod.GenericMaking, object)[] generic_args, object[] args, SourceContext sourceContext)
        {
            var ret = new ExecutionContext(super.Context);
            ret.Name = $"{Name}@{Convert.ToString(Creations++, 16).ToUpper().PadLeft(4, '0')}";
            ret.parent = wrap;
            if (LocalVariables.GetValue("constructor").Item1 is TemplateMethod constructor)
            {
                if (constructor.ParamNames.Length != args.Length)
                    throw new WrongConstructorCallArgumentsCount(sourceContext);
                constructor.@this = ret.wrap;
                constructor.MakeGenericMethod(generic_args).GetValue(ret, args);
            }
            return ret;
        }
        public ExecutionContext CreateInstanceGeneric(ExecutionContext definitioncontext, GenericMethod constructor, Type[] generic_args, object[] args)
        {
            var ret = new ExecutionContext(definitioncontext);
            ret.Name = $"{Name}@{Convert.ToString(Creations++, 16).ToUpper().PadLeft(4, '0')}";
            ret.parent = wrap;
            constructor.@this = ret.wrap;
            constructor.MakeGenericMethod(generic_args).GetValue(ret, args);
            return ret;
        }
        public ExecutionContext CreateInstanceGeneric(Type[] generic_args, object[] args, SourceContext sourceContext)
        {
            var ret = new ExecutionContext(super.Context);
            ret.Name = $"{Name}@{Convert.ToString(Creations++, 16).ToUpper().PadLeft(4, '0')}";
            ret.parent = wrap;
            if (LocalVariables.GetValue("constructor").Item1 is GenericMethod constructor)
            {
                if (constructor.ParamNames.Length != args.Length)
                    throw new WrongConstructorCallArgumentsCount(sourceContext);
                constructor.@this = ret.wrap;
                constructor.MakeGenericMethod(generic_args).GetValue(ret, args);
            }
            return ret;
        }
        public ExecutionContext CreateInstance(ExecutionContext definitioncontext, Method constructor, object[] args)
        {
            var ret = new ExecutionContext(definitioncontext);
            ret.Name = $"{Name}@{Convert.ToString(Creations++, 16).ToUpper().PadLeft(4, '0')}";
            ret.parent = wrap;
            constructor.@this = ret.wrap;
            constructor.GetValue(ret, args);
            return ret;
        }
        public ExecutionContext CreateInstance(object[] args, SourceContext sourceContext)
        {
            var ret = new ExecutionContext(super.Context);
            ret.Name = $"{Name}@{Convert.ToString(Creations++, 16).ToUpper().PadLeft(4, '0')}";
            ret.parent = wrap;
            if (LocalVariables.GetValue("constructor").Item1 is Method constructor)
            {
                if (constructor.ParamNames.Length != args.Length)
                    throw new WrongConstructorCallArgumentsCount(sourceContext);
                constructor.@this = ret.wrap;
                constructor.GetValue(ret, args);
            }
            return ret;
        }

        public void Implementation(ExecutionContext ret, Method constructor, object[] args)
        {
            constructor.@this = ret.wrap;
            constructor.GetValue(ret, args);
        }
        public void Implementation(ExecutionContext ret, object[] args, SourceContext sourceContext)
        {
            if (LocalVariables.GetValue("constructor").Item1 is Method constructor)
            {
                if (constructor.ParamNames.Length != args.Length) 
                    throw new WrongConstructorCallArgumentsCount(sourceContext);
                constructor.@this = ret.wrap;
                constructor.GetValue(ret, args);
            }
        }

        public readonly LocalVariablesContainer LocalVariables;

        internal ExecutionContext copy(ExecutionContext context)
        {
            LocalVariables.FillOther(context.LocalVariables);
            //if (context.@private != null && @private != null)
            //    @private.Context.LocalVariables.FillOther(context.@private.Context.LocalVariables);
            return this;
        }

        public static object virtualize(object o, ExecutionContext context)
        {
            if (o is Method method)
            {
                method = method.CloneWithNewName(method.Name);
                method.identity(context.wrap);
                return method;
            }
            return o;
        }

        internal ExecutionContext implement(ExecutionContext context)
        {
            var vars = context.LocalVariables.Variables;
            foreach (var x in context.LocalVariables.NamedIdentificators)
            {
                var o = vars[x.Value];
                LocalVariables.SetValue(x.Key, virtualize(o, this));
            }
            return this;
        }
    }
#pragma warning restore IDE1006 // Стили именования
}
