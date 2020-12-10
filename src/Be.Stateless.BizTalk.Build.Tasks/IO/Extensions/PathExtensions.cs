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
using System.IO;

namespace Be.Stateless.BizTalk.IO.Extensions
{
	internal static class PathExtensions
	{
		internal static bool Equals(this string path, string otherPath)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));
			if (otherPath == null) throw new ArgumentNullException(nameof(otherPath));
			// see https://stackoverflow.com/a/22873389/1789441
			return new Uri(path.TrimTrailingDirectorySeparator()) == new Uri(otherPath.TrimTrailingDirectorySeparator());
		}

		private static string TrimTrailingDirectorySeparator(this string path)
		{
			return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}
	}
}
