#region Copyright & License

// Copyright © 2012 - 2020 François Chabot
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
using System.Linq;
using System.Reflection;
using Be.Stateless.BizTalk.CSharp.Extensions;
using Be.Stateless.BizTalk.Dsl.Binding.CodeDom;
using Be.Stateless.Linq.Extensions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CSharp;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Msbuild Task.")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public class GenerateCSharpOrchestrationBinding : TranspilationTask
	{
		#region Base Class Member Overrides

		protected override void Transpile()
		{
			using (var provider = new CSharpCodeProvider())
			{
				DeletePreviousCSharpOrchestrationBindings();
				var outputs = new List<ITaskItem>();
				foreach (var orchestration in Orchestrations)
				{
					var outputDirectory = ComputeTranspilationOutputDirectory(orchestration, "Orchestrations");
					var outputFilePath = Path.Combine(outputDirectory, $"{orchestration.Name}{EXTENSION}");
					Log.LogMessage(MessageImportance.High, "Generating orchestration C# binding class '{0}'.", orchestration.FullName);
					provider.GenerateAndSaveCodeFromCompileUnit(orchestration.ConvertToOrchestrationBindingCodeCompileUnit(), outputFilePath);
					Log.LogMessage(MessageImportance.Low, $"Adding orchestration binding class  to output item {nameof(CSharpOrchestrationBindings)} group {outputFilePath}");
					outputs.Add(new TaskItem(outputFilePath));
				}
				CSharpOrchestrationBindings = outputs.ToArray();
			}
		}

		#endregion

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		[Output]
		public ITaskItem[] CSharpOrchestrationBindings { get; private set; }

		[Required]
		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		public ITaskItem[] OrchestrationAssemblies { get; set; }

		[SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Local")]
		private Type[] Orchestrations => OrchestrationAssemblies
			.Select(ra => ra.GetMetadata("Identity"))
			.Select(Assembly.LoadFrom)
			.ToArray() // make sure all assemblies are loaded before proceeding with reflection
			.SelectMany(a => a.GetOrchestrationTypes())
			.ToArray();

		private void DeletePreviousCSharpOrchestrationBindings()
		{
			Log.LogMessage(MessageImportance.Normal, "Deleting previously generated orchestration C# binding class files.");
			Directory.EnumerateFiles(RootPath, $"*{EXTENSION}", SearchOption.AllDirectories)
				.ForEach(
					filePath => {
						Log.LogMessage(MessageImportance.Low, $"Deleting file {filePath}.");
						File.Delete(filePath);
						CleanFolder(Path.GetDirectoryName(filePath));
					});
		}

		private void CleanFolder(string directory)
		{
			while (!string.Equals(directory, RootPath, StringComparison.OrdinalIgnoreCase) && !Directory.EnumerateFileSystemEntries(directory!).Any())
			{
				Log.LogMessage(MessageImportance.Low, $"Deleting directory {directory}.");
				Directory.Delete(directory);
				directory = Path.GetDirectoryName(directory);
			}
		}

		private const string EXTENSION = "OrchestrationBinding.Designer.cs";
	}
}
