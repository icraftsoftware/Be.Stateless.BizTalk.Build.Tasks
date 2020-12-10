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
using Be.Stateless.BizTalk.Dsl;
using Be.Stateless.Extensions;
using Be.Stateless.IO.Extensions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public abstract class TranspilationTask : Task
	{
		#region Base Class Member Overrides

		public override bool Execute()
		{
			try
			{
				BizTalkAssemblyResolver.Register(msg => Log.LogMessage(msg), ReferencedPaths);
				Transpile();
				return true;
			}
			catch (Exception exception)
			{
				if (exception.IsFatal()) throw;
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
		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		public ITaskItem[] ReferencedAssemblies { get; set; }

		[Required]
		public string RootNamespace { get; set; }

		[Required]
		public string RootPath { get; set; }

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		protected string[] ReferencedPaths => ReferencedAssemblies
			.Select(ra => ra.GetMetadata("Identity"))
			.Select(Path.GetDirectoryName)
			.ToArray();

		protected abstract void Transpile();

		protected string ComputeTranspilationOutputDirectory(Type type, string defaultRelativePath)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			var commonNamespacePrefix = new[] { RootNamespace, type.Namespace }.GetCommonPath('.');
			var relativePath = commonNamespacePrefix.IsNullOrEmpty()
				? defaultRelativePath
				: type.Namespace!.Substring(commonNamespacePrefix.Length + 1).Replace('.', Path.DirectorySeparatorChar);
			return Path.Combine(RootPath, relativePath);
		}
	}
}
