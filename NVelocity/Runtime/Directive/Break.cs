using System;
using System.Collections.Generic;
using System.Text;
using NVelocity.Context;
using System.IO;
using NVelocity.Runtime.Parser.Node;

namespace NVelocity.Runtime.Directive
{
    public class Break : Directive
    {
        /**
         *  simple init - init the tree and get the elementKey from
         *  the AST
         * @param rs
         * @param context
         * @param node
         * @throws TemplateInitException
         */
        public override void Init(IRuntimeServices rs, IInternalContextAdapter context, INode node)
        //throws TemplateInitException
        {
            base.Init(rs, context, node);
        }

        public override string Name
        {
            get
            {
                return "break";
            }
            set { throw new NotSupportedException(); }
        }

        /**
         * Return type of this directive.
         * @return The type of this directive.
         */
        public override DirectiveType Type
        {
            get
            {
                return DirectiveType.LINE;
            }
        }


        /**
         * Break directive does not actually do any rendering. 
         * 
         * This directive throws a BreakException (RuntimeException) which
         * signals foreach directive to break out of the loop. Note that this
         * directive does not verify that it is being called inside a foreach
         * loop.
         * 
         * @param context
         * @param writer
         * @param node
         * @return true if the directive rendered successfully.
         * @throws IOException
         * @throws MethodInvocationException
         * @throws ResourceNotFoundException
         * @throws ParseErrorException
         */
        public override bool Render(IInternalContextAdapter context, TextWriter writer, NVelocity.Runtime.Parser.Node.INode node)
        {
            throw new BreakException();
        }
    }

    public class BreakException : System.Exception
    {

    }
}
