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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Be.Stateless.BizTalk.Dsl.Pipeline;
using Be.Stateless.Extensions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.PipelineEditor.PolicyFile;
using Microsoft.BizTalk.PipelineOM;
using Stage = Be.Stateless.BizTalk.Dsl.Pipeline.Stage;

namespace Be.Stateless.BizTalk.CodeDom.Pipeline
{
	internal static class CodeConstructorExtensions
	{
		[SuppressMessage("ReSharper", "SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault", Justification = "See Microsoft PipelineEditor.PipelineCompiler.")]
		internal static void AddStage(this CodeConstructor constructor, Stage stage)
		{
			var executionMode = stage.StagePolicy.ExecutionMethod switch {
				ExecMethod.All => ExecutionMode.all,
				ExecMethod.FirstMatch => ExecutionMode.firstRecognized,
				_ => throw new ArgumentOutOfRangeException(
					nameof(stage),
					stage.StagePolicy.ExecutionMethod,
					$"Stage '{stage.Category.Name}' Execution Method '{stage.StagePolicy.ExecutionMethod}' is not supported; only {nameof(ExecMethod.All)} and {nameof(ExecMethod.FirstMatch)} are supported.")
			};
			var invokeExpression = new CodeMethodInvokeExpression(
				new CodeThisReferenceExpression(),
				nameof(Microsoft.BizTalk.PipelineOM.Pipeline.AddStage),
				new CodeSnippetExpression($"{typeof(Microsoft.BizTalk.PipelineOM.Stage).FullName}.{stage.Category.Name}"),
				new CodeSnippetExpression($"{typeof(ExecutionMode).FullName}.{executionMode}"));
			constructor.Statements.Add(
				constructor.Statements.Count == 0
					? new CodeVariableDeclarationStatement(typeof(Microsoft.BizTalk.PipelineOM.Stage), VARIABLE_NAME, invokeExpression)
					: new CodeAssignStatement(new CodeVariableReferenceExpression(VARIABLE_NAME), invokeExpression));
		}

		public static CodeVariableDeclarationStatement DeclareComponent(this CodeConstructor constructor, IPipelineComponentDescriptor component, string variableName)
		{
			var declarationStatement = new CodeVariableDeclarationStatement(
				typeof(IBaseComponent),
				variableName,
				new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(typeof(PipelineManager)),
					nameof(PipelineManager.CreateComponent),
					new CodeSnippetExpression($"\"{component.AssemblyQualifiedName}\"")));
			constructor.Statements.Add(declarationStatement);
			return declarationStatement;
		}

		[SuppressMessage("ReSharper", "CoVariantArrayConversion")]
		internal static void ConfigureComponent(
			this CodeConstructor constructor,
			CodeVariableDeclarationStatement componentDeclaration,
			IEnumerable<PropertyContents> propertyContents)
		{
			constructor.Statements.Add(
				new CodeConditionStatement(
					// see https://stackoverflow.com/questions/52157314/c-sharp-codedom-as-and-is-keywords-functionality
					// see https://www.codeproject.com/Articles/12131/Commonly-Used-NET-Coding-Patterns-in-CodeDom#CodePatternIsInstExpression
					new CodeMethodInvokeExpression(
						new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(componentDeclaration.Name), nameof(GetType)),
						nameof(Type.IsInstanceOfType),
						new CodeTypeOfExpression(typeof(IPersistPropertyBag))),
					new CodeExpressionStatement(
						new CodeMethodInvokeExpression(
							new CodeCastExpression(typeof(IPersistPropertyBag), new CodeVariableReferenceExpression(componentDeclaration.Name)),
							nameof(IPersistPropertyBag.Load),
							new CodeObjectCreateExpression(
								typeof(PropertyBag),
								new CodeObjectCreateExpression(
									typeof(ArrayList),
									new CodeArrayCreateExpression(
										typeof(PropertyContents),
										propertyContents.Select(
												pc => new CodeObjectCreateExpression(
													typeof(PropertyContents),
													new CodePrimitiveExpression(pc.Name),
													pc.Value.ToPrimitiveType()))
											.ToArray()
									))),
							new CodePrimitiveExpression(0)))));
		}

		internal static void AddComponent(this CodeConstructor constructor, CodeVariableDeclarationStatement componentDeclaration)
		{
			constructor.Statements.Add(
				new CodeExpressionStatement(
					new CodeMethodInvokeExpression(
						new CodeThisReferenceExpression(),
						nameof(Microsoft.BizTalk.PipelineOM.Pipeline.AddComponent),
						new CodeVariableReferenceExpression(VARIABLE_NAME),
						new CodeVariableReferenceExpression(componentDeclaration.Name))));
		}

		private static CodeExpression ToPrimitiveType(this object value)
		{
			return value.IfNotNull(v => v.GetType().IsEnum)
				? new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(value.GetType()), value.ToString())
				: value.IfNotNull(v => v is string)
					? new CodeSnippetExpression("\"" + value + "\"")
					: new CodePrimitiveExpression(value);
		}

		private const string VARIABLE_NAME = "stage";
	}
}
