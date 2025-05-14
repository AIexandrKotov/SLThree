using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SLThree.Extensions;
using SLThree.Metadata;
using System.Numerics;
namespace SLThree.Native
{
    public class ILRepresentation
    {
        public enum ILOpCode
        {
            Nop = 0,
            Break = 1,
            Ldarg_0 = 2,
            Ldarg_1 = 3,
            Ldarg_2 = 4,
            Ldarg_3 = 5,
            Ldloc_0 = 6,
            Ldloc_1 = 7,
            Ldloc_2 = 8,
            Ldloc_3 = 9,
            Stloc_0 = 10,
            Stloc_1 = 11,
            Stloc_2 = 12,
            Stloc_3 = 13,
            Ldarg_S = 14,
            Ldarga_S = 15,
            Starg_S = 16,
            Ldloc_S = 17,
            Ldloca_S = 18,
            Stloc_S = 19,
            Ldnull = 20,
            Ldc_I4_M1 = 21,
            Ldc_I4_0 = 22,
            Ldc_I4_1 = 23,
            Ldc_I4_2 = 24,
            Ldc_I4_3 = 25,
            Ldc_I4_4 = 26,
            Ldc_I4_5 = 27,
            Ldc_I4_6 = 28,
            Ldc_I4_7 = 29,
            Ldc_I4_8 = 30,
            Ldc_I4_S = 31,
            Ldc_I4 = 32,
            Ldc_I8 = 33,
            Ldc_R4 = 34,
            Ldc_R8 = 35,
            Dup = 37,
            Pop = 38,
            Jmp = 39,
            Call = 40,
            Calli = 41,
            Ret = 42,
            Br_S = 43,
            Brfalse_S = 44,
            Brtrue_S = 45,
            Beq_S = 46,
            Bge_S = 47,
            Bgt_S = 48,
            Ble_S = 49,
            Blt_S = 50,
            Bne_Un_S = 51,
            Bge_Un_S = 52,
            Bgt_Un_S = 53,
            Ble_Un_S = 54,
            Blt_Un_S = 55,
            Br = 56,
            Brfalse = 57,
            Brtrue = 58,
            Beq = 59,
            Bge = 60,
            Bgt = 61,
            Ble = 62,
            Blt = 63,
            Bne_Un = 64,
            Bge_Un = 65,
            Bgt_Un = 66,
            Ble_Un = 67,
            Blt_Un = 68,
            Switch = 69,
            Ldind_I1 = 70,
            Ldind_U1 = 71,
            Ldind_I2 = 72,
            Ldind_U2 = 73,
            Ldind_I4 = 74,
            Ldind_U4 = 75,
            Ldind_I8 = 76,
            Ldind_I = 77,
            Ldind_R4 = 78,
            Ldind_R8 = 79,
            Ldind_Ref = 80,
            Stind_Ref = 81,
            Stind_I1 = 82,
            Stind_I2 = 83,
            Stind_I4 = 84,
            Stind_I8 = 85,
            Stind_R4 = 86,
            Stind_R8 = 87,
            Add = 88,
            Sub = 89,
            Mul = 90,
            Div = 91,
            Div_Un = 92,
            Rem = 93,
            Rem_Un = 94,
            And = 95,
            Or = 96,
            Xor = 97,
            Shl = 98,
            Shr = 99,
            Shr_Un = 100,
            Neg = 101,
            Not = 102,
            Conv_I1 = 103,
            Conv_I2 = 104,
            Conv_I4 = 105,
            Conv_I8 = 106,
            Conv_R4 = 107,
            Conv_R8 = 108,
            Conv_U4 = 109,
            Conv_U8 = 110,
            Callvirt = 111,
            Cpobj = 112,
            Ldobj = 113,
            Ldstr = 114,
            Newobj = 115,
            Castclass = 116,
            Isinst = 117,
            Conv_R_Un = 118,
            Unbox = 121,
            Throw = 122,
            Ldfld = 123,
            Ldflda = 124,
            Stfld = 125,
            Ldsfld = 126,
            Ldsflda = 127,
            Stsfld = 128,
            Stobj = 129,
            Conv_Ovf_I1_Un = 130,
            Conv_Ovf_I2_Un = 131,
            Conv_Ovf_I4_Un = 132,
            Conv_Ovf_I8_Un = 133,
            Conv_Ovf_U1_Un = 134,
            Conv_Ovf_U2_Un = 135,
            Conv_Ovf_U4_Un = 136,
            Conv_Ovf_U8_Un = 137,
            Conv_Ovf_I_Un = 138,
            Conv_Ovf_U_Un = 139,
            Box = 140,
            Newarr = 141,
            Ldlen = 142,
            Ldelema = 143,
            Ldelem_I1 = 144,
            Ldelem_U1 = 145,
            Ldelem_I2 = 146,
            Ldelem_U2 = 147,
            Ldelem_I4 = 148,
            Ldelem_U4 = 149,
            Ldelem_I8 = 150,
            Ldelem_I = 151,
            Ldelem_R4 = 152,
            Ldelem_R8 = 153,
            Ldelem_Ref = 154,
            Stelem_I = 155,
            Stelem_I1 = 156,
            Stelem_I2 = 157,
            Stelem_I4 = 158,
            Stelem_I8 = 159,
            Stelem_R4 = 160,
            Stelem_R8 = 161,
            Stelem_Ref = 162,
            Ldelem = 163,
            Stelem = 164,
            Unbox_Any = 165,
            Conv_Ovf_I1 = 179,
            Conv_Ovf_U1 = 180,
            Conv_Ovf_I2 = 181,
            Conv_Ovf_U2 = 182,
            Conv_Ovf_I4 = 183,
            Conv_Ovf_U4 = 184,
            Conv_Ovf_I8 = 185,
            Conv_Ovf_U8 = 186,
            Refanyval = 194,
            Ckfinite = 195,
            Mkrefany = 198,
            Ldtoken = 208,
            Conv_U2 = 209,
            Conv_U1 = 210,
            Conv_I = 211,
            Conv_Ovf_I = 212,
            Conv_Ovf_U = 213,
            Add_Ovf = 214,
            Add_Ovf_Un = 215,
            Mul_Ovf = 216,
            Mul_Ovf_Un = 217,
            Sub_Ovf = 218,
            Sub_Ovf_Un = 219,
            Endfinally = 220,
            Leave = 221,
            Leave_S = 222,
            Stind_I = 223,
            Conv_U = 224,
            Prefix7 = 248,
            Prefix6 = 249,
            Prefix5 = 250,
            Prefix4 = 251,
            Prefix3 = 252,
            Prefix2 = 253,
            Prefix1 = 254,
            Prefixref = 255,
            Arglist = 65024,
            Ceq = 65025,
            Cgt = 65026,
            Cgt_Un = 65027,
            Clt = 65028,
            Clt_Un = 65029,
            Ldftn = 65030,
            Ldvirtftn = 65031,
            Ldarg = 65033,
            Ldarga = 65034,
            Starg = 65035,
            Ldloc = 65036,
            Ldloca = 65037,
            Stloc = 65038,
            Localloc = 65039,
            Endfilter = 65041,
            Unaligned_ = 65042,
            Volatile_ = 65043,
            Tail_ = 65044,
            Initobj = 65045,
            Constrained_ = 65046,
            Cpblk = 65047,
            Initblk = 65048,
            Rethrow = 65050,
            Sizeof = 65052,
            Refanytype = 65053,
            Readonly_ = 65054
        }

