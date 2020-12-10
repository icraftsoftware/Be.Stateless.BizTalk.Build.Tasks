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
using System.Reflection;
using Be.Stateless.BizTalk.Dsl;
using Be.Stateless.BizTalk.Dsl.Binding.CodeDom;
using Be.Stateless.Extensions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "UnusedType.Global")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public class ResolveBizTalkAssembly : Task
	{
		#region Base Class Member Overrides

		public override bool Execute()
		{
			try
			{
				BizTalkAssemblyResolver.Register(msg => Log.LogMessage(msg), ReferencedPaths);
				BizTalkAssemblies = ReferencedAssemblies
					.Where(a => Assembly.LoadFrom(a.GetMetadata("Identity")).IsBizTalkAssembly())
					.ToArray();
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

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		[Output]
		public ITaskItem[] BizTalkAssemblies { get; private set; }

		[Required]
		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		public ITaskItem[] ReferencedAssemblies { get; set; }

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		private string[] ReferencedPaths => ReferencedAssemblies
			.Select(ra => ra.GetMetadata("Identity"))
			.Select(Path.GetDirectoryName)
			.ToArray();
	}
}
