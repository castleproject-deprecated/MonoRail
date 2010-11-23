#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion

namespace Castle.MonoRail.Mvc.ViewEngines
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using MonoRail.WebForms;
    using WebForms;

    public class WebFormView : IView
    {
        public WebFormView(IWebFormFactory webFormFactory, string viewPath, string layoutPath)
        {
            Contract.Requires(webFormFactory != null);
            Contract.Requires(viewPath != null);
            Contract.Requires(layoutPath != null);
            Contract.EndContractBlock();

            this.WebFormFactory = webFormFactory;
            this.ViewPath = viewPath;
            this.MasterPath = layoutPath; // translation of concept names
        }

        public string MasterPath { get; private set; }
        public string ViewPath { get; private set; }
        public IWebFormFactory WebFormFactory { get; private set; }

        public virtual void Process(ViewContext viewContext, TextWriter writer)
        {
            Contract.Assert(writer != null);

            object viewInstance = WebFormFactory.CreateInstanceFromVirtualPath(ViewPath, typeof(object));
            if (viewInstance == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentUICulture,
                        "WebForm Page could not be created: {0}",
                        ViewPath));
            }

            ViewPage viewPage = viewInstance as ViewPage;
            if (viewPage != null)
            {
                RenderViewPage(viewContext, viewPage);
                return;
            }

//            ViewUserControl viewUserControl = viewInstance as ViewUserControl;
//            if (viewUserControl != null)
//            {
//                RenderViewUserControl(viewContext, viewUserControl);
//                return;
//            }

            throw new InvalidOperationException(
                String.Format(
                    CultureInfo.CurrentUICulture,
                    "Invalid view base class: {0}",
                    ViewPath));
        }

        private void RenderViewPage(ViewContext context, ViewPage page)
        {
            if (!String.IsNullOrEmpty(MasterPath))
            {
                page.MasterLocation = MasterPath;
            }

//            page.ViewData = context.ViewData;
            page.RenderView(context);
        }

//        private void RenderViewUserControl(ViewContext context, ViewUserControl control)
//        {
//            if (!String.IsNullOrEmpty(MasterPath))
//            {
//                throw new InvalidOperationException(MvcResources.WebFormViewEngine_UserControlCannotHaveMaster);
//            }
//
//            control.ViewData = context.ViewData;
//            control.RenderView(context);
//        }
    }
}
