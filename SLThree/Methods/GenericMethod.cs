using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using SLThree.Visitors;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using static SLThree.GenericMethod.GenericInfo;

namespace SLThree
{
    public class GenericMethod : Method
    {
        public abstract class GenericInfo
        {
            public virtual BaseExpression Placement { get; }
            public int GenericPosition;

            public abstract ref TypenameExpression GetPlacer();
            public virtual void MakeGeneric(TypenameExpression newtypeexpr) => GetPlacer() = newtypeexpr;

            private static Dictionary<Type, ConstructorInfo> kvs;
            static GenericInfo()
            {
                kvs = typeof(GenericInfo)
                    .GetNestedTypes()
                    .Where(x => !x.GetInterfaces().Contains(typeof(IComplexedGenericInfo)))
                    .Select(x => (x, x.BaseType?.GetGenericArguments()[0]))
                    .ToDictionary(x => x.Item2, x => x.x.GetConstructor(new Type[2] { x.Item2, typeof(int) }));
            }
            public static GenericInfo GetGenericInfo(BaseExpression expression, int pos)
            {
                var type = expression.GetType();
                if (kvs.TryGetValue(type, out var info))
                {
                    return info.Invoke(new object[2] { expression, pos }) as GenericInfo;
                }
                else throw new RuntimeError($"Generics not supported in {type.GetTypeString()}", expression.SourceContext);
            }

            private interface IComplexedGenericInfo { }
            public class Typename : GenericInfo<TypenameExpression>, IComplexedGenericInfo
            {
                public int Position;

                public Typename(TypenameExpression concrete, int position, int position_at) : base(concrete, position)
                {
                    Position = position_at;
                }

                public override ref TypenameExpression GetPlacer() => ref Concrete.Generics[Position];
            }
            public class InvokeGenericExpression : GenericInfo<SLThree.InvokeGenericExpression>, IComplexedGenericInfo
            {
                public int Position;

                public InvokeGenericExpression(SLThree.InvokeGenericExpression concrete, int position, int position_at) : base(concrete, position)
                {
                    Position = position_at;
                }

                public override ref TypenameExpression GetPlacer() => ref Concrete.GenericArguments[Position];
            }
            public class Reflection : GenericInfo<ReflectionExpression>, IComplexedGenericInfo
            {
                public bool GenericArgument;
                public int Position;

                public Reflection(ReflectionExpression concrete, int position, bool generic, int position_at) : base(concrete, position)
                {
                    GenericArgument = generic;
                    Position = position_at;
                }

                public override void MakeGeneric(TypenameExpression newtypeexpr)
                {
                    if (GenericArgument)
                        Concrete.MethodGenericArguments[Position] = newtypeexpr;
                    else
                        Concrete.MethodArguments[Position] = newtypeexpr;
                }

                public override ref TypenameExpression GetPlacer()
                {
                    throw new NotImplementedException();
                }
            }


            public class CastGenericInfo : GenericInfo<CastExpression>
            {
                public CastGenericInfo(CastExpression concrete, int position) : base(concrete, position)
                {
                }

                public override ref TypenameExpression GetPlacer() => ref Concrete.Type;
            }
            public class New : GenericInfo<NewExpression>
            {
                public New(NewExpression concrete, int position) : base(concrete, position)
                {
                }

                public override ref TypenameExpression GetPlacer() => ref Concrete.Typename;
            }
            public class ArrayCreator : GenericInfo<CreatorArray>
            {
                public ArrayCreator(CreatorArray concrete, int position) : base(concrete, position)
                {
                }

                public override ref TypenameExpression GetPlacer() => ref Concrete.ListType;
            }
            public class ListCreator : GenericInfo<CreatorList>
            {
                public ListCreator(CreatorList concrete, int position) : base(concrete, position)
                {
                }

