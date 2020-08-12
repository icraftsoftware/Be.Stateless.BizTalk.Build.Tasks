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
using Be.Stateless.BizTalk.Dsl.Pipeline.Xml.Serialization;
using Be.Stateless.Extensions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Msbuild Task.")]
	public class GenerateDesignerPipeline : CompilePipelineDefinitionBaseTask
	{
		#region Base Class Member Overrides

		public override bool Execute()
		{
			try
			{
				BizTalkAssemblyResolver.Register(msg => Log.LogMessage(msg), ReferencedPaths);
				var outputs = new List<ITaskItem>();
				foreach (var pipelineType in PipelineDefinitions)
				{
					var outputDirectory = ComputePipelineOutputDirectory(pipelineType);
					var outputFilePath = Path.Combine(outputDirectory, $"{pipelineType.Name}{EXTENSION}");
					Log.LogMessage(MessageImportance.High, $"Generating pipeline designer file '{pipelineType.FullName}{EXTENSION}'.");
					Directory.CreateDirectory(outputDirectory);
					var serializer = pipelineType.GetPipelineDesignerDocumentSerializer();
					serializer.Save(outputFilePath);

					Log.LogMessage(MessageImportance.High, $"Adding pipeline designer to output item {nameof(DesignerPipelines)} group {outputFilePath}");
					var taskItem = new TaskItem(outputFilePath);
					taskItem.SetMetadata("TypeName", pipelineType.Name);
					taskItem.SetMetadata("Namespace", pipelineType.Namespace);
					outputs.Add(taskItem);
				}
				DesignerPipelines = outputs.ToArray();
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
		public ITaskItem[] DesignerPipelines { get; private set; }

		private const string EXTENSION = ".btp";
	}
}
