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

using System.Linq;
using System.Reflection;
using Be.Stateless.BizTalk.Reflection.Extensions;
using Be.Stateless.Linq.Extensions;
using FluentAssertions;
using Xunit;

namespace Be.Stateless.BizTalk.CodeDom.Binding
{
	public class BizTalkAssemblyExtensionsFixture
	{
		[Fact]
		public void GetOrchestrationTypes()
		{
			var orchestrationTypes = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
				.Distinct((a1, a2) => a1.FullName == a2.FullName).Select(Assembly.Load)
				.Where(a => a.IsBizTalkAssembly())
				.SelectMany(a => a.GetOrchestrationTypes());

			orchestrationTypes.Should().BeEquivalentTo(new[] { typeof(Orchestrations.Bound.Process), typeof(Orchestrations.Direct.Process) });
		}

		[Fact]
		public void IsBizTalkAssembly()
		{
			Assembly.GetExecutingAssembly().IsBizTalkAssembly().Should().BeFalse();
		}
	}
}
