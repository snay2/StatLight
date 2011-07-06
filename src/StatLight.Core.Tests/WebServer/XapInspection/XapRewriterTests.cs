using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Ionic.Zip;
using NUnit.Framework;
using StatLight.Core.WebServer.XapInspection;

namespace StatLight.Core.Tests.WebServer.XapInspection
{
    [TestFixture]
    public class XapRewriterTests : FixtureBase
    {
        private XapRewriter _xapRewriter;
        private ZipFile _originalXapHost;
        private ZipFile _originalXapUnderTest;
        private ZipFile _expectedXapHost;
        private string _expectedAppManifest;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            _xapRewriter = new XapRewriter(base.TestLogger);


            var appManifest = @"<Deployment xmlns=""http://schemas.microsoft.com/client/2007/deployment"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" EntryPointAssembly=""StatLight.Client.Harness"" EntryPointType=""StatLight.Client.Harness.App"" RuntimeVersion=""4.0.50826.0"">
  <Deployment.Parts>
    <AssemblyPart x:Name=""StatLight.Client.Harness"" Source=""StatLight.Client.Harness.dll"" />
    <AssemblyPart x:Name=""System.Windows.Controls"" Source=""System.Windows.Controls.dll"" />
    <AssemblyPart x:Name=""System.Xml.Linq"" Source=""System.Xml.Linq.dll"" />
    <AssemblyPart x:Name=""System.Xml.Serialization"" Source=""System.Xml.Serialization.dll"" />
    <AssemblyPart x:Name=""Microsoft.Silverlight.Testing"" Source=""Microsoft.Silverlight.Testing.dll"" />
    <AssemblyPart x:Name=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight"" Source=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll"" />
  </Deployment.Parts>
</Deployment>
";

            _originalXapHost = new ZipFile();
            _originalXapHost.AddTemporaryFile("AppManifest.xaml", fileBytes: appManifest.ToByteArray());
            _originalXapHost.AddTemporaryFile("StatLight.Client.Harness.dll");
            _originalXapHost.AddTemporaryFile("StatLight.Client.Harness.MSTest.dll");
            _originalXapHost.AddTemporaryFile("System.Windows.Controls.dll");
            _originalXapHost.AddTemporaryFile("System.Xml.Linq.dll");
            _originalXapHost.AddTemporaryFile("System.Xml.Serialization.dll");
            _originalXapHost.AddTemporaryFile("Microsoft.Silverlight.Testing.dll");
            _originalXapHost.AddTemporaryFile("Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll");


            var appManifest2 = @"<Deployment xmlns=""http://schemas.microsoft.com/client/2007/deployment"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" EntryPointAssembly=""StatLight.IntegrationTests.Silverlight.MSTest"" EntryPointType=""StatLight.IntegrationTests.Silverlight.App"" RuntimeVersion=""4.0.50826.0"">
  <Deployment.Parts>
    <AssemblyPart x:Name=""StatLight.IntegrationTests.Silverlight.MSTest"" Source=""StatLight.IntegrationTests.Silverlight.MSTest.dll"" />
    <AssemblyPart x:Name=""Microsoft.Silverlight.Testing"" Source=""Microsoft.Silverlight.Testing.dll"" />
    <AssemblyPart x:Name=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight"" Source=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll"" />
  </Deployment.Parts>
</Deployment>
";

            _originalXapUnderTest = new ZipFile();
            _originalXapUnderTest.AddTemporaryFile("AppManifest.xaml", fileBytes: appManifest2.ToByteArray());
            _originalXapUnderTest.AddTemporaryFile("StatLight.IntegrationTests.Silverlight.MSTest.dll");
            _originalXapUnderTest.AddTemporaryFile("Microsoft.Silverlight.Testing.dll");
            _originalXapUnderTest.AddTemporaryFile("Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll");
            _originalXapUnderTest.AddTemporaryFile("Test.xml", "Test/Test", "Hello".ToByteArray());

            // This was a crazy case that the Sterling db project exposed to StatLight (They had duplicate test assemblies)
            _originalXapUnderTest.AddEntry("Microsoft.Silverlight.Testing.dll", "/Binaries/", new byte[] { 1, 2 });
            _originalXapUnderTest.AddEntry("Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll", "/Binaries/", new byte[] { 1, 2 });


