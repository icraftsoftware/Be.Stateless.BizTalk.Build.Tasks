﻿#region Copyright & License

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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Be.Stateless.BizTalk.Dsl;
using Be.Stateless.BizTalk.Dsl.Pipeline;
using Be.Stateless.BizTalk.Dsl.Pipeline.Extensions;
using Be.Stateless.Extensions;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Implements Msbuild Task API.")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Implements Msbuild Task API.")]
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Msbuild Task.")]
	[Serializable]
	public class CompilePipelineDefinition : Task
	{
		#region Base Class Member Overrides

		[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
		public override bool Execute()
		{
			try
			{
				var assemblyPaths = PipelineDefinitionAssemblies
					.Select(ra => ra.GetMetadata("Identity"))
					.ToArray();
				BizTalkAssemblyResolver.Register(
					msg => Log.LogMessage(msg),
					assemblyPaths.Select(Path.GetDirectoryName).ToArray());

				// TODO refactor this in task ResolveReferencedBizTalkPipelineAssemblies
				var pipelineTypes = assemblyPaths
					.Select(Assembly.LoadFrom)
					// make sure all assemblies are loaded before proceeding with reflection
					.ToArray()
					// discard Be.Stateless.BizTalk.Dsl assembly
					.Where(a => a != typeof(ReceivePipeline).Assembly)
					.SelectMany(a => a.GetTypes())
					.Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
					.Where(t => typeof(ReceivePipeline).IsAssignableFrom(t) || typeof(SendPipeline).IsAssignableFrom(t));

				// TODO delete all previously generated files

				var outputs = new List<ITaskItem>();
				foreach (var pipelineType in pipelineTypes)
				{
					var path = ComputeOutputPath(pipelineType);
					Log.LogMessage(MessageImportance.High, "Generating pipeline '{0}'.", pipelineType.FullName);
					Directory.CreateDirectory(Path.GetDirectoryName(path));
					var serializer = ((IVisitable<IPipelineVisitor>) Activator.CreateInstance(pipelineType)).GetPipelineDesignerDocumentSerializer();
					serializer.Save(path);
					Log.LogMessage(MessageImportance.High, "Adding pipeline to item group {0}", path);
					var taskItem = new TaskItem(path);
					taskItem.SetMetadata("TypeName", pipelineType.Name);
					taskItem.SetMetadata("Namespace", pipelineType.Namespace);
					outputs.Add(taskItem);
				}
				PipelineFiles = outputs.ToArray();
				return true;
			}
			catch (Exception exception)
			{
				Log.LogErrorFromException(exception, true, true, null);
				return false;
			}
			finally
			{
				BizTalkAssemblyResolver.Unregister();
			}
		}

		#endregion

		[Required]
		public ITaskItem[] PipelineDefinitionAssemblies { get; set; }

		[Output]
		public ITaskItem[] PipelineFiles { get; private set; }

		[Required]
		public string RootNamespace { get; set; }

		[Required]
		public string RootPath { get; set; }

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		private string ComputeOutputPath(Type pipelineType)
		{
			var projectDirectory = RootPath ?? Directory.GetCurrentDirectory();
			var projectRootNamespace = RootNamespace ?? new Project(BuildEngine.ProjectFileOfTaskNode).AllEvaluatedProperties.Single(p => p.Name == "RootNamespace").EvaluatedValue;

			var relativePath = !pipelineType.Namespace.IsNullOrWhiteSpace() && pipelineType.Namespace.StartsWith(projectRootNamespace + ".")
				? pipelineType.Namespace.Substring(projectRootNamespace.Length + 1)
				: "Pipelines";
			var folder = Path.Combine(projectDirectory, relativePath);
			return Path.Combine(folder, pipelineType.Name + ".btp");
		}
	}
}
