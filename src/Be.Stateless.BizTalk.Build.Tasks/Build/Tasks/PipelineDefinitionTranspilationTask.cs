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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Be.Stateless.BizTalk.Dsl.Pipeline.Extensions;
using Microsoft.Build.Framework;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Implements Msbuild Task API.")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Implements Msbuild Task API.")]
	public abstract class PipelineDefinitionTranspilationTask : TranspilationTask
	{
		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		[Required]
		public ITaskItem[] PipelineDefinitionAssemblies { get; set; }

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		[SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
		protected Type[] PipelineDefinitions => PipelineDefinitionAssemblies
			.Select(pda => pda.GetMetadata("Identity"))
			.GetPipelineDefinitionTypes();

		protected string ComputePipelineTranspilationOutputDirectory(Type pipeline)
		{
			return ComputeTranspilationOutputDirectory(pipeline, "Pipelines");
		}
	}
}
