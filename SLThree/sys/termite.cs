using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class termite
    {
        /// <summary>
        /// Создаёт StaticExpression, который всегда будет возвращать объект obj
        /// </summary>
        public static StaticExpression @static(object obj) => new StaticExpression(obj);
    }
#pragma warning restore IDE1006 // Стили именования
}
