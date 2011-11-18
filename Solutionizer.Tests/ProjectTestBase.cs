﻿using System;
using System.IO;
using NUnit.Framework;

namespace Solutionizer.Tests {
    public class ProjectTestBase {
        protected string _testDataPath;

        [SetUp]
        public void SetUp() {
            _testDataPath = Path.Combine(Path.GetTempPath(), "Solutionizer-" + DateTime.Now.ToString("o").Replace(':', '-'));
            Directory.CreateDirectory(_testDataPath);
        } 

        [TearDown]
        public void TearDown() {
            Directory.Delete(_testDataPath, true);
        }

        protected void CreateProjects() {
            var p1 = Path.Combine(_testDataPath, "Project1");
            Directory.CreateDirectory(p1);
            CopyTestDataToPath("CsTestProject1.csproj", p1);

            var p2 = Path.Combine(_testDataPath, "Project2");
            Directory.CreateDirectory(p2);
            CopyTestDataToPath("CsTestProject2.csproj", p2);
        }

        private static void CopyTestDataToPath(string resource, string path) {
            Directory.CreateDirectory(path);
            var stream = typeof(ProjectTestBase).Assembly.GetManifestResourceStream("Solutionizer.Tests.TestData." + resource);
            using (var input = new StreamReader(stream))
            using (var output = File.CreateText(Path.Combine(path, resource))) {
                var content = input.ReadToEnd();
                output.Write(content);
            }
        }
    }
}