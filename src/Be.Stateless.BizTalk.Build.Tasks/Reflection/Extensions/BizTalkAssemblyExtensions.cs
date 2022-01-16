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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.BizTalk.XLANGs.BTXEngine;
using Microsoft.XLANGs.BaseTypes;

namespace Be.Stateless.BizTalk.Reflection.Extensions
{
	public static class BizTalkAssemblyExtensions
	{
		/// <summary>
		/// Checks whether <paramref name="assembly"/> is a BizTalk Server assembly.
		/// </summary>
		/// <param name="assembly">
		/// The assembly to check.
		/// </param>
		/// <returns>
		/// <c>true</c> if <paramref name="assembly"/> is a BizTalk Server assembly, <c>false</c> otherwise.
		/// </returns>
		public static bool IsBizTalkAssembly(this Assembly assembly)
		{
			if (assembly == null) throw new ArgumentNullException(nameof(assembly));
			return assembly.GetCustomAttributes(typeof(BizTalkAssemblyAttribute), false).Any();
		}

		/// <summary>
		/// Returns the BizTalk Server Orchestrations' types, i.e. types derived from <see cref="BTXService"/>, defined in the
		/// <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly">
		/// The assembly to introspect for <see cref="BTXService"/>-derived types.
		/// </param>
		/// <returns>
		/// The <see cref="Array"/> of <see cref="BTXService"/>-derived types defined in the <paramref name="assembly"/>.
		/// </returns>
		[SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
		public static Type[] GetOrchestrationTypes(this Assembly assembly)
		{
			return !assembly.IsBizTalkAssembly()
				? Array.Empty<Type>()
				: assembly.GetTypes().Where(t => typeof(BTXService).IsAssignableFrom(t) && !t.IsAbstract).ToArray();
		}
	}
}
