#region Copyright & License

// Copyright © 2012 - 2022 François Chabot
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Be.Stateless.BizTalk.Dsl.Pipeline.Xml.Serialization;
using Microsoft.Build.Framework;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "MSBuild Task.")]
	public class GenerateDesignerPipeline : PipelineDefinitionTranspilationTask
	{
		#region Base Class Member Overrides

		protected override string OutputFileExtension => ".btp";

		protected override void Transpile(Type type, ITaskItem outputTaskItem)
		{
			Log.LogMessage(MessageImportance.High, $"Generating designer file for type '{type.FullName}'.");
			Directory.CreateDirectory(Path.GetDirectoryName(outputTaskItem.ItemSpec)!);
			var serializer = type.GetPipelineDesignerDocumentSerializer();
			serializer.Save(outputTaskItem.ItemSpec);
			outputTaskItem.SetMetadata("TypeName", type.Name);
			outputTaskItem.SetMetadata("Namespace", type.Namespace);
		}

		#endregion

		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "MSBuild Task API.")]
		[Output]
		public ITaskItem[] DesignerPipelines => OutputTaskItems.ToArray();
	}
}
