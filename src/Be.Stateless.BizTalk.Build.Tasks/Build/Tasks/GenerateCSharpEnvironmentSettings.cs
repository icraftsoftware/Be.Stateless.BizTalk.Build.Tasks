#region Copyright & License

// Copyright © 2012 - 2021 François Chabot
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using Be.Stateless.BizTalk.CSharp.Extensions;
using Be.Stateless.BizTalk.Dsl.Environment.Settings.CodeDom;
using Be.Stateless.Extensions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CSharp;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "MSBuild Task.")]
	public class GenerateCSharpEnvironmentSettings : Task
	{
		#region Base Class Member Overrides

		public override bool Execute()
		{
			try
			{
				var outputs = new List<ITaskItem>();
				foreach (var xlSettingsItem in ExcelEnvironmentSettings)
				{
					Log.LogMessage(MessageImportance.High, "Generating C# environment settings class '{0}'.", xlSettingsItem.GetMetadata("Identity"));
					var csharpSettingsItem = new TaskItem(ComputeOutputPath(xlSettingsItem));
					csharpSettingsItem.SetMetadata("DependentUpon", xlSettingsItem.GetMetadata("Filename") + xlSettingsItem.GetMetadata("Extension"));
					outputs.Add(csharpSettingsItem);
					using (var provider = new CSharpCodeProvider())
					{
						Log.LogMessage(MessageImportance.High, $"Generating C# class file for excel environment settings file '{xlSettingsItem.GetMetadata("FullPath")}'.");
						var xmlDocument = new XmlDocument();
						xmlDocument.Load(xlSettingsItem.GetMetadata("FullPath"));
						var compileUnit = xmlDocument.ConvertToEnvironmentSettingsCodeCompileUnit(
							xlSettingsItem.GetMetadata("Namespace"),
							xlSettingsItem.GetMetadata("TypeName"),
							xlSettingsItem.GetMetadata("Filename"));
						provider.GenerateAndSaveCodeFromCompileUnit(compileUnit, csharpSettingsItem.ItemSpec);
					}
				}
				// TODO ensure all generated classes have the same target environment value list
				CSharpEnvironmentSettings = outputs.ToArray();
				return true;
			}
			catch (Exception exception)
			{
				Log.LogErrorFromException(exception, true, true, null);
				return false;
			}
		}

		#endregion

		[Output]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "MSBuild Task API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MSBuild Task API.")]
		public ITaskItem[] CSharpEnvironmentSettings { get; private set; }

		[Required]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "MSBuild Task API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MSBuild Task API.")]
		public ITaskItem[] ExcelEnvironmentSettings { get; set; }

		private string ComputeOutputPath(ITaskItem settingsFileItem)
		{
			var inputPath = settingsFileItem.GetMetadata("Link").IfNotNullOrEmpty(p => p) ?? settingsFileItem.GetMetadata("Identity");
			return Path.Combine(Path.GetDirectoryName(inputPath)!, settingsFileItem.GetMetadata("TypeName") + ".Designer.cs");
		}
	}
}
