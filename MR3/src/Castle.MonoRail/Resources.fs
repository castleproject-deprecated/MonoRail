//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Resource

    open System.ComponentModel.Composition

    // abstractions for path providers/file system

    [<AbstractClass>]
    type ResourceProvider() = 
        abstract member Exists : name:string -> bool
        abstract member GetResource : name:string -> Resource    
    
    and Resource(name:string, opendel:unit -> System.IO.Stream) = 
            member x.Name = name
            member x.Open() = opendel()


    [<Export(typeof<ResourceProvider>)>]
    [<ExportMetadata("Order", 100000)>]
    type VirtualResourceProvider () =
        inherit ResourceProvider()
        let _vpProvider = System.Web.Hosting.HostingEnvironment.VirtualPathProvider

        override x.Exists name = 
            _vpProvider.FileExists name
        
        override x.GetResource name = 
            let file = _vpProvider.GetFile name
            VirtualResource(file) :> Resource

    
    and VirtualResource(file) = 
        inherit Resource(file.Name, fun f -> file.Open())


