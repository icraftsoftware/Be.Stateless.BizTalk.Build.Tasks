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

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Be.Stateless.BizTalk.Reflection;
using Be.Stateless.BizTalk.Reflection.Extensions;
using Microsoft.Build.Framework;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "MSBuild Task.")]
	public class ResolveBizTalkAssembly : BizTalkAssemblyResolvingTask
	{
		#region Base Class Member Overrides

		protected override void ExecuteCore()
		{
			BizTalkAssemblies = ReferencedAssemblies
				.Where(a => AssemblyLoader.Load(a.GetMetadata("Identity")).IsBizTalkAssembly())
				.ToArray();
		}

		#endregion

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "MSBuild Task API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MSBuild Task API.")]
		[Output]
		public ITaskItem[] BizTalkAssemblies { get; private set; }
	}
}
