# Be.Stateless.BizTalk.Build.Tasks

##### Build Pipelines

[![][pipeline.mr.badge]][pipeline.mr]

[![][pipeline.ci.badge]][pipeline.ci]

##### Latest Release

[![][nuget.badge]][nuget]

[![][nuget.build.badge]][nuget.build]

[![][release.badge]][release]

##### Release Preview

[![][nuget.preview.badge]][nuget.preview]

[![][nuget.build.preview.badge]][nuget.build.preview]

##### Documentation

[![][doc.main.badge]][doc.main]

[![][doc.this.badge]][doc.this]

[![][help.badge]][help]

## Overview

Custom `MSBuild` tasks leveraging the artifacts written in either one of the `BizTalk.Factory`'s `DSL`s. The `Be.Stateless.BizTalk.Build.Tasks` assembly however provides only the `MSBuild` tasks, to actually integrate them into the `MSBuild` process, the developer has to reference the [BizTalk.Server.2020.Build][biztalk.server.2020.build] `NuGet` package, which tailors the `MSBuild` process to Microsoft BizTalk Server® 2020.

<!-- badges -->

[doc.main.badge]: https://img.shields.io/static/v1?label=BizTalk.Factory%20SDK&message=User's%20Guide&color=8CA1AF&logo=readthedocs
[doc.main]: https://www.stateless.be/ "BizTalk.Factory SDK User's Guide"
[doc.this.badge]: https://img.shields.io/static/v1?label=Be.Stateless.BizTalk.Build.Tasks&message=User's%20Guide&color=8CA1AF&logo=readthedocs
[doc.this]: https://www.stateless.be/BizTalk/Build/Tasks "Be.Stateless.BizTalk.Build.Tasks User's Guide"
[github.badge]: https://img.shields.io/static/v1?label=Repository&message=Be.Stateless.BizTalk.Build.Tasks&logo=github
[github]: https://github.com/icraftsoftware/Be.Stateless.BizTalk.Build.Tasks "Be.Stateless.BizTalk.Build.Tasks GitHub Repository"
[help.badge]: https://img.shields.io/static/v1?label=Be.Stateless.BizTalk.Build.Tasks&message=Developer%20Help&color=8CA1AF&logo=microsoftacademic
[help]: https://github.com/icraftsoftware/biztalk.factory.github.io/blob/master/Help/BizTalk/Build/Tasks/README.md "Be.Stateless.BizTalk.Build.Tasks Developer Help"
[nuget.badge]: https://img.shields.io/nuget/v/Be.Stateless.BizTalk.Build.Tasks.svg?label=Be.Stateless.BizTalk.Build.Tasks&style=flat&logo=nuget
[nuget]: https://www.nuget.org/packages/Be.Stateless.BizTalk.Build.Tasks "Be.Stateless.BizTalk.Build.Tasks NuGet Package"
[nuget.build.badge]: https://img.shields.io/nuget/v/BizTalk.Server.2020.Build.svg?label=BizTalk.Server.2020.Build&style=flat&logo=nuget
[nuget.build]: https://www.nuget.org/packages/BizTalk.Server.2020.Build
[nuget.preview.badge]: https://badge-factory.azurewebsites.net/package/icraftsoftware/be.stateless/BizTalk.Factory.Preview/Be.Stateless.BizTalk.Build.Tasks?logo=nuget
[nuget.preview]: https://dev.azure.com/icraftsoftware/be.stateless/_packaging?_a=package&feed=BizTalk.Factory.Preview&package=Be.Stateless.BizTalk.Build.Tasks&protocolType=NuGet "Be.Stateless.BizTalk.Build.Tasks Preview NuGet Package"
[nuget.build.preview.badge]: https://badge-factory.azurewebsites.net/package/icraftsoftware/be.stateless/BizTalk.Factory.Preview/BizTalk.Server.2020.Build?logo=nuget
[nuget.build.preview]: https://dev.azure.com/icraftsoftware/be.stateless/_packaging?_a=package&feed=BizTalk.Factory.Preview&package=BizTalk.Server.2020.Build&protocolType=NuGet "BizTalk.Server.2020.Build Preview NuGet Package"
[pipeline.ci.badge]: https://dev.azure.com/icraftsoftware/be.stateless/_apis/build/status/Be.Stateless.BizTalk.Build.Tasks%20Continuous%20Integration?branchName=master&label=Continuous%20Integration%20Build
[pipeline.ci]: https://dev.azure.com/icraftsoftware/be.stateless/_build/latest?definitionId=55&branchName=master "Be.Stateless.BizTalk.Build.Tasks Continuous Integration Build Pipeline"
[pipeline.mr.badge]: https://dev.azure.com/icraftsoftware/be.stateless/_apis/build/status/Be.Stateless.BizTalk.Build.Tasks%20Manual%20Release?branchName=master&label=Manual%20Release%20Build
[pipeline.mr]: https://dev.azure.com/icraftsoftware/be.stateless/_build/latest?definitionId=56&branchName=master "Be.Stateless.BizTalk.Build.Tasks Manual Release Build Pipeline"
[release.badge]: https://img.shields.io/github/v/release/icraftsoftware/Be.Stateless.BizTalk.Build.Tasks?label=Release&logo=github
[release]: https://github.com/icraftsoftware/Be.Stateless.BizTalk.Build.Tasks/releases/latest "Be.Stateless.BizTalk.Build.Tasks GitHub Release"

<!-- links -->

[biztalk.server.2020.build]: https://www.nuget.org/packages/BizTalk.Server.2020.Build