                public override ref TypenameExpression GetPlacer() => ref Concrete.ListType;
            }
            public class RangeCreator : GenericInfo<CreatorRange>
            {
                public RangeCreator(CreatorRange concrete, int position) : base(concrete, position)
                {
                }

                public override ref TypenameExpression GetPlacer() => ref Concrete.RangeType;
            }
            public class ContextCreator : GenericInfo<CreatorContext>
            {
                public ContextCreator(CreatorContext concrete, int position) : base(concrete, position)
                {
                }

                public override ref TypenameExpression GetPlacer() => ref Concrete.Typecast;
            }
            public class Chooser : GenericInfo<UnaryGetChooser>
            {
                public Chooser(UnaryGetChooser concrete, int position) : base(concrete, position)
                {
                }

                public override ref TypenameExpression GetPlacer() => ref Concrete.Typename;
            }

            public class Is : GenericInfo<BinaryIs>
            {
                public Is(BinaryIs concrete, int position) : base(concrete, position)
                {
                }

                public override void MakeGeneric(TypenameExpression newtypeexpr)
                {
                    Concrete.Right = newtypeexpr;
                }

                public override ref TypenameExpression GetPlacer()
                {
                    throw new NotImplementedException();
                }
            }
            public class Reflect : GenericInfo<UnaryReflection>
            {
                public Reflect(UnaryReflection concrete, int position) : base(concrete, position)
                {
                }

                public override void MakeGeneric(TypenameExpression newtypeexpr)
                {
                    Concrete.Left = newtypeexpr;
                }

                public override ref TypenameExpression GetPlacer()
                {
                    throw new NotImplementedException();
                }
            }
            public class StaticReflect : GenericInfo<UnaryStaticReflection>
            {
                public StaticReflect(UnaryStaticReflection concrete, int position) : base(concrete, position)
                {
                }

                public override void MakeGeneric(TypenameExpression newtypeexpr)
                {
                    Concrete.Left = newtypeexpr;
                }

                public override ref TypenameExpression GetPlacer()
                {
                    throw new NotImplementedException();
                }
            }
            public class Name : GenericInfo<NameExpression>
            {
                public Name(NameExpression concrete, int position) : base(concrete, position)
                {
                }

                public override ref TypenameExpression GetPlacer() => ref Concrete.TypeHint;
            }
        }
        public abstract class GenericInfo<T>: GenericInfo where T : BaseExpression
        {
            public T Concrete;
            public sealed override BaseExpression Placement => Concrete;
            public GenericInfo(T concrete, int position)
            {
                Concrete = concrete;
                GenericPosition = position;
            }
        }
        public class GenericMethodSignature : GenericInfo
        {
            public GenericMethod Method;
            public int position;
            public override ref TypenameExpression GetPlacer()
            {
                if (position == -1) return ref Method.ReturnType;
                else return ref Method.ParamTypes[position];
            }
        }

        internal class GenericFinder : AbstractVisitor
        {
            public string[] Generics;
            public GenericMethod Method;

            public List<GenericInfo> Infos = new List<GenericInfo>();

