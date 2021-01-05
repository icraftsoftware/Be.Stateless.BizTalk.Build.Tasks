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
using Be.Stateless.BizTalk.Build.Tasks;
using Microsoft.Build.Framework;

namespace Be.Stateless.BizTalk.Dummies
{
	public class TranspilationTaskSpy : TranspilationTask
	{
		public TranspilationTaskSpy(string fallBackRootPath, string outputFileExtension, Type[] inputTypes = null)
		{
			FallBackRootPath = fallBackRootPath;
			OutputFileExtension = outputFileExtension;
			InputTypes = inputTypes;
		}

		#region Base Class Member Overrides

		protected override string FallBackRootPath { get; }

		protected override Type[] InputTypes { get; }

		protected override string OutputFileExtension { get; }

		protected override void Transpile(Type type, ITaskItem outputTaskItem) { }

		#endregion

		public new ITaskItem CreateOutputTaskItem(Type type)
		{
			return base.CreateOutputTaskItem(type);
		}
	}
}
