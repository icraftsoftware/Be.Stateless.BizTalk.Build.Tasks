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
using Be.Stateless.Linq;
using FluentAssertions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;

namespace Be.Stateless.BizTalk.Build.Utilities
{
	public class TaskItemFixture
	{
		[Fact]
		public void AddDuplicateTaskItem()
		{
			var outputs = new HashSet<ITaskItem>(new LambdaComparer<ITaskItem>((lti, rti) => lti.ItemSpec.Equals(rti.ItemSpec, StringComparison.InvariantCultureIgnoreCase)));
			outputs.Add(new TaskItem(@"c:\project\item.cs")).Should().BeTrue();
			outputs.Add(new TaskItem(@"c:\project\item.cs")).Should().BeFalse();
		}
	}
}
