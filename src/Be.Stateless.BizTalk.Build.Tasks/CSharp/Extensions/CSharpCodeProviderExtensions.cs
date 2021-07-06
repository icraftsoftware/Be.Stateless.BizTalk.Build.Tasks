#region Copyright & License

// Copyright © 2012 - 2021 François Chabot
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
using System.IO;
using Be.Stateless.Extensions;
using Microsoft.CSharp;

namespace Be.Stateless.BizTalk.CSharp.Extensions
{
	public static class CSharpCodeProviderExtensions
	{
		public static void GenerateAndSaveCodeFromCompileUnit(this CSharpCodeProvider codeProvider, CodeCompileUnit codeCompileUnit, string path)
		{
			if (codeProvider == null) throw new ArgumentNullException(nameof(codeProvider));
			if (codeCompileUnit == null) throw new ArgumentNullException(nameof(codeCompileUnit));
			if (path.IsNullOrEmpty()) throw new ArgumentNullException(nameof(path));

			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			using (var writer = new StreamWriter(path))
			{
				codeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, writer, new() { BracingStyle = "C", IndentString = "\t", VerbatimOrder = true });
			}
		}
	}
}
