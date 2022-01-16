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
using System.Reflection;
using Be.Stateless.BizTalk.Dsl.Pipeline;

namespace Be.Stateless.BizTalk.CodeDom.Pipeline
{
	internal static class CodeNamespaceExtensions
	{
		internal static CodeTypeDeclaration AddPipelineClass<T>(this CodeNamespace @namespace, Pipeline<T> pipeline) where T : IPipelineStageList
		{
			if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
			if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
			var @class = new CodeTypeDeclaration {
				IsClass = true,
				TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed,
				Name = pipeline.GetType().Name,
				BaseTypes = {
					new CodeTypeReference(
						pipeline is ReceivePipeline
							? typeof(Microsoft.BizTalk.PipelineOM.ReceivePipeline)
							: typeof(Microsoft.BizTalk.PipelineOM.SendPipeline))
				}
			};
			@class.AddGeneratedCodeAttribute();
			@namespace.Types.Add(@class);
			return @class;
		}
	}
}
