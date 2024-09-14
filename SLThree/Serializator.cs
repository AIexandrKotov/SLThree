using SLThree.Metadata;
using System;

namespace SLThree
{
    public class Serializator : ISerializator
    {
        public static Serializator This = new Serializator();

        public byte[] Serialize(ExecutionContext.IExecutable statement)
        {
            throw new NotImplementedException();
        }

        public ExecutionContext.IExecutable Deserialize(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
