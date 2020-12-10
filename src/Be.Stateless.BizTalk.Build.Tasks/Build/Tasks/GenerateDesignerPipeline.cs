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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Be.Stateless.BizTalk.Dsl.Pipeline.Xml.Serialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Msbuild Task.")]
	public class GenerateDesignerPipeline : PipelineDefinitionTranspilationTask
	{
		#region Base Class Member Overrides

		protected override void Transpile()
		{
			var outputs = new List<ITaskItem>();
			foreach (var pipeline in PipelineDefinitions)
			{
				var outputDirectory = ComputePipelineTranspilationOutputDirectory(pipeline);
				var outputFilePath = Path.Combine(outputDirectory, $"{pipeline.Name}.btp");
				Log.LogMessage(MessageImportance.High, $"Generating pipeline designer file '{outputFilePath}'.");
				Directory.CreateDirectory(outputDirectory);
				var serializer = pipeline.GetPipelineDesignerDocumentSerializer();
				serializer.Save(outputFilePath);

				Log.LogMessage(MessageImportance.Low, $"Adding pipeline designer to output item {nameof(DesignerPipelines)} group {outputFilePath}");
				var taskItem = new TaskItem(outputFilePath);
				taskItem.SetMetadata("TypeName", pipeline.Name);
				taskItem.SetMetadata("Namespace", pipeline.Namespace);
				outputs.Add(taskItem);
			}
			DesignerPipelines = outputs.ToArray();
		}

		#endregion

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		[Output]
		public ITaskItem[] DesignerPipelines { get; private set; }
	}
}
