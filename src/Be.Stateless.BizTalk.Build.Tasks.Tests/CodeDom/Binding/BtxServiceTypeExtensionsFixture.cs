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

using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Be.Stateless.BizTalk.Dsl.Binding;
using Be.Stateless.BizTalk.Orchestrations.Bound;
using Be.Stateless.BizTalk.Reflection.Extensions;
using Be.Stateless.Linq.Extensions;
using Be.Stateless.Resources;
using FluentAssertions;
using Microsoft.CSharp;
using Xunit;

namespace Be.Stateless.BizTalk.CodeDom.Binding
{
	public class BtxServiceTypeExtensionsFixture
	{
		[Fact]
		public void CompileToDynamicAssembly()
		{
			var btxServiceType = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
				.Distinct((a1, a2) => a1.FullName == a2.FullName).Select(Assembly.Load)
				.Where(a => a.IsBizTalkAssembly())
				.SelectMany(a => a.GetOrchestrationTypes())
				.First();

			var assembly = btxServiceType
				.CompileToDynamicAssembly();

			var orchestrationBinding = assembly.CreateInstance(btxServiceType.FullName + CodeTypeDeclarationExtensions.ORCHESTRATION_BINDING_TYPE_NAME_SUFFIX);
			orchestrationBinding.Should().NotBeNull().And.BeAssignableTo<OrchestrationBindingBase<Process>>();
		}

		[Fact]
		public void ConvertToOrchestrationBindingCodeCompileUnit()
		{
			var btxServiceType = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
				.Distinct((a1, a2) => a1.FullName == a2.FullName).Select(Assembly.Load)
				.Where(a => a.IsBizTalkAssembly())
				.SelectMany(a => a.GetOrchestrationTypes())
				.First();

			var builder = new StringBuilder();
			using (var provider = new CSharpCodeProvider())
			using (var writer = new StringWriter(builder))
			{
				provider.GenerateCodeFromCompileUnit(
					btxServiceType.ConvertToOrchestrationBindingCodeCompileUnit(),
					writer,
					new() { BracingStyle = "C", IndentString = "\t", VerbatimOrder = true });
			}

			// being resilient to runtime version in CodeDom heading comment
			Regex.Replace(builder.ToString(), @"(//\s+)Runtime Version:\d\.\d\.\d+\.\d+", @"$1Runtime Version:4.0.30319.42000", RegexOptions.Multiline)
				.Should().Be(
					ResourceManager.Load(
						Assembly.GetExecutingAssembly(),
						"Be.Stateless.BizTalk.Resources.Binding.ProcessOrchestrationBinding.Designer.cs",
						s => new StreamReader(s).ReadToEnd()));
		}
	}
}
