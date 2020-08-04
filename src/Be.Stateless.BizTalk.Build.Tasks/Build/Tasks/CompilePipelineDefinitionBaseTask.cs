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
using Be.Stateless.BizTalk.Dsl.Pipeline;
using Be.Stateless.Extensions;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Implements Msbuild Task API.")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Implements Msbuild Task API.")]
	public abstract class CompilePipelineDefinitionBaseTask : Task
	{
		[Required]
		public ITaskItem[] PipelineDefinitionAssemblies { get; set; }

		[Required]
		public ITaskItem[] ReferencedAssemblies { get; set; }

		[Required]
		public string RootNamespace { get; set; }

		[Required]
		public string RootPath { get; set; }

		protected IEnumerable<Type> PipelineDefinitions => PipelineDefinitionAssemblies
			.Select(ra => ra.GetMetadata("Identity"))
			.Select(Assembly.LoadFrom)
			// make sure all assemblies are loaded before proceeding with reflection
			.ToArray()
			// discard Be.Stateless.BizTalk.Dsl.Pipeline assembly
			.Where(a => a != typeof(ReceivePipeline).Assembly)
			.SelectMany(a => a.GetTypes())
			.Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
			.Where(t => typeof(ReceivePipeline).IsAssignableFrom(t) || typeof(SendPipeline).IsAssignableFrom(t))
			.ToArray();

		protected string[] ReferencedPaths => ReferencedAssemblies
			.Select(ra => ra.GetMetadata("Identity"))
			.Select(Path.GetDirectoryName)
			.ToArray();

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		protected string ComputePipelineOutputDirectory(Type pipelineType)
		{
			var projectDirectory = RootPath ?? Directory.GetCurrentDirectory();
			var projectRootNamespace = RootNamespace ?? new Project(BuildEngine.ProjectFileOfTaskNode).AllEvaluatedProperties.Single(p => p.Name == "RootNamespace").EvaluatedValue;
			var relativePath = !pipelineType.Namespace.IsNullOrWhiteSpace() && pipelineType.Namespace.StartsWith(projectRootNamespace + ".")
				? pipelineType.Namespace.Substring(projectRootNamespace.Length + 1)
				: "Pipelines";
			return Path.Combine(projectDirectory, relativePath);
		}
	}
}
