using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SLThree.Visitors
{
    public class XmlViewer : AbstractVisitor
    {
        XDocument xdoc;
        Stack<XElement> XElements = new Stack<XElement>();

        public XmlViewer()
        {
            xdoc = new XDocument();
            XElements.Push(new XElement("SLThree"));
        }

        public override void VisitExpression(NameExpression expression)
        {
            if (expression.TypeHint != null)
                XElements.Peek().Add(new XAttribute("type", expression.TypeHint.ToString()));
            XElements.Peek().Add(new XAttribute("name", expression.Name));
        }

        public override void VisitExpression(Literal expression)
        {
            XElements.Peek().Add(new XAttribute("raw", expression.RawRepresentation));
        }

        public override void VisitExpression(TypenameExpression expression)
        {
            XElement xd = new XElement("TypenameExpression");
            XElements.Push(xd);
            xd.Add(new XAttribute("name", expression.Typename.ToString()));
            if (expression.Generics != null)
            {
                foreach (var x in expression.Generics)
                    VisitExpression(x);
            }
            XElements.Pop();
            XElements.Peek().Add(xd);
        }

        public override void VisitExpression(BaseExpression expression)
        {
            var xnow = new XElement(expression.GetType().Name);
            XElements.Peek().Add(xnow);
            XElements.Push(xnow);
            base.VisitExpression(expression);
            XElements.Pop();
        }

        public override void VisitStatement(BaseStatement statement)
        {
            var xnow = new XElement(statement.GetType().Name);
            XElements.Peek().Add(xnow);
            XElements.Push(xnow);
            base.VisitStatement(statement);
            XElements.Pop();
        }

        public static string GetView(object o)
        {
            var sv = new XmlViewer();
            sv.VisitAny(o);
            sv.xdoc.Add(sv.XElements.First());
            return sv.xdoc.ToString();
        }
    }
}
