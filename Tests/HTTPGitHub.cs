/*
* Copyright © 2018-2023 EDDiscovery development team
*
* Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
* file except in compliance with the License. You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software distributed under
* the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
* ANY KIND, either express or implied. See the License for the specific language
* governing permissions and limitations under the License.
* 
* 
*/
using BaseUtils;
using NFluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(Eval))]
    public class HTTPGitHubTest
    {
        [Test]
        public void FolderTree()
        {
            GitHubClass ghc = new BaseUtils.GitHubClass("https://api.github.com/repos/EDDiscovery/EDDiscoveryData/", "EDDiscovery");
            //var files = ghc.ReadFolder(new System.Threading.CancellationToken(), "VideoFiles");
            var files2 = ghc.ReadFolderTree(new System.Threading.CancellationToken(), "master", "VideoFiles");
            Check.That(files2).IsNotNull();
            ghc.DownloadFiles(new System.Threading.CancellationToken(), @"c:\code\examples\gh1", files2, true, true);
        }
    }
}
