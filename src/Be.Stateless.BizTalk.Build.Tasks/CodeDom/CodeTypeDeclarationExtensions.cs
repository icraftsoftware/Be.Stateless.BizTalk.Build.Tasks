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
using System.Reflection;

namespace Be.Stateless.BizTalk.CodeDom
{
	internal static class CodeTypeDeclarationExtensions
	{
		internal static void AddGeneratedCodeAttribute(this CodeTypeDeclaration type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			var assemblyName = Assembly.GetExecutingAssembly().GetName();
			var generatedCodeAttribute = new CodeAttributeDeclaration(
				new CodeTypeReference(nameof(GeneratedCodeAttribute)),
				new CodeAttributeArgument(new CodePrimitiveExpression(assemblyName.Name)),
				new CodeAttributeArgument(new CodePrimitiveExpression(assemblyName.Version.ToString())));
			type.CustomAttributes.Add(generatedCodeAttribute);
		}
	}
}
