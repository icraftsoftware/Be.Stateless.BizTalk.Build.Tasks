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
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Be.Stateless.BizTalk.Dsl;
using Be.Stateless.BizTalk.Dsl.Binding;
using Be.Stateless.Linq.Extensions;
using Be.Stateless.Reflection;
using Microsoft.BizTalk.XLANGs.BTXEngine;
using Microsoft.CSharp;
using Microsoft.XLANGs.BaseTypes;
using Microsoft.XLANGs.BizTalk.ProcessInterface;
using Microsoft.XLANGs.Core;

namespace Be.Stateless.BizTalk.CodeDom.Binding
{
	public static class BtxServiceTypeExtensions
	{
		[SuppressMessage("ReSharper", "CommentTypo")]
		public static CodeCompileUnit ConvertToOrchestrationBindingCodeCompileUnit(this Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (!typeof(BTXService).IsAssignableFrom(type)) throw new ArgumentException($"{type.FullName} is not an orchestration type.", nameof(type));

			var ports = (PortInfo[]) Reflector.GetField(type, "_portInfo");
			var unboundPorts = ports
				// filter out direct ports
				.Where(p => p.FindAttribute(typeof(DirectBindingAttribute)) == null)
				.ToArray();

			// namespace <orchestration's namespace>
			var @namespace = new CodeNamespace(type.Namespace);
			// using System;
			@namespace.ImportNamespace(typeof(Action<>));
			// using System.CodeDom.Compiler;
			@namespace.ImportNamespace(typeof(GeneratedCodeAttribute));
			// using Be.Stateless.BizTalk.Dsl.Binding;
			@namespace.ImportNamespace(typeof(OrchestrationBindingBase<>));
			// using Microsoft.XLANGs.Core;
			@namespace.ImportNamespace(typeof(PortInfo));

			if (unboundPorts.Any())
			{
				var @interface = @namespace.AddBindingInterface(type);
				unboundPorts.ForEach(port => @interface.AddPortPropertyMember(port));
				var @class = @namespace.AddBindingClass(type, @interface);
				ports.ForEach(port => @class.AddPortOperationMember(port));
				@class.AddDefaultConstructor();
				@class.AddBindingConfigurationConstructor(@interface);
				unboundPorts.ForEach(port => @class.AddPortPropertyMember(port, @interface));
			}
			else
			{
				var @class = @namespace.AddBindingClass(type);
				ports.ForEach(port => @class.AddPortOperationMember(port));
				@class.AddDefaultConstructor();
				@class.AddBindingConfigurationConstructor();
			}

			var compileUnit = new CodeCompileUnit();
			compileUnit.Namespaces.Add(@namespace);
			return compileUnit;
		}

		[SuppressMessage("ReSharper", "CommentTypo")]
		internal static Assembly CompileToDynamicAssembly(this Type type)
		{
			using (var provider = new CSharpCodeProvider())
			{
				var parameters = new CompilerParameters(
					new[] {
						// System.dll
						typeof(GeneratedCodeAttribute).Assembly.Location,
						// Microsoft.XLANGs.BizTalk.Engine.dll
						typeof(BTXService).Assembly.Location,
						// Microsoft.XLANGs.BizTalk.ProcessInterface.dll
						typeof(IOrchestration).Assembly.Location,
						// Microsoft.XLANGs.Engine.dll
						typeof(Service).Assembly.Location,
						// Be.Stateless.BizTalk.Dsl.Abstractions.dll
						typeof(IDslSerializer).Assembly.Location,
						// Be.Stateless.BizTalk.Dsl.Binding.dll
						typeof(IOrchestrationBinding).Assembly.Location,
						// orchestration's assembly
						type.Assembly.Location
					}) {
					GenerateInMemory = true,
					IncludeDebugInformation = true
				};

				var compileUnit = type.ConvertToOrchestrationBindingCodeCompileUnit();
				var results = provider.CompileAssemblyFromDom(parameters, compileUnit);
				if (results.Errors.Count > 0) throw new(results.Errors.Cast<CompilerError>().Aggregate(string.Empty, (k, e) => $"{k}\r\n{e}"));

				return results.CompiledAssembly;
			}
		}
	}
}
