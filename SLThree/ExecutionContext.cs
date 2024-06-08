﻿using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SLThree
{

#pragma warning disable IDE1006 // Стили именования
    public class ExecutionContext
    {
        public interface IExecutable
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

        public readonly List<Exception> Errors = new List<Exception>();

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

        internal ExecutionContext PreviousContext;
        //public ContextWrap pred => new ContextWrap(PreviousContext);
        public readonly ContextWrap wrap;
        public ContextWrap super;
        public ContextWrap @private;
        public ContextWrap @base;
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

        public ExecutionContext CreateInstance(Method constructor, object[] args)
        {
            var ret = new ExecutionContext(super.Context);
            ret.Name = $"{Name}@{Convert.ToString(Creations++, 16).ToUpper().PadLeft(4, '0')}";
            ret.@base = wrap;
            constructor.Constructor = true;
            constructor.definitionplace = ret.wrap;
            constructor.GetValue(ret, args);
            return ret;
        }

        public readonly LocalVariablesContainer LocalVariables;
    }
#pragma warning restore IDE1006 // Стили именования
}
