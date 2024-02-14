using SLThree.Extensions;
using System;
using System.Reflection.Emit;

namespace SLThree.JIT
{
    public class AbstractNameInfo
    {
        public string Name;
        public Type Type;
        public NameType NameType;
        public int Index;

        public override string ToString() => $"{NameType}{Index} {Type.GetTypeString()} {Name}";

        public static void LoadLocal(ILGenerator generator, int index)
        {
            switch (index)
            {
                case 0:
                    generator.Emit(OpCodes.Ldloc_0);
                    return;
                case 1:
                    generator.Emit(OpCodes.Ldloc_1);
                    return;
                case 2:
                    generator.Emit(OpCodes.Ldloc_2);
                    return;
                case 3:
                    generator.Emit(OpCodes.Ldloc_3);
                    return;
                default:
                    generator.Emit(OpCodes.Ldloc, index);
                    return;
            }
        }
        public static void SetLocal(ILGenerator generator, int index)
        {
            switch (index)
            {
                case 0:
                    generator.Emit(OpCodes.Stloc_0);
                    return;
                case 1:
                    generator.Emit(OpCodes.Stloc_1);
                    return;
                case 2:
                    generator.Emit(OpCodes.Stloc_2);
                    return;
                case 3:
                    generator.Emit(OpCodes.Stloc_3);
                    return;
                default:
                    generator.Emit(OpCodes.Stloc, index);
                    return;
            }
        }

        public void EmitGet(ILGenerator generator)
        {
            switch (NameType)
            {
                case NameType.Parameter:
                    switch (Index)
                    {
                        case 0:
                            generator.Emit(OpCodes.Ldarg_0);
                            return;
                        case 1:
                            generator.Emit(OpCodes.Ldarg_1);
                            return;
                        case 2:
                            generator.Emit(OpCodes.Ldarg_2);
                            return;
                        case 3:
                            generator.Emit(OpCodes.Ldarg_3);
                            return;
                        default:
                            generator.Emit(OpCodes.Ldarg, Index);
                            return;
                    }
                case NameType.Local:
                    LoadLocal(generator, Index); return;
            }
        }
        public void EmitSet(ILGenerator generator)
        {
            switch (NameType)
            {
                case NameType.Parameter:
                    generator.Emit(OpCodes.Starg, Index);
                    return;
                case NameType.Local:
                    SetLocal(generator, Index);
                    return;
            }
        }
    }
}
