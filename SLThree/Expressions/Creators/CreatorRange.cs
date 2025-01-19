using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class CreatorRange : BaseExpression
    {
        public override int Priority => 0;
        public BaseExpression LowerBound;
        public BaseExpression UpperBound;
        public TypenameExpression RangeType;
        public bool Excluding;

        public CreatorRange(BaseExpression lowerBound, BaseExpression upperBound, bool excluding, ISourceContext context) : this(lowerBound, upperBound, null, excluding, context) { }
        public CreatorRange(BaseExpression lowerBound, BaseExpression upperBound, TypenameExpression type, bool excluding, ISourceContext context) : base(context)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
            RangeType = type;
            Excluding = excluding;
        }

        #region Ranges
        public struct RangeEnumerator : IEnumerable<object>
        {
            public long Lower;
            public long Upper;

            public RangeEnumerator(long lower, long upper, bool excluding = false)
            {
                Lower = lower;
                Upper = upper - (excluding ? 1 : 0);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<object> GetEnumerator()
            {
                for (var i = Lower; i <= Upper; i++)
                    yield return i;
            }

            public override string ToString() => $"{Lower}..={Upper}";
        }
        public struct LongRangeEnumerator : IEnumerable<long>
        {
            public long Lower;
            public long Upper;

            public LongRangeEnumerator(long lower, long upper, bool excluding = false)
            {
                Lower = lower;
                Upper = upper - (excluding ? 1 : 0);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<long> GetEnumerator()
            {
                for (var i = Lower; i <= Upper; i++)
                    yield return i;
            }

            public override string ToString() => $"<i64>{Lower}..={Upper}";
        }
        public struct ULongRangeEnumerator : IEnumerable<ulong>
        {
            public ulong Lower;
            public ulong Upper;
            
            public ULongRangeEnumerator(ulong lower, ulong upper, bool excluding = false)
            {
                Lower = lower;
                Upper = upper - (excluding ? 1u : 0);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<ulong> GetEnumerator()
            {
                for (var i = Lower; i <= Upper; i++)
                    yield return i;
            }

            public override string ToString() => $"<u64>{Lower}..={Upper}";
        }
        public struct IntRangeEnumerator : IEnumerable<int>
        {
            public int Lower;
            public int Upper;

            public IntRangeEnumerator(int lower, int upper, bool excluding = false)
            {
                Lower = lower;
                Upper = upper - (excluding ? 1 : 0);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<int> GetEnumerator()
            {
                for (var i = Lower; i <= Upper; i++)
                    yield return i;
            }

            public override string ToString() => $"<i32>{Lower}..={Upper}";
        }
        public struct UIntRangeEnumerator : IEnumerable<uint>
        {
            public uint Lower;
            public uint Upper;

            public UIntRangeEnumerator(uint lower, uint upper, bool excluding = false)
            {
                Lower = lower;
                Upper = upper - (excluding ? 1u : 0);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<uint> GetEnumerator()
            {
                for (var i = Lower; i <= Upper; i++)
                    yield return i;
            }

            public override string ToString() => $"<u32>{Lower}..={Upper}";
        }
        public struct ShortRangeEnumerator : IEnumerable<short>
        {
            public short Lower;
            public short Upper;

            public ShortRangeEnumerator(short lower, short upper, bool excluding = false)
            {
                Lower = lower;
                Upper = (short)(upper - (excluding ? 1 : 0));
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<short> GetEnumerator()
            {
                for (var i = Lower; i <= Upper; i++)
                    yield return i;
            }

            public override string ToString() => $"<i16>{Lower}..={Upper}";
        }
        public struct UShortRangeEnumerator : IEnumerable<ushort>
        {
            public ushort Lower;
            public ushort Upper;

            public UShortRangeEnumerator(ushort lower, ushort upper, bool excluding = false)
            {
                Lower = lower;
                Upper = (ushort)(upper - (excluding ? 1u : 0u));
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<ushort> GetEnumerator()
            {
                for (var i = Lower; i <= Upper; i++)
                    yield return i;
            }

            public override string ToString() => $"<u16>{Lower}..={Upper}";
        }
        public struct SByteRangeEnumerator : IEnumerable<sbyte>
        {
            public sbyte Lower;
            public sbyte Upper;

            public SByteRangeEnumerator(sbyte lower, sbyte upper, bool excluding = false)
            {
                Lower = lower;
                Upper = (sbyte)(upper - (excluding ? 1 : 0));
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<sbyte> GetEnumerator()
            {
                for (var i = Lower; i <= Upper; i++)
                    yield return i;
            }

            public override string ToString() => $"<i8>{Lower}..={Upper}";
        }
        public struct ByteRangeEnumerator : IEnumerable<byte>
        {
            public byte Lower;
            public byte Upper;

            public ByteRangeEnumerator(byte lower, byte upper, bool excluding = false)
            {
                Lower = lower;
                Upper = (byte)(upper - (excluding ? 1 : 0));
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<byte> GetEnumerator()
            {
                for (var i = Lower; i <= Upper; i++)
                    yield return i;
            }

            public override string ToString() => $"<u8>{Lower}..={Upper}";
        }
        #endregion

        public override object GetValue(ExecutionContext context)
        {
            if (context.ForbidImplicit)
            {
                if (RangeType == null)
                {
                    var lower = LowerBound.GetValue(context).Cast<long>();
                    var upper = UpperBound.GetValue(context).Cast<long>();
                    return new RangeEnumerator(lower, upper, Excluding);
                }
                else
                {
                    var type = RangeType.GetValue(context).Cast<Type>();
                    if (type == typeof(long))
                        return new LongRangeEnumerator(
                            LowerBound.GetValue(context).Cast<long>(),
                            UpperBound.GetValue(context).Cast<long>(), Excluding);
                    if (type == typeof(ulong))
                        return new ULongRangeEnumerator(
                            LowerBound.GetValue(context).Cast<ulong>(),
                            UpperBound.GetValue(context).Cast<ulong>(), Excluding);
                    if (type == typeof(int))
                        return new IntRangeEnumerator(
                            LowerBound.GetValue(context).Cast<int>(),
                            UpperBound.GetValue(context).Cast<int>(), Excluding);
                    if (type == typeof(uint))
                        return new UIntRangeEnumerator(
                            LowerBound.GetValue(context).Cast<uint>(),
                            UpperBound.GetValue(context).Cast<uint>(), Excluding);
                    if (type == typeof(short))
                        return new ShortRangeEnumerator(
                            LowerBound.GetValue(context).Cast<short>(),
                            UpperBound.GetValue(context).Cast<short>(), Excluding);
                    if (type == typeof(ushort))
                        return new UShortRangeEnumerator(
                            LowerBound.GetValue(context).Cast<ushort>(),
                            UpperBound.GetValue(context).Cast<ushort>(), Excluding);
                    if (type == typeof(sbyte))
                        return new SByteRangeEnumerator(
                            LowerBound.GetValue(context).Cast<sbyte>(),
                            UpperBound.GetValue(context).Cast<sbyte>(), Excluding);
                    if (type == typeof(byte))
                        return new ByteRangeEnumerator(
                            LowerBound.GetValue(context).Cast<byte>(),
                            UpperBound.GetValue(context).Cast<byte>(), Excluding);
                    throw new RangeIncorrectType(type, SourceContext);
                }
            }
            else
            {
                if (RangeType == null)
                {
                    var lower = LowerBound.GetValue(context).CastToType<long>();
                    var upper = UpperBound.GetValue(context).CastToType<long>();
                    return new RangeEnumerator(lower, upper, Excluding);
                }
                else
                {
                    var type = RangeType.GetValue(context).Cast<Type>();
                    if (type == typeof(long))
                        return new LongRangeEnumerator(
                            LowerBound.GetValue(context).CastToType<long>(),
                            UpperBound.GetValue(context).CastToType<long>(), Excluding);
                    if (type == typeof(ulong))
                        return new ULongRangeEnumerator(
                            LowerBound.GetValue(context).CastToType<ulong>(),
                            UpperBound.GetValue(context).CastToType<ulong>(), Excluding);
                    if (type == typeof(int))
                        return new IntRangeEnumerator(
                            LowerBound.GetValue(context).CastToType<int>(),
                            UpperBound.GetValue(context).CastToType<int>(), Excluding);
                    if (type == typeof(uint))
                        return new UIntRangeEnumerator(
                            LowerBound.GetValue(context).CastToType<uint>(),
                            UpperBound.GetValue(context).CastToType<uint>(), Excluding);
                    if (type == typeof(short))
                        return new ShortRangeEnumerator(
                            LowerBound.GetValue(context).CastToType<short>(),
                            UpperBound.GetValue(context).CastToType<short>(), Excluding);
                    if (type == typeof(ushort))
                        return new UShortRangeEnumerator(
                            LowerBound.GetValue(context).CastToType<ushort>(),
                            UpperBound.GetValue(context).CastToType<ushort>(), Excluding);
                    if (type == typeof(sbyte))
                        return new SByteRangeEnumerator(
                            LowerBound.GetValue(context).CastToType<sbyte>(),
                            UpperBound.GetValue(context).CastToType<sbyte>(), Excluding);
                    if (type == typeof(byte))
                        return new ByteRangeEnumerator(
                            LowerBound.GetValue(context).CastToType<byte>(),
                            UpperBound.GetValue(context).CastToType<byte>(), Excluding);
                    throw new RangeIncorrectType(type, SourceContext);
                }
            }
        }

        public override string ExpressionToString() => $"{(RangeType == null ? "" : $"<{RangeType}>")}{LowerBound}..{(Excluding?"":"=")}{UpperBound}";

        public override object Clone()
        {
            return new CreatorRange(LowerBound.CloneCast(), UpperBound.CloneCast(), RangeType.CloneCast(), Excluding, SourceContext.CloneCast());
        }
    }
}
