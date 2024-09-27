
using SLThree.Extensions.Cloning;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SLThree
{
    public class AtomLiteral : Literal<object>
    {
        public AtomLiteral(object value, string raw, ISourceContext cursor) : base(value, raw, cursor) { }
        public AtomLiteral(object value, ISourceContext cursor) : base(value, cursor) { }
        public AtomLiteral() : base() { }
        public override object Clone() => new AtomLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        }; 
        
        public static AtomLiteral Hash(string name, int size, ISourceContext context)
        {
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(name));
                object value;
                switch (size)
                {
                    case 1:
                        value = unchecked((sbyte)hash[0]);
                        break;
                    case 2:
                        value = BitConverter.ToInt16(hash, 0);
                        break;
                    case 3:
                        value = BitConverter.ToInt32(hash, 0) >> 8;
                        break;
                    case 4:
                        value = BitConverter.ToInt32(hash, 0);
                        break;
                    case 5:
                        value = BitConverter.ToInt64(hash, 0) >> 24;
                        break;
                    case 6:
                        value = BitConverter.ToInt64(hash, 0) >> 16;
                        break;
                    case 7:
                        value = BitConverter.ToInt64(hash, 0) >> 8;
                        break;
                    case 8:
                        value = BitConverter.ToInt64(hash, 0);
                        break;
                    default:
                        value = hash;
                        break;
                }
                return new AtomLiteral(value, name, context);
            }
        }
    }
}
