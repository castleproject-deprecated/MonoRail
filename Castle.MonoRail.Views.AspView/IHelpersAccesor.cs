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

namespace Castle.MonoRail.Views.AspView
{
	using Components.DictionaryAdapter;
    using Framework.Helpers;

	public interface IHelpersAccesor
	{
		/// <summary>
		/// Gets the AjaxHelper instance
		/// </summary>
		[KeySubstitution("Ajax", "AjaxHelper")]
		AjaxHelper Ajax { get; }

		/// <summary>
		/// Gets the BehaviourHelper instance
		/// </summary>
		[KeySubstitution("Behaviour", "BehaviourHelper")]
		BehaviourHelper Behaviour { get; }

		/// <summary>
		/// Gets the DateFormatHelper instance
		/// </summary>
		[KeySubstitution("DateFormat", "DateFormatHelper")]
		DateFormatHelper DateFormat { get; }

		/// <summary>
		/// Gets the DictHelper instance
		/// </summary>
		[KeySubstitution("Dict", "DictHelper")]
		DictHelper Dict { get; }

		/// <summary>
		/// Gets the EffectsFatHelper instance
		/// </summary>
		[KeySubstitution("EffectsFat", "EffectsFatHelper")]
		EffectsFatHelper EffectsFat { get; }

		/// <summary>
		/// Gets the FormHelper instance
		/// </summary>
		[KeySubstitution("Form", "FormHelper")]
		FormHelper Form { get; }

		/// <summary>
		/// Gets the PaginationHelper instance
		/// </summary>
		[KeySubstitution("Pagination", "PaginationHelper")]
		PaginationHelper Pagination { get; }

		/// <summary>
		/// Gets the ScriptaculousHelper instance
		/// </summary>
		[KeySubstitution("Scriptaculous", "ScriptaculousHelper")]
		ScriptaculousHelper Scriptaculous { get; }

		/// <summary>
		/// Gets the UrlHelper instance
		/// </summary>
		[KeySubstitution("Url", "UrlHelper")]
		UrlHelper Url { get; }

		/// <summary>
		/// Gets the WizardHelper instance
		/// </summary>
		[KeySubstitution("Wizard", "WizardHelper")]
		WizardHelper Wizard { get; }

		/// <summary>
		/// Gets the ZebdaHelper instance
		/// </summary>
		[KeySubstitution("Zebda", "ZebdaHelper")]
		ZebdaHelper Zebda { get; }
	}
}