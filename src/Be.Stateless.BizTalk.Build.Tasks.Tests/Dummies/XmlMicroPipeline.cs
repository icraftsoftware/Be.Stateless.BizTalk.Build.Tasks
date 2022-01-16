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

using System.Diagnostics.CodeAnalysis;
using Be.Stateless.BizTalk.Component;
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.BizTalk.Dsl.Pipeline;
using Be.Stateless.BizTalk.MicroComponent;
using Be.Stateless.BizTalk.Schema.Annotation;
using Microsoft.BizTalk.Component;

namespace Be.Stateless.BizTalk.Dummies
{
	internal class XmlMicroPipeline : SendPipeline
	{
		[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
		public XmlMicroPipeline()
		{
			Description = "XML receive micro pipeline.";
			Version = new(1, 0);
			Stages.PreAssemble
				.AddComponent(
					new FailedMessageRoutingEnablerComponent {
						SuppressRoutingFailureReport = false
					})
				.AddComponent(
					new MicroPipelineComponent {
						Enabled = true,
						Components = new[] {
							new ContextPropertyExtractor {
								Extractors = new[] {
									new XPathExtractor(BizTalkFactoryProperties.MapTypeName.QName, "/letter/*/from", ExtractionMode.Promote),
									new XPathExtractor(BizTalkFactoryProperties.MessageType.QName, "/letter/*/paragraph", ExtractionMode.Write)
								}
							}
						}
					});
			Stages.Assemble
				.AddComponent(new XmlAsmComp());
			Stages.Encode
				.AddComponent(new MicroPipelineComponent { Enabled = true });
		}
	}
}
