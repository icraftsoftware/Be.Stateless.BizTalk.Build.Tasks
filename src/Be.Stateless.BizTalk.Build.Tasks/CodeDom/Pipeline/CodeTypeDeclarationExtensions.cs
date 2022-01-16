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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Be.Stateless.BizTalk.Dsl.Pipeline;
using Microsoft.BizTalk.PipelineOM;

namespace Be.Stateless.BizTalk.CodeDom.Pipeline
{
	internal static class CodeTypeDeclarationExtensions
	{
		internal static void AddConstructor<T>(this CodeTypeDeclaration @class, T stages) where T : IPipelineStageList
		{
			if (@class == null) throw new ArgumentNullException(nameof(@class));
			if (stages == null) throw new ArgumentNullException(nameof(stages));
			var constructor = new CodeConstructor { Name = @class.Name, Attributes = MemberAttributes.Public };
			var compId = 0;
			foreach (var stage in ((StageList) (IPipelineStageList) stages).Where(stage => stage.Components.Count > 0))
			{
				constructor.AddStage(stage);
				foreach (var component in (ComponentList) stage.Components)
				{
					var componentDeclaration = constructor.DeclareComponent(component, $"comp{compId++}");
					constructor.ConfigureComponent(componentDeclaration, component.PropertyContents.Select(pc => new PropertyContents(pc.Name, pc.Value)));
					constructor.AddComponent(componentDeclaration);
				}
			}
			@class.Members.Add(constructor);
		}

		[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
		internal static void AddXmlContentProperty(this CodeTypeDeclaration @class, string runtimePipelineDefinition)
		{
			var memberField = new CodeMemberField {
				Attributes = MemberAttributes.Const | MemberAttributes.Private,
				Name = "_strPipeline",
				Type = new(typeof(string)),
				InitExpression = new CodeSnippetExpression("@\"" + runtimePipelineDefinition + "\"")
			};
			@class.Members.Add(
				new CodeMemberProperty {
					Attributes = MemberAttributes.Public | MemberAttributes.Override,
					Name = nameof(Microsoft.BizTalk.PipelineOM.Pipeline.XmlContent),
					GetStatements = { new CodeMethodReturnStatement(new CodeVariableReferenceExpression(memberField.Name)) },
					HasSet = false,
					Type = new(typeof(string))
				});
			@class.Members.Add(memberField);
		}

		[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
		internal static void AddVersionDependentGuidProperty(this CodeTypeDeclaration @class, Guid versionDependentGuid)
		{
			var memberField = new CodeMemberField {
				Attributes = MemberAttributes.Const | MemberAttributes.Private,
				Name = "_versionDependentGuid",
				Type = new(typeof(string)),
				InitExpression = new CodePrimitiveExpression(versionDependentGuid.ToString())
			};
			@class.Members.Add(
				new CodeMemberProperty {
					Attributes = MemberAttributes.Public | MemberAttributes.Override,
					Name = nameof(Microsoft.BizTalk.PipelineOM.Pipeline.VersionDependentGuid),
					GetStatements = {
						new CodeMethodReturnStatement(new CodeObjectCreateExpression(typeof(Guid), new CodeVariableReferenceExpression(memberField.Name)))
					},
					HasSet = false,
					Type = new(typeof(Guid))
				});
			@class.Members.Add(memberField);
		}
	}
}
