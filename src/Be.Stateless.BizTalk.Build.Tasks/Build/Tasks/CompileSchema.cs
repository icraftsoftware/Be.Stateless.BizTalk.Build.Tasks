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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.BizTalk.Studio.Extensibility;
using Microsoft.BizTalk.TOM;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.BizTalkProject.Base;
using Microsoft.VisualStudio.BizTalkProject.BuildTasks;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public class CompileSchema : BizTalkBaseTask
	{
		#region Nested Type: SchemaBuildSnapshot

		private class SchemaBuildSnapshot : BizTalkBuildSnapshot
		{
			public SchemaBuildSnapshot(object serviceProvider, List<string> projectReferences, List<SchemaBuildFileInfo> schemaFilesToCompile) : base(serviceProvider)
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
			var schemaBuildSnapshot = new SchemaBuildSnapshot(
				GetServiceProvider(),
				ProjectReferences.Select(projectReference => projectReference.GetMetadata("FullPath")).ToList(),
				SchemaItems.Select(schemaItem => SchemaBuildFileInfo.GetSchemaFileInfo(schemaItem, RootNamespace)).ToList());
			schemaBuildSnapshot.ProjectConfigProperties[DictionaryTags.EnableUnitTesting] = EnableUnitTesting;
			schemaBuildSnapshot.ProjectConfigProperties[DictionaryTags.TreatWarningsAsErrors] = TreatWarningAsError;
			schemaBuildSnapshot.ProjectConfigProperties[DictionaryTags.WarningLevel] = WarningLevel;
			return !LogErrors(Compile(schemaBuildSnapshot, schemaBuildSnapshot.SchemaFilesToCompile, out _));
		}

		#endregion

		[Required]
		public bool EnableUnitTesting { get; set; }

		[Required]
		public ITaskItem[] ProjectReferences { get; set; }

		[Required]
		public string RootNamespace { get; set; }

		[Required]
		public ITaskItem[] SchemaItems { get; set; }

		public bool TreatWarningAsError { get; set; }

		[Required]
		public int WarningLevel { get; set; }

		[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
		private BizTalkErrorCollection Compile(IBizTalkBuildSnapShot buildSnapshot, IEnumerable<SchemaFileInfo> schemaFilesToCompile, out List<FileInfo> generatedCodeFiles)
		{
			var errors = new EditorCompiler().Compile(buildSnapshot, new SchemaCompileStatusCallback(), buildSnapshot.GetCompilableFiles(), null, out var pvOutBlobs);
			var errorCollection = new BizTalkErrorCollection();
			ProjectHelper.PopulateErrorCollection(errorCollection, errors);
			var fileContents = (string[]) pvOutBlobs;
			generatedCodeFiles = errorCollection.HasErrors
				? Enumerable.Empty<FileInfo>().ToList()
				: schemaFilesToCompile
					.Select(schemaFileInfo => Path.Combine(Path.GetDirectoryName(schemaFileInfo.FilePath), Path.GetFileName(schemaFileInfo.FilePath) + ".cs"))
					.Select((fullName, i) => ProjectHelper.GenerateFile(fileContents[i], fullName))
					.ToList();
			return errorCollection;
		}
	}
}
