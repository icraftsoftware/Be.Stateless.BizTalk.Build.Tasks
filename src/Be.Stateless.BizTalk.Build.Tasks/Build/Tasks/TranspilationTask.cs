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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Be.Stateless.Extensions;
using Be.Stateless.IO.Extensions;
using Be.Stateless.Linq;
using Be.Stateless.Linq.Extensions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	public abstract class TranspilationTask : BizTalkAssemblyResolvingTask
	{
		#region Base Class Member Overrides

		protected override void ExecuteCore()
		{
			Cleanup();
			Transpile();
		}

		#endregion

		[Required]
		public string RootNamespace { get; set; }

		[Required]
		public string RootPath { get; set; }

		protected abstract string FallBackRootPath { get; }

		protected abstract Type[] InputTypes { get; }

		protected abstract string OutputFileExtension { get; }

		protected HashSet<ITaskItem> OutputTaskItems { get; } = new(new LambdaComparer<ITaskItem>((lti, rti) => lti.ItemSpec.Equals(rti.ItemSpec, StringComparison.InvariantCultureIgnoreCase)));

		internal void Transpile()
		{
			foreach (var type in InputTypes)
			{
				var taskItem = CreateOutputTaskItem(type);
				Transpile(type, taskItem);
				if (!OutputTaskItems.Add(taskItem)) throw new InvalidOperationException($"'{taskItem.ItemSpec}' output task item has been defined multiple times.");
				Log.LogMessage(MessageImportance.Low, $"Adding output task item to output item group '{taskItem.ItemSpec}'.");
			}
		}

		protected abstract void Transpile(Type type, ITaskItem outputTaskItem);

		internal ITaskItem CreateOutputTaskItem(Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			var commonNamespacePrefix = new[] { RootNamespace, type.Namespace }.GetCommonPath('.');
			var outputDirectory = commonNamespacePrefix.IsNullOrEmpty()
				? Path.Combine(FallBackRootPath, type.Namespace!.Replace('.', Path.DirectorySeparatorChar))
				: Path.Combine(RootPath, type.Namespace!.Substring(commonNamespacePrefix.Length + 1).Replace('.', Path.DirectorySeparatorChar));
			var outputFilePath = Path.Combine(outputDirectory, $"{type.Name}{OutputFileExtension}");
			return new TaskItem(outputFilePath);
		}

		private void Cleanup()
		{
			Log.LogMessage(MessageImportance.Normal, "Cleaning up previous transpilation outputs.");
			Directory.EnumerateFiles(RootPath, $"*{OutputFileExtension}", SearchOption.AllDirectories)
				.ForEach(
					filePath => {
						Log.LogMessage(MessageImportance.Low, $"Deleting file {filePath}.");
						File.Delete(filePath);
						CleanFolder(Path.GetDirectoryName(filePath));
					});
		}

		private void CleanFolder(string directory)
		{
			while (!string.Equals(directory, RootPath, StringComparison.OrdinalIgnoreCase) && !Directory.EnumerateFileSystemEntries(directory!).Any())
			{
				Log.LogMessage(MessageImportance.Low, $"Deleting directory {directory}.");
				Directory.Delete(directory);
				directory = Path.GetDirectoryName(directory);
			}
		}
	}
}
