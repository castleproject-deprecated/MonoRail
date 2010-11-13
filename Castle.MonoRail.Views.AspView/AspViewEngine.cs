// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Castle.MonoRail.Framework.Test;

namespace Castle.MonoRail.Views.AspView
{
	using System;
	using System.Collections;
	using System.IO;
	using System.Configuration;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Text.RegularExpressions;
	using System.Collections.Generic;
	using System.Threading;
	using System.Web;
	using Configuration;
	using Framework.Configuration;
	using Compiler;
	using Compiler.Factories;
	using Framework;
	using Core;

	public class AspViewEngine : ViewEngineBase, IAspViewEngineTestAccess
	{
		static readonly ReaderWriterLock OptionsLocker = new ReaderWriterLock();

		private List<ICompilationContext> compilationContexts = new List<ICompilationContext>();

		private IMonoRailConfiguration monoRailConfiguration;
		static bool needsRecompiling;
		static AspViewEngineOptions options;
		static readonly Regex invalidClassNameCharacters = new Regex("[^a-zA-Z0-9\\-*#=/\\\\_.]", RegexOptions.Compiled);

		readonly Hashtable compilations = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));

		#region IAspViewEngineTestAccess
		Hashtable IAspViewEngineTestAccess.Compilations
		{
			get { return compilations; }
		}
		void IAspViewEngineTestAccess.SetViewSourceLoader(IViewSourceLoader viewSourceLoader)
		{
			ViewSourceLoader = viewSourceLoader;
		}

		void IAspViewEngineTestAccess.SetOptions(AspViewEngineOptions newOptions)
		{
			options = newOptions;
		}

		void IAspViewEngineTestAccess.SetCompilationContext(List<ICompilationContext> contexts)
		{
			compilationContexts = contexts;
		}

		#endregion

		public void Initialize()
		{
			InitializeOptionsIfNeeded();

			if (compilationContexts.Count == 0)
			{
				var siteRoot = AppDomain.CurrentDomain.BaseDirectory;

				compilationContexts.Add(
					new WebCompilationContext(
						monoRailConfiguration.ViewEngineConfig.ViewPathRoot,
						new DirectoryInfo(siteRoot),
						new DirectoryInfo(options.CompilerOptions.TemporarySourceFilesDirectory)));

				foreach (var path in monoRailConfiguration.ViewEngineConfig.PathSources)
				{
					compilationContexts.Add(
						new WebCompilationContext(
							path,
							new DirectoryInfo(siteRoot),
							new DirectoryInfo(options.CompilerOptions.TemporarySourceFilesDirectory)));
				}
			}

			LoadCompiledViews();

			if (options.CompilerOptions.AutoRecompilation)
			{
				// invalidate compiled views cache on any change to the view sources
				ViewSourceLoader.ViewChanged += delegate(object sender, FileSystemEventArgs e)
												{
													foreach (var extension in AbstractCompiler.TemplateExtensions)
													{
														if (e.Name.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
														{
															needsRecompiling = true;
														}
													}
												};
			}
		}

		#region ViewEngineBase implementation

		public override void Service(IServiceProvider provider)
		{
			base.Service(provider);

			monoRailConfiguration = (IMonoRailConfiguration)provider.GetService(typeof(IMonoRailConfiguration));

			Initialize();
		}
		public override bool HasTemplate(string templateName)
		{
			var className = GetClassName(templateName);
			return compilations.ContainsKey(className);
		}

		public override void Process(string templateName, string layoutName, TextWriter output, IDictionary<string, object> parameters)
		{
			var controllerContext = new ControllerContext();
			if (layoutName != null)
			{
				controllerContext.LayoutNames = new[] { layoutName };
			}
			foreach (var pair in parameters)
			{
				controllerContext.PropertyBag[pair.Key] = pair.Value;
			}

			var stubContext = new StubEngineContext();

			Process(templateName, output, stubContext, null, controllerContext);
		}

		public override void Process(string templateName, TextWriter output, IEngineContext context, IController controller, IControllerContext controllerContext)
		{
			// Get the main, entry-point, view instance
			IViewBaseInternal view = GetView(templateName, output, context, controller, controllerContext);

			// create layout views if needed. The entry point is changed to the outer-most layout.
			// each layout on the way holds its containing layout on the ContentView property,
			// down to the original entry-point.
			if (controllerContext.LayoutNames != null)
			{
				var layoutNames = controllerContext.LayoutNames;
				for (var i = layoutNames.Length - 1; i >= 0; --i)
				{
					var layoutName = layoutNames[i].Trim();
					IViewBaseInternal layout = GetLayout(layoutName, output, context, controller, controllerContext);
					layout.ContentView = view;
					view = layout;
				}
			}
			if (controller != null)
			{
				controller.PreSendView(view);
			}

			// initialize the entry point
			view.Initialize(this,output, context, controller, controllerContext, null);

			// initialize inner layouts down to the original entry-point
			var parent = view;
			var backtraceLayoutsToMainView = view.ContentView;
			while (backtraceLayoutsToMainView!=null)
			{
				backtraceLayoutsToMainView.Initialize(this, output, context, controller, controllerContext, parent.Properties);
				parent = backtraceLayoutsToMainView;
				backtraceLayoutsToMainView = parent.ContentView;
			}
			InitializeViewsStack(context);
			// process the view
			view.Process();
			if (controller != null)
			{
				controller.PostSendView(view);
			}
		}

		const string ViewsStackKey = "__ASPVIEW_VIEWS_STACK";
		public static void InitializeViewsStack(IEngineContext context)
		{
			context.Items[ViewsStackKey] = new Stack<AspViewBase>();
		}
		public static Stack<AspViewBase> GetViewsStack(IEngineContext context)
		{
			return context.Items[ViewsStackKey] as Stack<AspViewBase>;
		}
        
		public override void ProcessPartial(string partialName, TextWriter output, IEngineContext context, IController controller, IControllerContext controllerContext)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override string ViewFileExtension
		{
			get { return "aspx"; }
		}

		#region NJS
		public override string JSGeneratorFileExtension
		{
			get { throw new AspViewException("This version of AspView does not implements NJS."); }
		}

		public override bool SupportsJSGeneration
		{
			get { return false; }
		}

		#endregion
		#endregion

		public AspViewEngineOptions Options { get { return options; } }

		static AspViewBase CreateView(Type type)
		{
			return (AspViewBase)FormatterServices.GetUninitializedObject(type);
		}

		public virtual AspViewBase GetView(string templateName, TextWriter output, IEngineContext context, IController controller, IControllerContext controllerContext)
		{
			templateName = NormalizeFileName(templateName);
			var className = GetClassName(templateName);
			if (needsRecompiling)
			{
				CompileViewsInMemory();
				needsRecompiling = false;
			}

			var viewType = compilations[className] as Type;

			if (viewType == null)
				throw new AspViewException("Cannot find view type for {0}.",
					templateName);
			// create a view instance
			AspViewBase theView;
			try
			{
				theView = CreateView(viewType);
			}
			catch (Exception ex)
			{
				throw new AspViewException(ex, "Cannot create view instance from '{0}'.", templateName);
			}
			if (theView == null)
				throw new AspViewException(string.Format(
					"Cannot find view '{0}'", templateName));
			return theView;
		}

		protected virtual AspViewBase GetLayout(string layoutName, TextWriter output, IEngineContext context, IController controller, IControllerContext controllerContext)
		{
			var layoutTemplate = "layouts\\" + layoutName;
			if (layoutName.StartsWith("\\"))
				layoutTemplate = layoutName;
			return GetView(layoutTemplate, output, context, controller, controllerContext);
		}

		protected virtual void CompileViewsInMemory()
		{
			compilations.Clear();

			foreach (var compilationContext in compilationContexts)
			{
				var compiler = new OnlineCompiler(
					new CSharpCodeProviderAdapterFactory(),
					new PreProcessor(),
					compilationContext,
					options.CompilerOptions);

				LoadCompiledViewsFrom(compiler.Execute());
			}
		}

		private void CacheViewType(Type viewType)
		{
			compilations[viewType.Name] = viewType;
		}

		private void LoadCompiledViews()
		{
			if (options.CompilerOptions.AutoRecompilation)
			{
				CompileViewsInMemory();
				return;
			}
			compilations.Clear();

			var viewAssemblies = new List<string>();

			foreach (var compilationContext in compilationContexts)
			{
				viewAssemblies.AddRange(
					Directory.GetFiles(Path.Combine(compilationContext.SiteRoot.FullName, "bin"), "*CompiledViews.dll",
									   SearchOption.TopDirectoryOnly)
					);
			}

			foreach (var assembly in viewAssemblies)
			{
				Assembly precompiledViews;

				try
				{
					precompiledViews = Assembly.LoadFile(Path.GetFullPath(assembly));
				}
				catch (Exception e)
				{
					var error = string.Format("Could not load views assembly [{0}]", assembly);
					Logger.ErrorFormat(error);
					throw new InvalidOperationException(error, e);
				}

				if (precompiledViews != null)
					LoadCompiledViewsFrom(precompiledViews);
			}

		}

		private void LoadCompiledViewsFrom(Assembly viewsAssembly)
		{
			if (viewsAssembly == null)
				return;
			try
			{
				foreach (var type in viewsAssembly.GetTypes())
					CacheViewType(type);
			}
			catch (ReflectionTypeLoadException rtle)
			{
				var loaderErrors = "";
				foreach (var loaderException in rtle.LoaderExceptions)
				{
					loaderErrors += loaderException + Environment.NewLine;
				}

				Logger.Error(loaderErrors);
				throw new InvalidOperationException("Could not load type from views assembly because: " + Environment.NewLine + loaderErrors, rtle);
			}
		}

		public static string GetClassName(string fileName)
		{
			fileName = fileName.ToLowerInvariant();
			if (Path.HasExtension(fileName))
			{
				var lastDotIndex = fileName.LastIndexOf('.');
				fileName = fileName.Substring(0, lastDotIndex);
			}

			fileName = invalidClassNameCharacters.Replace(fileName, "_");

			var className = fileName
				.Replace('\\', '_')
				.Replace('/', '_')
				.Replace("-", "_dash_")
				.Replace("=", "_equals_")
				.Replace("*", "_star_")
				.Replace("#", "_sharp_")
				.Replace("=", "_equals_")
				.TrimStart('_')
				.Replace('.', '_');

			return className;
		}

		public static string NormalizeFileName(string fileName)
		{
			return fileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
		}

		private static void InitializeOptionsIfNeeded()
		{
			OptionsLocker.AcquireReaderLock(Timeout.Infinite);
			if (options != null)
			{
				OptionsLocker.ReleaseReaderLock();
				return;
			}

			OptionsLocker.UpgradeToWriterLock(Timeout.Infinite);
			if (options != null)
			{
				OptionsLocker.ReleaseWriterLock();
				return;
			}

			try
			{
				var optionsBuilder = new AspViewOptionsBuilder();
				InitializeProgrammaticConfig(optionsBuilder);

				var appSettingsOptions = GetAppSettingsOptions();
				if (appSettingsOptions != null)
					optionsBuilder.ApplyConfigurableOverrides(appSettingsOptions);

				options = optionsBuilder.BuildOptions();
			}
			finally
			{
				OptionsLocker.ReleaseWriterLock();
			}
		}

		private static void InitializeProgrammaticConfig(AspViewOptionsBuilder optionsBuilder)
		{
			var app = GetApplicationInstance();
			if (app == null)
				return;

			var configEvents = app as IAspViewConfigurationEvents;
			if (configEvents == null)
				return;

			configEvents.Configure(optionsBuilder);
		}

		private static AspViewConfigurationSection.Model GetAppSettingsOptions()
		{
			var sectionNames = new[] { "aspView", "aspview" };
			foreach (var sectionName in sectionNames)
			{
				var xmlOptions = ConfigurationManager.GetSection(sectionName) as AspViewConfigurationSection.Model;
				if (xmlOptions != null)
					return xmlOptions;
			}
			return null;
		}

		///<summary>
		/// Implementors should return a generator instance if
		/// the view engine supports JS generation.
		///</summary>
		///
		///<param name="generatorInfo">The generator info.</param>
		///<param name="context">The request context.</param>
		///<param name="controller">The controller.</param>
		///<param name="controllerContext">The controller context.</param>
		///<returns>
		///A JS generator instance
		///</returns>
		public override object CreateJSGenerator(JSCodeGeneratorInfo generatorInfo, IEngineContext context,
												 IController controller, IControllerContext controllerContext)
		{
			throw new NotImplementedException();
		}

		///<summary>
		/// Processes the js generation view template - using the templateName
		/// to obtain the correct template, and using the specified <see cref="T:System.IO.TextWriter" />
		/// to output the result.
		///</summary>
		///
		///<param name="templateName">Name of the template.</param>
		///<param name="output">The output.</param>
		///<param name="generatorInfo">The generator info.</param>
		///<param name="context">The request context.</param>
		///<param name="controller">The controller.</param>
		///<param name="controllerContext">The controller context.</param>
		public override void GenerateJS(string templateName, TextWriter output, JSCodeGeneratorInfo generatorInfo,
										IEngineContext context, IController controller, IControllerContext controllerContext)
		{
			throw new NotImplementedException();
		}

		///<summary>
		/// Wraps the specified content in the layout using the 
		/// context to output the result.
		///</summary>
		///
		public override void RenderStaticWithinLayout(string contents, IEngineContext context, IController controller,
													  IControllerContext controllerContext)
		{
			throw new NotImplementedException();
		}

		public static Func<HttpApplication> GetApplicationInstance =
			(() => HttpContext.Current == null ? null : HttpContext.Current.ApplicationInstance);
	}
}
