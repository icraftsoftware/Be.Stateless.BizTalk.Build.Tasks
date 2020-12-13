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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Be.Stateless.Extensions;
using Be.Stateless.Linq.Extensions;
using Microsoft.BizTalk.Studio.Extensibility;
using Microsoft.BizTalk.TOM;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.BizTalkProject.Base;
using Microsoft.VisualStudio.BizTalkProject.BuildTasks;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "MSBuild Task.")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "MSBuild Task.")]
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "MSBuild Task.")]
	public class GenerateCSharpSchema : Task
	{
		#region Nested Type: SchemaBuildSnapshot

		private class SchemaBuildSnapshot : BizTalkBuildSnapshot
		{
			public SchemaBuildSnapshot(List<string> projectReferences, List<SchemaBuildFileInfo> schemaFilesToCompile) : base(null)
			{
				_projectReferences = projectReferences;
				_schemaFilesToCompile = schemaFilesToCompile;
			}

			#region Base Class Member Overrides

			public override List<BizTalkFileInfo> GetCompilableProjectFiles() => SchemaFilesToCompile.Cast<BizTalkFileInfo>().ToList();

			public override List<string> GetProjectReferences() => _projectReferences;

			#endregion

			public IEnumerable<SchemaFileInfo> SchemaFilesToCompile => _schemaFilesToCompile;

			private readonly List<string> _projectReferences;
			private readonly List<SchemaBuildFileInfo> _schemaFilesToCompile;
		}

		#endregion

		#region Nested Type: SchemaCompileStatusCallback

		private class SchemaCompileStatusCallback : IBtsCompileStatusCallback
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
				// TODO add disabling warning support in *.*proj files for envelope where not all root is a body xpath
				var schemaBuildSnapshot = new SchemaBuildSnapshot(
					ProjectReferences.Select(projectReference => projectReference.GetMetadata("FullPath")).ToList(),
					XmlSchemas.Select(schemaItem => SchemaBuildFileInfo.GetSchemaFileInfo(schemaItem, RootNamespace)).ToList());
				schemaBuildSnapshot.ProjectConfigProperties[DictionaryTags.EnableUnitTesting] = false;
				schemaBuildSnapshot.ProjectConfigProperties[DictionaryTags.TreatWarningsAsErrors] = false;
				schemaBuildSnapshot.ProjectConfigProperties[DictionaryTags.WarningLevel] = WarningLevel;
				return Compile(schemaBuildSnapshot);
			}
			catch (Exception exception)
			{
				if (exception.IsFatal()) throw;
				Log.LogErrorFromException(exception, true, true, null);
				return false;
			}
		}

		#endregion

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		[Output]
		public ITaskItem[] CSharpSchemas { get; private set; }

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		[Required]
		public ITaskItem[] ProjectReferences { get; set; }

		[Required]
		public string RootNamespace { get; set; }

		[Required]
		public int WarningLevel { get; set; }

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		[Required]
		public ITaskItem[] XmlSchemas { get; set; }

		private bool Compile(SchemaBuildSnapshot schemaBuildSnapshot)
		{
			var errors = new EditorCompiler().Compile(
				schemaBuildSnapshot,
				new SchemaCompileStatusCallback(),
				schemaBuildSnapshot.GetCompilableFiles(),
				null,
				out var pvOutBlobs);

			var bizTalkErrorCollection = new BizTalkErrorCollection(errors);
			if (bizTalkErrorCollection.HasErrors)
			{
				// see Microsoft.VisualStudio.BizTalkProject.BuildTasks.BizTalkBaseTask.ReportBizTalkErrors
				bizTalkErrorCollection.Where(be => be.get_Type() != BtsErrorType.InformationalToTaskList).ForEach(LogError);
				return false;
			}
			var outputs = new List<ITaskItem>();
			var fileContents = (string[]) pvOutBlobs;
			schemaBuildSnapshot.SchemaFilesToCompile
				.Select(schemaFileInfo => $"{schemaFileInfo.FilePath}.cs")
				.Select(
					(outputPath, i) => {
						Log.LogMessage(MessageImportance.Normal, $"Generating schema C# code file '{outputPath}'.");
						File.WriteAllText(outputPath, fileContents[i]);
						return outputPath;
					})
				.ForEach(
					outputPath => {
						Log.LogMessage(MessageImportance.Low, $"Adding schema C# code file to output item {nameof(CSharpSchemas)} group {outputPath}");
						outputs.Add(new TaskItem(outputPath));
					});
			CSharpSchemas = outputs.ToArray();
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
