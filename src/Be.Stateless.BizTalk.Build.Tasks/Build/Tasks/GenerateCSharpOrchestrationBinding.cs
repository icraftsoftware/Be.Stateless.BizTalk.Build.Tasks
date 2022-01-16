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
using System.Reflection;
using Be.Stateless.BizTalk.CodeDom.Binding;
using Be.Stateless.BizTalk.CSharp.Extensions;
using Be.Stateless.BizTalk.Reflection.Extensions;
using Microsoft.Build.Framework;
using Microsoft.CSharp;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "MSBuild Task.")]
	public class GenerateCSharpOrchestrationBinding : TranspilationTask
	{
		#region Base Class Member Overrides

		protected override string FallBackRootPath => Path.Combine(RootPath, "Orchestrations");

		protected override Type[] InputTypes => OrchestrationAssemblies
			.Select(ra => ra.GetMetadata("Identity"))
			.Select(Assembly.LoadFrom)
			.ToArray() // make sure all assemblies are loaded before proceeding with reflection
			.SelectMany(a => a.GetOrchestrationTypes())
			.ToArray();

		protected override string OutputFileExtension => "OrchestrationBinding.Designer.cs";

		protected override void Transpile(Type type, ITaskItem outputTaskItem)
		{
			using (var provider = new CSharpCodeProvider())
			{
				Log.LogMessage(MessageImportance.High, "Generating orchestration C# binding class '{0}'.", type.FullName);
				provider.GenerateAndSaveCodeFromCompileUnit(type.ConvertToOrchestrationBindingCodeCompileUnit(), outputTaskItem.ItemSpec);
			}
		}

		#endregion

		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "MSBuild Task API.")]
		[Output]
		public ITaskItem[] CSharpOrchestrationBindings => OutputTaskItems.ToArray();

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "MSBuild Task API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MSBuild Task API.")]
		[Required]
		public ITaskItem[] OrchestrationAssemblies { get; set; }
	}
}
