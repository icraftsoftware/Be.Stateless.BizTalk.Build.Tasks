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
using System.IO;
using System.Linq;
using Be.Stateless.BizTalk.Dsl.Pipeline.Extensions;
using Microsoft.Build.Framework;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	public abstract class PipelineDefinitionTranspilationTask : TranspilationTask
	{
		#region Base Class Member Overrides

		protected override string FallBackRootPath => Path.Combine(RootPath, "Pipelines");

		protected override Type[] InputTypes => PipelineDefinitionAssemblies
			.Select(pda => pda.GetMetadata("Identity"))
			.GetPipelineDefinitionTypes();

		#endregion

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "MSBuild Task API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MSBuild Task API.")]
		[Required]
		public ITaskItem[] PipelineDefinitionAssemblies { get; set; }
	}
}
