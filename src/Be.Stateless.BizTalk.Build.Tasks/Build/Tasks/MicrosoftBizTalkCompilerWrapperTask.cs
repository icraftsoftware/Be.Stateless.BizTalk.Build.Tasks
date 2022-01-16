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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Be.Stateless.Extensions;
using Be.Stateless.Linq.Extensions;
using Microsoft.BizTalk.Studio.Extensibility;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.BizTalkProject.Base;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	public abstract class MicrosoftBizTalkCompilerWrapperTask : Task
	{
		#region Nested Type: BuildSnapshot

		private class BuildSnapshot : BizTalkBuildSnapshot
		{
			public BuildSnapshot(List<string> projectReferences, List<BizTalkFileInfo> filesToCompile) : base(null)
			{
				_projectReferences = projectReferences;
				_filesToCompile = filesToCompile;
			}

			#region Base Class Member Overrides

			public override List<BizTalkFileInfo> GetCompilableProjectFiles() => _filesToCompile.ToList();

			public override List<string> GetProjectReferences() => _projectReferences;

			#endregion

			private readonly List<BizTalkFileInfo> _filesToCompile;
			private readonly List<string> _projectReferences;
		}

		#endregion

		#region Nested Type: CompilationStatusCallback

		private class CompilationStatusCallback : IBtsCompileStatusCallback
		{
			#region IBtsCompileStatusCallback Members

			public bool ReportProgress(uint itemsLeft, uint itemsTotal) => true;

			#endregion
		}

		#endregion

		#region Base Class Member Overrides

		public override bool Execute()
		{
			try
			{
				var buildSnapshot = new BuildSnapshot(ProjectReferences.Select(projectReference => projectReference.GetMetadata("FullPath")).ToList(), FilesToCompile) {
					ProjectConfigProperties = {
						[DictionaryTags.EnableUnitTesting] = false,
						[DictionaryTags.TreatWarningsAsErrors] = false,
						[DictionaryTags.WarningLevel] = WarningLevel
					}
				};
				return Compile(buildSnapshot);
			}
			catch (Exception exception)
			{
				if (exception.IsFatal()) throw;
				Log.LogErrorFromException(exception, true, true, null);
				return false;
			}
		}

		#endregion

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "MSBuild Task API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MSBuild Task API.")]
		[Required]
		public ITaskItem[] ProjectReferences { get; set; }

		[SuppressMessage("ReSharper", "MemberCanBeProtected.Global", Justification = "MSBuild Task API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MSBuild Task API.")]
		[Required]
		public string RootNamespace { get; set; }

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "MSBuild Task API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MSBuild Task API.")]
		[Required]
		public int WarningLevel { get; set; }

		protected abstract IBizTalkCompiler BizTalkCompiler { get; }

		protected abstract List<BizTalkFileInfo> FilesToCompile { get; }

		protected abstract ITaskItem[] OutputItemGroup { set; }

		private bool Compile(BizTalkBuildSnapshot buildSnapshot)
		{
			var errors = BizTalkCompiler.Compile(
				buildSnapshot,
				new CompilationStatusCallback(),
				buildSnapshot.GetCompilableFiles(),
				null,
				out var pvOutBlobs);

			var bizTalkErrorCollection = new BizTalkErrorCollection(errors);
			if (bizTalkErrorCollection.HasErrors)
			{
				// see Microsoft.VisualStudio.BizTalkProject.BuildTasks.BizTalkBaseTask.ReportBizTalkErrors
				bizTalkErrorCollection.Where(be => be.get_Type() != BtsErrorType.InformationalToTaskList).ForEach(LogError);
				return false;
			}
			var fileContents = (string[]) pvOutBlobs;
			OutputItemGroup = buildSnapshot.GetCompilableProjectFiles()
				.Select(fileInfo => $"{fileInfo.FilePath}.cs")
				.Select(
					(outputPath, i) => {
						Log.LogMessage(MessageImportance.Normal, $"Generating C# code file '{outputPath}'.");
						File.WriteAllText(outputPath, fileContents[i]);
						return outputPath;
					})
				.Select(
					outputPath => {
						Log.LogMessage(MessageImportance.Low, $"Adding C# code file '{outputPath}' to output item group.");
						return (ITaskItem) new TaskItem(outputPath);
					})
				.ToArray();
			return true;
		}

		[SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
		private void LogError(IBizTalkError error)
		{
			static T TryGetValue<T>(Func<T> function)
			{
				try
				{
					return function();
				}
				catch (Exception exception)
				{
					if (exception.IsFatal()) throw;
					return default;
				}
			}

			var text = error.get_Text();
			var document = error.get_Document();
			var errorNumberText = error.get_ErrorNumberText();
			var lineNumber = TryGetValue(() => (int) error.get_LineNumber());
			var columnNumber = TryGetValue(() => (int) error.get_ColumnNumber());
			switch (error.get_Type())
			{
				case BtsErrorType.FatalError:
				case BtsErrorType.Error:
					Log.LogError(string.Empty, errorNumberText, string.Empty, document, lineNumber, columnNumber, 0, 0, text);
					break;
				case BtsErrorType.Warning:
					Log.LogWarning(string.Empty, errorNumberText, string.Empty, document, lineNumber, columnNumber, 0, 0, text);
					break;
				default:
					Log.LogMessage(ProjectHelper.GetErrorMessage(error, true));
					break;
			}
		}
	}
}
