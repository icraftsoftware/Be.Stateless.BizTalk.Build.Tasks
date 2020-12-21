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
using System.Linq;
using Microsoft.BizTalk.Studio.Extensibility;
using Microsoft.BizTalk.TOM;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.BizTalkProject.Base;
using Microsoft.VisualStudio.BizTalkProject.BuildTasks;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	// see Microsoft.VisualStudio.BizTalkProject.BuildTasks.SchemaCompiler, Microsoft.VisualStudio.BizTalkProject.BuildTasks, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
	// TODO add disabling warning support in *.*proj files for envelope where not all root is a body xpath
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "MSBuild Task.")]
	public class GenerateCSharpSchema : BizTalkCompilationTask
	{
		#region Base Class Member Overrides

		protected override IBizTalkCompiler BizTalkCompiler => new EditorCompiler();

		protected override List<BizTalkFileInfo> FilesToCompile =>
			XmlSchemas.Select(xmlSchema => (BizTalkFileInfo) SchemaBuildFileInfo.GetSchemaFileInfo(xmlSchema, RootNamespace)).ToList();

		protected override ITaskItem[] OutputItemGroup
		{
			set => CSharpSchemas = value;
		}

		#endregion

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "MSBuild Task API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MSBuild Task API.")]
		[Output]
		public ITaskItem[] CSharpSchemas { get; private set; }

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "MSBuild Task API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MSBuild Task API.")]
		[Required]
		public ITaskItem[] XmlSchemas { get; set; }
	}
}
