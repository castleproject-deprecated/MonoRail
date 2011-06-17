//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.Blade

    open System
    open System.Collections.Generic
    
    [<AbstractClass>]
    type BaseBladePage() = 

        let _sections = Dictionary<string, Action>(StringComparer.InvariantCultureIgnoreCase)

        (* 
        public virtual HttpContextBase Context { get; set; }
        public virtual string VirtualPath { get; set; }
        *)

        abstract member Initialize : unit -> unit 
        abstract member RenderPage : unit -> unit 

        default x.Initialize() = 
            ()

        member x.DefineSection(name:string, action:Action) =
            _sections.[name] <- action

        member x.RenderSection(name:string, required:bool) = 
            ()

        member x.RenderSection(name:string) = 
            x.RenderSection (name, true)

        (* 
        public static void WriteTo(TextWriter writer, HelperResult content) {
            if (content != null) {
                content.WriteTo(writer);
            }
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteTo(TextWriter writer, object content) {
            writer.Write(HttpUtility.HtmlEncode(content));
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteLiteralTo(TextWriter writer, object content) {
            writer.Write(content);
        }
        *)