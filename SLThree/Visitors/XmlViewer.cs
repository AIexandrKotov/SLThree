using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SLThree.Visitors
{
    public class XmlViewer : AbstractVisitor
    {
        XDocument xdoc;
        Stack<XElement> XElements = new Stack<XElement>();
        long depth = 0, max_depth = long.MaxValue;

        public XmlViewer()
        {
            xdoc = new XDocument();
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

        public List<ExecutionContext> OutedContexts = new List<ExecutionContext>();

        public override void VisitAny(object o)
        {
            depth++;
            switch (o)
            {
                case TemplateMethod method:
                    {
                        var xmethod = new XElement("RuntimeMethod");
                        if (XElements.Count > 0)
                            XElements.Peek().Add(xmethod);
                        XElements.Push(xmethod);
                        Visit(method);
                        if (XElements.Count > 1) XElements.Pop();
                    }
                    break;
                case GenericMethod method:
                    {
                        var xmethod = new XElement("RuntimeMethod");
                        if (XElements.Count > 0)
                            XElements.Peek().Add(xmethod);
                        XElements.Push(xmethod);
                        Visit(method);
                        if (XElements.Count > 1) XElements.Pop();
                    }
                    break;
                case Method method:
                    {
                        var xmethod = new XElement("RuntimeMethod");
                        if (XElements.Count > 0)
                            XElements.Peek().Add(xmethod);
                        XElements.Push(xmethod);
                        if (depth <= max_depth) Visit(method);
                        if (XElements.Count > 1) XElements.Pop();
                    }
                    break;
                case BaseStatement x:
                    {
                        var xcode = new XElement("Code");
                        if (XElements.Count > 0)
                            XElements.Peek().Add(xcode);
                        XElements.Push(xcode);
                        if (depth <= max_depth) VisitStatement(x);
                        if (XElements.Count > 1) XElements.Pop();
                    }
                    break;
                case BaseExpression x:
                    {
                        var xexpression = new XElement("Expression");
                        if (XElements.Count > 0)
                            XElements.Peek().Add(xexpression);
                        XElements.Push(xexpression);
                        if (depth <= max_depth) VisitExpression(x);
                        if (XElements.Count > 1) XElements.Pop();
                    }
                    break;
                case ExecutionContext x:
                    {
                        var xcontext = new XElement("Context");
                        if (XElements.Count > 0)
                            XElements.Peek().Add(xcontext);
                        XElements.Push(xcontext);
                        if (OutedContexts.Contains(x))
                            xcontext.Value = "Already outed";
                        else if (depth <= max_depth) Visit(x);
                        OutedContexts.Add(x);
                        if (XElements.Count > 1) XElements.Pop();
                    }
                    break;
                case ContextWrap y:
                    {
                        var xcontext = new XElement("Context");
                        if (XElements.Count > 0)
                            XElements.Peek().Add(xcontext);
                        XElements.Push(xcontext);
                        if (OutedContexts.Contains(y.Context))
                            xcontext.Value = "Already outed";
                        else if (depth <= max_depth) Visit(y.Context);
                        OutedContexts.Add(y.Context);
                        if (XElements.Count > 1) XElements.Pop();
                    }
                    break;
                default:
                    {
                        var xo = new XElement("Object");
                        if (XElements.Count > 0)
                            XElements.Peek().Add(xo);
                        XElements.Push(xo);
                        xo.Add(new XAttribute("type", o.GetType().GetTypeString()));
                        xo.Add(new XAttribute("hash", Convert.ToString(o.GetHashCode(), 16)));
                        if (XElements.Count > 1) XElements.Pop();
                    }
                    break;
            }
            depth--;
        }

        public override void Visit(ExecutionContext context)
        {
            XElements.Peek().Add(new XAttribute("Name", context.Name));
            XElements.Peek().Add(new XAttribute("ForbidImplicit", context.ForbidImplicit));
            foreach (var x in context.LocalVariables.GetAsDictionary())
            {
                switch (x.Value)
                {
                    case ClassAccess access:
                        {
                            var xvariable = new XElement("Using");
                            xvariable.Add(new XAttribute("alias", x.Key));
                            xvariable.Add(new XAttribute("type", access.Name.GetTypeString()));
                            XElements.Peek().Add(xvariable);
                        }
                        break;
                    case Method method:
                        {
                            var xmethod = new XElement("Method");
                            xmethod.Add(new XAttribute("alias", x.Key));
                            XElements.Peek().Add(xmethod);
                            XElements.Push(xmethod);
                            if (depth < max_depth) VisitAny(x.Value);
                            XElements.Pop();
                        }
                        break;
                    default:
                        {
                            var xvariable = new XElement("Variable");
                            xvariable.Add(new XAttribute("id", x.Key));
                            XElements.Peek().Add(xvariable);
                            XElements.Push(xvariable);
                            if (depth < max_depth) VisitAny(x.Value);
                            XElements.Pop();
                        }
                        break;
                }
            }
        }

        public static string GetView(object o, long max_depth = 3)
        {
            var sv = new XmlViewer();
            sv.max_depth = max_depth;
            sv.VisitAny(o);
            sv.xdoc.Add(sv.XElements.Last());
            return sv.xdoc.ToString(SaveOptions.None);
        }
    }
}
