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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Be.Stateless.BizTalk.Dsl.Pipeline;
using Be.Stateless.BizTalk.Dsl.Pipeline.Extensions;
using Be.Stateless.BizTalk.Dsl.Pipeline.Xml.Serialization;
using Microsoft.BizTalk.Component;
using Microsoft.CSharp;

namespace Be.Stateless.BizTalk.CodeDom.Pipeline
{
	public static class PipelineExtensions
	{
		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public DSL API.")]
		public static CodeCompileUnit ConvertToPipelineRuntimeCodeCompileUnit(this Type pipelineType)
		{
			return pipelineType.AsReceivePipeline()?.ConvertToPipelineRuntimeCodeCompileUnit()
				?? pipelineType.AsSendPipeline()?.ConvertToPipelineRuntimeCodeCompileUnit()
				?? throw new InvalidOperationException("Pipeline instantiation failure.");
		}

		public static CodeCompileUnit ConvertToPipelineRuntimeCodeCompileUnit<T>(this Pipeline<T> pipeline)
			where T : IPipelineStageList
		{
			if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

			//see Microsoft.BizTalk.PipelineEditor.PipelineCompiler::GenerateCompilerOutput, Microsoft.BizTalk.PipelineOM, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
			var @namespace = new CodeNamespace(pipeline.GetType().Namespace);
			// using System.CodeDom.Compiler;
			@namespace.ImportNamespace(typeof(GeneratedCodeAttribute));

			var @class = @namespace.AddPipelineClass(pipeline);
			@class.AddConstructor(pipeline.Stages);
			@class.AddXmlContentProperty(pipeline.GetPipelineRuntimeDocumentSerializer().Serialize());
			@class.AddVersionDependentGuidProperty(pipeline.VersionDependentGuid);
			return new() { Namespaces = { @namespace } };
		}

		[SuppressMessage("ReSharper", "CommentTypo")]
		internal static Assembly CompileToDynamicAssembly(this Type type)
		{
			using (var provider = new CSharpCodeProvider())
			{
				var parameters = new CompilerParameters(
					new[] {
						// System.dll
						typeof(GeneratedCodeAttribute).Assembly.Location,
						// Microsoft.BizTalk.Pipeline.dll
						typeof(Microsoft.BizTalk.Component.Interop.IBaseComponent).Assembly.Location,
						// Microsoft.BizTalk.Pipeline.Components
						typeof(XmlAsmComp).Assembly.Location,
						// Microsoft.BizTalk.PipelineOM.dll
						typeof(Microsoft.BizTalk.PipelineEditor.PropertyContents).Assembly.Location,
						// Microsoft.XLANGs.BaseTypes.dll
						typeof(Microsoft.XLANGs.BaseTypes.PipelineBase).Assembly.Location
					}) {
					GenerateInMemory = true,
					IncludeDebugInformation = true
				};

				var compileUnit = type.ConvertToPipelineRuntimeCodeCompileUnit();
				var results = provider.CompileAssemblyFromDom(parameters, compileUnit);
				if (results.Errors.Count > 0) throw new(results.Errors.Cast<CompilerError>().Aggregate(string.Empty, (k, e) => $"{k}\r\n{e}"));

				return results.CompiledAssembly;
			}
		}
	}
}
