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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Be.Stateless.BizTalk.Dsl.Pipeline.CodeDom;
using Be.Stateless.Extensions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CSharp;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Msbuild Task.")]
	public class GenerateCSharpPipeline : CompilePipelineDefinitionBaseTask
	{
		#region Base Class Member Overrides

		public override bool Execute()
		{
			try
			{
				BizTalkAssemblyResolver.Register(msg => Log.LogMessage(msg), ReferencedPaths);
				using (var provider = new CSharpCodeProvider())
				{
					var outputs = new List<ITaskItem>();
					foreach (var pipelineType in PipelineDefinitions)
					{
						var outputDirectory = ComputePipelineOutputDirectory(pipelineType);
						var outputFilePath = Path.Combine(outputDirectory, $"{pipelineType.Name}{EXTENSION}");
						Log.LogMessage(MessageImportance.High, $"Generating pipeline C# code file '{pipelineType.FullName}{EXTENSION}'.");
						Directory.CreateDirectory(outputDirectory);
						using (var writer = new StreamWriter(outputFilePath))
						{
							provider.GenerateCodeFromCompileUnit(
								pipelineType.ConvertToCodeCompileUnit(),
								writer,
								new CodeGeneratorOptions { BracingStyle = "C", IndentString = "\t", VerbatimOrder = true });
						}

						Log.LogMessage(MessageImportance.High, $"Adding pipeline to output item {nameof(CSharpPipelines)} group {outputFilePath}");
						outputs.Add(new TaskItem(outputFilePath));
					}
					CSharpPipelines = outputs.ToArray();
				}
				return true;
			}
			catch (Exception exception)
			{
				if (exception.IsFatal()) throw;
				Log.LogErrorFromException(exception, true, true, null);
				return false;
			}
			finally
			{
				BizTalkAssemblyResolver.Unregister();
			}
		}

		#endregion

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		[Output]
		public ITaskItem[] CSharpPipelines { get; private set; }

		private const string EXTENSION = ".btp.cs";
	}
}