            public override void VisitExpression(TypenameExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (expression.Typename is NameExpression name && (name.Name == Generics[i]))
                    {
                        if (Executables.Count == 0)
                        {
                            Infos.Add(new GenericMethodSignature()
                            {
                                Method = Method,
                                GenericPosition = i,
                                position = Method.ReturnType == expression ? -1 : Array.IndexOf(Method.ParamTypes, expression)
                            });
                            break;
                        }
                        var place = Executables[Executables.Count - 1] as BaseExpression;
                        if (place == expression) place = Executables[Executables.Count - 2] as BaseExpression;
                        switch (place)
                        {
                            case TypenameExpression typename:
                                for (var j = 0; j < typename.Generics.Length; j++)
                                    if (typename.Generics[j].Typename?.Cast<NameExpression>().Name == Generics[i])
                                    {
                                        Infos.Add(new Typename(typename, i, j));
                                    }
                                break;
                            case InvokeGenericExpression invokeGeneric:
                                for (var j = 0; j < invokeGeneric.GenericArguments.Length; j++)
                                    if (invokeGeneric.GenericArguments[j].Typename?.Cast<NameExpression>().Name == Generics[i])
                                    {
                                        Infos.Add(new GenericInfo.InvokeGenericExpression(invokeGeneric, i, j));
                                    }
                                break;
                            case ReflectionExpression reflection:
                                if (reflection.MethodGenericArguments != null)
                                    for (var j = 0; j < reflection.MethodGenericArguments.Length; j++)
                                        if (reflection.MethodGenericArguments[j].Typename?.Cast<NameExpression>().Name == Generics[i])
                                        {
                                            Infos.Add(new Reflection(reflection, i, true, j));
                                        }
                                if (reflection.MethodArguments != null)
                                    for (var j = 0; j < reflection.MethodArguments.Length; j++)
                                        if (reflection.MethodArguments[j].Typename?.Cast<NameExpression>().Name == Generics[i])
                                        {
                                            Infos.Add(new Reflection(reflection, i, false, j));
                                        }
                                break;
                            default:
                                Infos.Add(GetGenericInfo(place, i));
                                break;
                        }
                    }
                    base.VisitExpression(expression);
                }
            }

            public override void Visit(Method method)
            {
                if (Method != method) return;
                for (var i = 0; i < method.ParamTypes.Length; i++)
                    if (method.ParamTypes[i] != null)
                        VisitExpression(method.ParamTypes[i]);
                if (method.ReturnType != null)
                    VisitExpression(method.ReturnType);
                base.Visit(method);
            }

            public override void Visit(GenericMethod method)
            {
                if (Method != method) return;
                for (var i = 0; i < method.ParamTypes.Length; i++)
                    if (method.ParamTypes[i] != null)
                        VisitExpression(method.ParamTypes[i]);
                if (method.ReturnType != null)
                    VisitExpression(method.ReturnType);
                base.Visit(method);
            }

            public static List<GenericInfo> FindAll(GenericMethod method)
            {
                var gf = new GenericFinder();
                gf.Generics = method.Generics.ConvertAll(x => x.Name);
                gf.Method = method;
                gf.Visit(method);
                return gf.Infos;
            }
        }

        public GenericMethod(string name, string[] paramNames, StatementList statements, TypenameExpression[] paramTypes, TypenameExpression returnType, ContextWrap definitionPlace, bool @implicit, bool recursive, NameExpression[] generics) : base(name, paramNames, statements, paramTypes, returnType, definitionPlace, @implicit, recursive)
        {
            Generics = generics;
            GenericsInfo = GenericFinder.FindAll(this);
            DefinitionParamTypes = ParamTypes.CloneArray();
            DefinitionReturnType = ReturnType.CloneCast();
        }

        public readonly TypenameExpression[] DefinitionParamTypes;
        public readonly TypenameExpression DefinitionReturnType;

        public Method MakeGenericMethod(Type[] args)
        {
            var argst = args.ConvertAll(x => new TypenameGenericPart() { Type = x });
            foreach (var x in GenericsInfo)
                x.MakeGeneric(argst[x.GenericPosition]);
            return base.CloneWithNewName(Name);
        }

        public readonly NameExpression[] Generics;
        public override string ToString() => $"{DefinitionReturnType?.ToString() ?? "any"} {Name}<{Generics.JoinIntoString(", ")}>({DefinitionParamTypes.ConvertAll(x => x?.ToString() ?? "any").JoinIntoString(", ")})";

        public List<GenericInfo> GenericsInfo;

        public override Method CloneWithNewName(string name)
        {
            return new GenericMethod(name, ParamNames?.CloneArray(), Statements.CloneCast(), ParamTypes?.CloneArray(), ReturnType.CloneCast(), definitionplace, Implicit, Recursive, Generics.CloneArray());
        }
    }
}
