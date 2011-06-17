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