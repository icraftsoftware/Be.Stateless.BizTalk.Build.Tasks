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
using System.Diagnostics.CodeAnalysis;
using Be.Stateless.BizTalk.Dummies;
using FluentAssertions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using Xunit;
using static FluentAssertions.FluentActions;

namespace Be.Stateless.BizTalk.Build.Tasks
{
	public class TranspilationTaskFixture
	{
		[Theory]
		// @formatter:off
		[InlineData(@"Be.Stateless.BizTalk", typeof(Be.Stateless.BizTalk.Orchestrations.Bound.Process), @"C:\Project\Orchestrations\Bound\ProcessOrchestrationBinding.Designer.cs")]
		[InlineData(@"Be.Stateless.BizTalk", typeof(Be.Stateless.BizTalk.Orchestrations.Direct.Process), @"C:\Project\Orchestrations\Direct\ProcessOrchestrationBinding.Designer.cs")]
		[InlineData(@"Be.Stateless.Health", typeof(Be.Stateless.BizTalk.Orchestrations.Bound.Process), @"C:\Project\BizTalk\Orchestrations\Bound\ProcessOrchestrationBinding.Designer.cs")]
		[InlineData(@"Org.Anization.Health", typeof(Be.Stateless.BizTalk.Orchestrations.Bound.Process), @"C:\Project\GeneratedOrchestrationBindings\Be\Stateless\BizTalk\Orchestrations\Bound\ProcessOrchestrationBinding.Designer.cs")]
		// @formatter:on
		[SuppressMessage("ReSharper", "RedundantNameQualifier")]
		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		public void ComputeTranspilationOutputDirectory(string rootNamespace, Type orchestrationType, string outputDirectory)
		{
			var sut = new TranspilationTaskSpy(@"C:\Project\GeneratedOrchestrationBindings", "OrchestrationBinding.Designer.cs") {
				RootNamespace = rootNamespace,
				RootPath = @"C:\Project"
			};
			sut.CreateOutputTaskItem(orchestrationType)
				.Should().BeEquivalentTo(new TaskItem(outputDirectory));
		}

		[Fact]
		public void TranspileThrowsWhenAddingDuplicateOutputTaskItem()
		{
			var sut = new TranspilationTaskSpy(
				@"C:\Project",
				".Designer.cs",
				new[] { typeof(Orchestrations.Bound.Process), typeof(Orchestrations.Bound.Process) }) {
				RootNamespace = @"Be.Stateless.BizTalk",
				RootPath = @"C:\Project",
				BuildEngine = new Mock<IBuildEngine>().Object
			};
			Invoking(() => sut.Transpile())
				.Should().Throw<InvalidOperationException>()
				.WithMessage(@"'C:\Project\Orchestrations\Bound\Process.Designer.cs' output task item has been defined multiple times.");
		}
	}
}
