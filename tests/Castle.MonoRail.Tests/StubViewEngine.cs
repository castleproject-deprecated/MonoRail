#region License
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
#endregion

namespace Castle.MonoRail.Tests
{
    using System;
    using System.Collections.Generic;
    using Castle.MonoRail.ViewEngines;

    public class StubViewEngine : IViewEngine
    {
        private readonly Func<IEnumerable<string>, IEnumerable<string>, ViewEngineResult> _resolve;
        private readonly Func<IEnumerable<string>, bool> _hasView;

        public StubViewEngine(
            Func<IEnumerable<string>, IEnumerable<string>, ViewEngineResult> resolve, 
            Func<IEnumerable<string>, bool> hasView)
        {
            _resolve = resolve;
            _hasView = hasView;
        }

        public ViewEngineResult ResolveView(IEnumerable<string> viewLocations, IEnumerable<string> layoutLocations)
        {
            return _resolve(viewLocations, layoutLocations);
        }

        public bool HasView(IEnumerable<string> viewLocations)
        {
            return _hasView(viewLocations);
        }
    }
}
