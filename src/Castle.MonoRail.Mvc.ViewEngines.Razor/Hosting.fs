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

namespace Castle.MonoRail.Mvc.ViewEngines.Razor

    open System.CodeDom
    open System.Reflection
    open System.Web.WebPages.Razor
    open System.Web.Razor
    open System.Web.Razor.Generator
    open System.Web.Razor.Parser
    open System.Web.Razor.Parser.SyntaxTree
    open System.Web.Razor.Text


    type MonoRailRazorHostFactory() = 
        inherit WebRazorHostFactory() 

        override x.CreateHost (vpath, ppath) = 
            let host = base.CreateHost(vpath, ppath)

            if not host.IsSpecialPage then
                upcast MonoRailRazorHost(vpath, ppath)
            else
                host


    and MonoRailRazorHost (vpath, ppath) as self =
        inherit WebPageRazorHost(vpath, ppath)

        let remove_ns names =
            for n in names do
                self.NamespaceImports.Remove n |> ignore

        do
            base.DefaultPageBaseClass <- typeof<Castle.MonoRail.Razor.WebViewPage>.FullName
            remove_ns [|"WebMatrix.Data";"System.Web.WebPages.Html";"WebMatrix.WebData"|]

        override this.DecorateCodeGenerator (codeGen) = 
            match codeGen with 
            | :? CSharpRazorCodeGenerator -> upcast CSharpRazorCodeWrappedGen(codeGen, this)
            // | :? VBRazorCodeGenerator -> upcast VBRazorCodeWrappedGen(codeGen, this)
            | _ -> base.DecorateCodeGenerator(codeGen)

        override this.DecorateCodeParser(parser) = 
            match parser with 
            | :? CSharpCodeParser -> upcast CSharpCodeWrappedParser()
            // | :? VBCodeParser -> upcast VBCodeWrappedParser()
            | _ -> base.DecorateCodeParser(parser)


    and ModelDirective(start, content, modelname) = 
        inherit CodeSpan(start, content)

        member x.ModelName = modelname

        new (context:ParserContext, modelname) = 
            ModelDirective(context.CurrentSpanStart, context.ContentBuffer.ToString(), modelname)


    and CSharpCodeWrappedParser() as self = 
        inherit CSharpCodeParser()
        let mutable _inheritLocation : SourceLocation option = None
        let mutable _modelStatementFound = false

        do
            let del = self.WrapSimpleBlockParser(BlockType.Directive, self.CreateDelegate())
            self.RazorKeywords.Add("model", del)


        member x.CreateDelegate() = 
            let nested_del = typeof<CodeParser>.GetNestedType("BlockParser", BindingFlags.Public ||| BindingFlags.NonPublic)
            let target = typeof<CSharpCodeWrappedParser>.GetMethod("ParseModelStatement")
            downcast System.MulticastDelegate.CreateDelegate(nested_del, x, target)

        override x.ParseInheritsStatement block = 
            _inheritLocation <- Some(x.CurrentLocation)
            let res = base.ParseInheritsStatement(block)
            x.CheckForInheritsAndModelStatements()
            res

        member x.CheckForInheritsAndModelStatements() = 
            if _modelStatementFound && _inheritLocation.IsSome then
                base.OnError (_inheritLocation.Value, "You can't specify the model directive _and_ the inherits directive")

        member x.ParseModelStatement (block:CodeBlockInfo) = 
            let endLocation = x.CurrentLocation
            let readWhitespace = x.RequireSingleWhiteSpace();
            let ctx = x.Context

            x.End(MetaCodeSpan.Create(ctx, false, if readWhitespace then AcceptedCharacters.None else AcceptedCharacters.Any))
            
            if _modelStatementFound then
                base.OnError (endLocation, "No more than a single @model directive is allowed")

            _modelStatementFound <- true 
            x.Context.AcceptWhiteSpace(true) |> ignore

            let mutable typename : string = null
            if ParserHelpers.IsIdentifierStart (x.CurrentCharacter) then
                let disposable = ctx.StartTemporaryBuffer()
                ctx.AcceptUntil [|'\r';'\n'|] |> ignore
                typename <- ctx.ContentBuffer.ToString()
                ctx.AcceptTemporaryBuffer()
                disposable.Dispose()

                ctx.AcceptNewLine()
            else 
                base.OnError (endLocation, "The @model directive must be followed by the typename of your model type")

            x.CheckForInheritsAndModelStatements()
            base.End( ModelDirective(ctx, typename) ) 
            false


    (* 
    and VBCodeWrappedParser() = 
        inherit VBCodeParser()
    *)

    and CSharpRazorCodeWrappedGen(codegen, host) as self =
        inherit CSharpRazorCodeGenerator(codegen.ClassName, codegen.RootNamespaceName, codegen.SourceFileName, host)

        do 
            if not host.IsSpecialPage then
                self.SetGenericArgumentForBase("dynamic");

        member x.SetGenericArgumentForBase(name) = 
            let baseType = CodeTypeReference(host.DefaultBaseClass + "<" + name + ">");
            x.GeneratedClass.BaseTypes.Clear()
            x.GeneratedClass.BaseTypes.Add baseType |> ignore

        override x.TryVisitSpecialSpan span = 
            RazorCodeGenerator.TryVisit<ModelDirective>(span, (fun arg -> x.VisitModelDirective arg span ))

        member x.VisitModelDirective (arg:ModelDirective) (span:Span) = 
            x.SetGenericArgumentForBase(arg.ModelName)

            if x.DesignTimeMode then
                x.WriteHelperVariable (span.Content, "_modelhelper")

    (*
    and VBRazorCodeWrappedGen(codegen, host) =
        inherit VBRazorCodeGenerator(codegen.ClassName, codegen.RootNamespaceName, codegen.SourceFileName, host)
    *)

