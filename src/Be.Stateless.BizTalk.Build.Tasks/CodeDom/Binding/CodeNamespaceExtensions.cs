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
using System.Reflection;
using Be.Stateless.BizTalk.Dsl.Binding;
using Microsoft.BizTalk.XLANGs.BTXEngine;

namespace Be.Stateless.BizTalk.CodeDom.Binding
{
	internal static class CodeNamespaceExtensions
	{
		internal static CodeTypeDeclaration AddBindingInterface(this CodeNamespace @namespace, Type type)
		{
			if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (!typeof(BTXService).IsAssignableFrom(type)) throw new ArgumentException($"{type.FullName} is not an orchestration type.", nameof(type));

			var @interface = new CodeTypeDeclaration("I" + type.Name + CodeTypeDeclarationExtensions.ORCHESTRATION_BINDING_TYPE_NAME_SUFFIX) {
				TypeAttributes = TypeAttributes.NotPublic,
				IsPartial = true,
				// ! IsInterface must be set last !
				IsInterface = true
			};
			@interface.BaseTypes.Add(new CodeTypeReference(nameof(IOrchestrationBinding)));
			@interface.AddGeneratedCodeAttribute();
			@namespace.Types.Add(@interface);
			return @interface;
		}

		internal static CodeTypeDeclaration AddBindingClass(this CodeNamespace @namespace, Type type)
		{
			if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (!typeof(BTXService).IsAssignableFrom(type)) throw new ArgumentException($"{type.FullName} is not an orchestration type.", nameof(type));

			var @class = new CodeTypeDeclaration(type.Name + CodeTypeDeclarationExtensions.ORCHESTRATION_BINDING_TYPE_NAME_SUFFIX) {
				TypeAttributes = TypeAttributes.NotPublic,
				IsPartial = true,
				IsClass = true
			};
			@class.BaseTypes.Add(new CodeTypeReference(typeof(OrchestrationBindingBase<>).Name, new CodeTypeReference(type.Name)));
			@class.AddGeneratedCodeAttribute();
			@namespace.Types.Add(@class);
			return @class;
		}

		internal static CodeTypeDeclaration AddBindingClass(this CodeNamespace @namespace, Type type, CodeTypeDeclaration bindingConfigurationInterface)
		{
			if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (!typeof(BTXService).IsAssignableFrom(type)) throw new ArgumentException($"{type.FullName} is not an orchestration type.", nameof(type));
			if (bindingConfigurationInterface == null) throw new ArgumentNullException(nameof(bindingConfigurationInterface));
			if (!bindingConfigurationInterface.IsInterface)
				throw new ArgumentException($"{bindingConfigurationInterface.GetType().FullName} is not an interface.", nameof(bindingConfigurationInterface));

			var @class = @namespace.AddBindingClass(type);
			@class.BaseTypes.Add(new CodeTypeReference(bindingConfigurationInterface.Name));
			return @class;
		}
	}
}
