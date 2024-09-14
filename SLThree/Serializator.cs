using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class Serializator : LanguageInformation.ISerializator
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
