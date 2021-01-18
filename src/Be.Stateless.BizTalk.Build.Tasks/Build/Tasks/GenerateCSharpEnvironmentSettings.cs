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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using Be.Stateless.Extensions;
using Be.Stateless.Resources;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "MSBuild Task.")]
	public class GenerateCSharpEnvironmentSettings : Task
	{
		#region Nested Type: Stringifier

		[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "XSLT extension object.")]
		private class Stringifier
		{
			public string Escape(string value)
			{
				// http://stackoverflow.com/questions/323640/can-i-convert-a-c-sharp-string-value-to-an-escaped-string-literal
				using (var writer = new StringWriter())
				using (var provider = CodeDomProvider.CreateProvider("CSharp"))
				{
					provider.GenerateCodeFromExpression(new CodePrimitiveExpression(value), writer, null);
					return writer.ToString();
				}
			}
		}

		#endregion

		#region Nested Type: Typifier

		[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "XSLT extension object.")]
		private class Typifier
		{
			public bool IsInteger(string value)
			{
				return int.TryParse(value, out _);
			}
		}

		#endregion

		static GenerateCSharpEnvironmentSettings()
		{
			var type = typeof(GenerateCSharpEnvironmentSettings);
			_xslt = ResourceManager.Load(
				type.Assembly,
				type.FullName + ".xslt",
				stream => {
					using (var xmlReader = XmlReader.Create(stream))
					{
						var xslt = new XslCompiledTransform(true);
						xslt.Load(xmlReader, XsltSettings.TrustedXslt, new XmlUrlResolver());
						return xslt;
					}
				});
		}

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
					using (var reader = XmlReader.Create(xlSettingsItem.GetMetadata("FullPath")))
					using (var writer = File.CreateText(csharpSettingsItem.GetMetadata("FullPath")))
					{
						var arguments = new XsltArgumentList();
						arguments.AddExtensionObject("urn:extensions.stateless.be:biztalk:environment-setting-class-generation:stringifier:2021:01", new Stringifier());
						arguments.AddExtensionObject("urn:extensions.stateless.be:biztalk:environment-setting-class-generation:typifier:2021:01", new Typifier());
						arguments.AddParam("clr-namespace-name", string.Empty, xlSettingsItem.GetMetadata("Namespace"));
						arguments.AddParam("clr-class-name", string.Empty, xlSettingsItem.GetMetadata("TypeName"));
						arguments.AddParam("settings-file-name", string.Empty, xlSettingsItem.GetMetadata("Filename"));
						_xslt.Transform(reader, arguments, writer);
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

		private static readonly XslCompiledTransform _xslt;
	}
}