        public static readonly ConcurrentDictionary<ILOpCode, OpCode> opCodes = new ConcurrentDictionary<ILOpCode, OpCode>(
            (Enum.GetValues(typeof(ILOpCode)) as ILOpCode[])
                .Select(x => new KeyValuePair<ILOpCode, OpCode>(x, typeof(OpCodes)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(u => (OpCode)u.GetValue(null))
                .First(y => y.Value == (short)x)
            ))
        );

        public static readonly ConcurrentDictionary<OperandType, int> Sizes = new ConcurrentDictionary<OperandType, int>(new Dictionary<OperandType, int>()
        {
            { OperandType.InlineBrTarget, 4 },
            { OperandType.InlineField, 4 },
            { OperandType.InlineI, 4 },
            { OperandType.InlineI8, 8 },
            { OperandType.InlineMethod, 4 },
            { OperandType.InlineR, 8 },
            { OperandType.InlineSig, 4 },
            { OperandType.InlineString, 4 },
            { OperandType.InlineSwitch, 4 },
            { OperandType.InlineType, 4 },
            { OperandType.InlineVar, 2 },
            { OperandType.ShortInlineBrTarget, 1 },
            { OperandType.ShortInlineI, 1 },
            { OperandType.ShortInlineR, 4 },
            { OperandType.ShortInlineVar, 1 },
        });

        public static void WriteIL(byte[] bytes, DefaultRestorator.IRestoratorWriter writer)
        {
            var pos = 0;

            while (pos < bytes.Length)
            {
                var opcode = default(OpCode);

                writer.WriteExpressionKeyword("IL" + pos.ToString("X").PadLeft(4, '0') + ": ");

                if (bytes.Length - pos > 1 && opCodes.TryGetValue((ILOpCode)((bytes[pos] << 8) + bytes[pos + 1]), out var opc2size))
                {
                    opcode = opc2size;
                    pos += 2;
                }
                else if (opCodes.TryGetValue((ILOpCode)(bytes[pos]), out var opc1size))
                {
                    opcode = opc1size;
                    pos += 1;
                }

                writer.WritePlainText(opcode.Name);

                if (opcode.OperandType != OperandType.InlineNone && Sizes.TryGetValue(opcode.OperandType, out var size))
                {
                    switch (opcode.OperandType)
                    {
                        case OperandType.ShortInlineBrTarget:
                            {
                                writer.WriteExpressionKeyword($" IL_{(pos + (bytes[pos] + size)).ToString("X").PadLeft(4, '0')}");
                            }
                            break;
                        default:
                            {
                                writer.WriteTypeText($" {opcode.OperandType}");
                                writer.WritePlainText($": ");
                                writer.WriteDigitText($"0x{GetHex(new ArraySegment<byte>(bytes, pos, size))}");
                                writer.WritePlainText(" (");
                                writer.WriteDigitText($"{GetDecimal(new ArraySegment<byte>(bytes, pos, size))}");
                                writer.WritePlainText(")");
                            }
                            break;
                    }
                    pos += size;
                }

                writer.Writeln();
            }
        }
        public static string GetHex(IEnumerable<byte> bytes)
        {
            return bytes.Select(x => x.ToString("X").PadLeft(2, '0')).Reverse().JoinIntoString("");
        }
        public static string GetDecimal(IEnumerable<byte> bytes)
        {
            return new BigInteger(bytes.ToArray()).ToString();
        }
        public static T GetIL<T>(byte[] bytes) where T: DefaultRestorator.IRestoratorWriter, new()
        {
            var w = new T();
            WriteIL(bytes, w);
            return w;
        }
        /// <summary>
        /// Текстовое представление IL-кода
        /// </summary>
        public static string GetILText(byte[] bytes) => GetIL<DefaultRestorator.DefaultRestoratorWriter>(bytes).GetString();
        public static void GetILConsole(byte[] bytes) => GetIL<DefaultRestorator.ConsoleWriter>(bytes);
    }
}