            _expectedAppManifest = @"<Deployment xmlns=""http://schemas.microsoft.com/client/2007/deployment"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" EntryPointAssembly=""StatLight.Client.Harness"" EntryPointType=""StatLight.Client.Harness.App"" RuntimeVersion=""4.0.50826.0"">
  <Deployment.Parts>
    <AssemblyPart x:Name=""StatLight.Client.Harness"" Source=""StatLight.Client.Harness.dll"" />
    <AssemblyPart x:Name=""System.Windows.Controls"" Source=""System.Windows.Controls.dll"" />
    <AssemblyPart x:Name=""System.Xml.Linq"" Source=""System.Xml.Linq.dll"" />
    <AssemblyPart x:Name=""System.Xml.Serialization"" Source=""System.Xml.Serialization.dll"" />
    <AssemblyPart x:Name=""Microsoft.Silverlight.Testing"" Source=""Microsoft.Silverlight.Testing.dll"" />
    <AssemblyPart x:Name=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight"" Source=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll"" />
    <AssemblyPart x:Name=""StatLight.IntegrationTests.Silverlight.MSTest"" Source=""StatLight.IntegrationTests.Silverlight.MSTest.dll"" />
  </Deployment.Parts>
</Deployment>
";
            _expectedXapHost = new ZipFile();
            _expectedXapHost.AddTemporaryFile("StatLight.Client.Harness.dll");
            _expectedXapHost.AddTemporaryFile("StatLight.Client.Harness.MSTest.dll");
            _expectedXapHost.AddTemporaryFile("System.Windows.Controls.dll");
            _expectedXapHost.AddTemporaryFile("System.Xml.Linq.dll");
            _expectedXapHost.AddTemporaryFile("System.Xml.Serialization.dll");
            _expectedXapHost.AddTemporaryFile("Microsoft.Silverlight.Testing.dll");
            _expectedXapHost.AddTemporaryFile("Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll");
            _expectedXapHost.AddTemporaryFile("StatLight.IntegrationTests.Silverlight.MSTest.dll");
            _expectedXapHost.AddTemporaryFile("Test.xml", "Test/Test", "Hello".ToByteArray());
            _expectedXapHost.AddTemporaryFile("AppManifest.xaml", fileBytes: _expectedAppManifest.ToByteArray());

            // This was a crazy case that the Sterling db project exposed to StatLight (They had duplicate test assemblies)
            _expectedXapHost.AddTemporaryFile("Microsoft.Silverlight.Testing.dll", "/Binaries/");
            _expectedXapHost.AddTemporaryFile("Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll", "/Binaries/");
        }

        [Test]
        public void Should_serialize_zip_bytes_correctly()
        {
            _originalXapHost.ToByteArray().ShouldEqual(_originalXapHost.ToByteArray());
        }

        [Test]
        public void Should_rewrite_correctly()
        {
            var newFiles = new List<ITestFile>
                               {
                                   new TempTestFile("StatLight.IntegrationTests.Silverlight.MSTest.dll"),
                                   new TempTestFile("Test/Test/Test.xml"),
                                   new TempTestFile("Binaries/Microsoft.Silverlight.Testing.dll"),
                                   new TempTestFile("Binaries/Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll"),
                               };

            ZipFile actualXapHost = _xapRewriter.RewriteZipHostWithFiles(_originalXapHost.ToByteArray(), newFiles, null);
            string actualXapHostFileName = Path.GetTempFileName();
            actualXapHost.Save(actualXapHostFileName);


            //var expectedAM = XElement.Load(_expectedAppManifest);
            //var actualAM = XElement.Load(actualXapHost["AppManifest.xaml"].OpenReader());
            //expectedAM.ShouldEqual(actualAM);

            AssertZipsEqual(_expectedXapHost, actualXapHost);

        }

        private static void AssertZipsEqual(ZipFile expected, ZipFile actual)
        {
            actual.Count.ShouldEqual(expected.Count, "zip files contain different counts");

            var allFiles = actual.Select(s => s.FileName).ToArray();

            foreach (var expectedFile in expected)
            {
                string firstOrDefault = allFiles.Where(w => w.Equals(expectedFile.FileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                firstOrDefault.ShouldNotBeNull("Could not find file {0} in files \n{1}".FormatWith(
                    expectedFile.FileName,
                    string.Join(Environment.NewLine, allFiles)));
                //allFiles.Contains(expectedFile.FileName).ShouldBeTrue();
            }
            //for (int i = 0; i < expected.Count; i++)
            //{
            //    var expectedFile = expected[i];
            //    var actualFile = actual[i];
            //
            //    actualFile.FileName.ShouldEqual(expectedFile.FileName);
            //    //actualFile.ToByteArray().ShouldEqual(expectedFile.ToByteArray(), "File [{0}] bytes not same.".FormatWith(actualFile.FileName));
            //}

        }

        private class TempTestFile : ITestFile
        {
            private readonly string _fileName;

            public TempTestFile(string fileName)
            {
                _fileName = fileName;
            }

            public string FileName
            {
                get { return _fileName; }
            }

            public byte[] File
            {
                get { return new byte[] { 1, 2, 3 }; }
            }
        }
    }

    static class Extensions
    {
        public static ZipFile AddTemporaryFile(this ZipFile zipFile, string fileName, string directoryName = "/", byte[] fileBytes = null)
        {
            if (fileBytes == null)
                fileBytes = new byte[] { 1, 2 };

            zipFile.AddEntry(fileName, directoryName, fileBytes);
            return zipFile;
        }
    }
}