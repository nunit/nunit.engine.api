// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System.IO;
using System.Security.Cryptography.X509Certificates;
using NUnit.Common;
using NUnit.Framework;

namespace NUnit.Engine.Api
{
    [TestFixture("test.dll")]
    [TestFixture("test1.dll,test2.dll,test3.dll")]
    public class TestPackageTests
    {
        private string[] _fileNames;
        private TestPackage _package;

        public TestPackageTests(string fileNames)
        {
            _fileNames = fileNames.Split(',');
        }

        [SetUp]
        public void CreatePackage()
        {
            _package = new TestPackage(_fileNames);
        }

        [Test]
        public void PackageIDsAreUnique()
        {
            var another = new TestPackage(_fileNames);
            Assert.That(another.ID, Is.Not.EqualTo(_package.ID));
        }

        [Test]
        public void PackageIsAnonymous()
        {
            Assert.That(_package.Name, Is.Null);
            Assert.That(_package.FullName, Is.Null);
        }

        [Test]
        public void HasSubPackageForEachFile()
        {
            Assert.That(_package.SubPackages.Count, Is.EqualTo(_fileNames.Length));
            for (int i = 0; i < _fileNames.Length; i++)
            {
                TestPackage subPackage = _package.SubPackages[i];
                string fileName = _fileNames[i];

                Assert.That(subPackage.Name, Is.EqualTo(fileName));
                Assert.That(subPackage.FullName, Is.EqualTo(Path.GetFullPath(fileName)));
            }
        }

        [Test]
        public void SubPackagesHaveNoSubPackages()
        {
            foreach (TestPackage subPackage in _package.SubPackages)
                Assert.That(subPackage.SubPackages.Count, Is.EqualTo(0));
        }

        [Test]
        public void SubPackagesHaveNoSettingsInitially()
        {
            foreach (TestPackage subPackage in _package.SubPackages)
                Assert.That(subPackage.Settings.Count, Is.EqualTo(0));
        }

        private static readonly PackageSetting MAX_AGENTS_42 = SettingDefinitions.MaxAgents.WithValue(42);
        private static readonly PackageSetting MAX_AGENTS_99 = SettingDefinitions.MaxAgents.WithValue(99);

        [Test]
        public void AddStandardSettingToPackage([Values(false, true)] bool recursive)
        {
            _package.AddSetting(MAX_AGENTS_99, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting(MAX_AGENTS_99.Name), Is.EqualTo(99));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting(MAX_AGENTS_99.Name), Is.EqualTo(99));
            }
        }

        [TestCase("setting1", "value1", false)]
        [TestCase("setting1", "value1", true)]
        public void AddCustomStringSettingToPackage(string settingName, string settingValue, bool recursive)
        {
            _package.AddSetting(settingName, settingValue, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting(settingName), Is.EqualTo(settingValue));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting(settingName), Is.EqualTo(settingValue));
            }
        }

        [Test]
        public void AddCustomBooleanSettingToPackage([Values(false, true)] bool recursive)
        {
            _package.AddSetting("setting1", true, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting("setting1"), Is.EqualTo(true));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting("setting1"), Is.EqualTo(true));
            }
        }

        [Test]
        public void AddCustomIntSettingToPackage([Values(false, true)] bool recursive)
        {
            _package.AddSetting("setting1", 42, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting("setting1"), Is.EqualTo(42));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting("setting1"), Is.EqualTo(42));
            }
        }

        [Test]
        public void ApplyStandardSettingToPackage([Values(false, true)] bool recursive)
        {
            _package.ApplySetting(MAX_AGENTS_99, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting(MAX_AGENTS_99.Name), Is.EqualTo(99));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting(MAX_AGENTS_99.Name), Is.EqualTo(99));
            }

            _package.ApplySetting(MAX_AGENTS_42, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting(MAX_AGENTS_42.Name), Is.EqualTo(42));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting(MAX_AGENTS_42.Name), Is.EqualTo(42));
            }
        }

        [Test]
        public void ApplyCustomStringSettingToPackage([Values(false, true)] bool recursive)
        {
            _package.ApplySetting("setting1", "value1", recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting("setting1"), Is.EqualTo("value1"));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting("setting1"), Is.EqualTo("value1"));
            }

            _package.ApplySetting("setting1", "value2", recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting("setting1"), Is.EqualTo("value2"));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting("setting1"), Is.EqualTo("value2"));
            }
        }

        [Test]
        public void ApplyCustomBooleanSettingToPackage([Values(false, true)] bool recursive)
        {
            _package.ApplySetting("setting1", true, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting("setting1"), Is.EqualTo(true));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting("setting1"), Is.EqualTo(true));
            }

            _package.ApplySetting("setting1", false, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting("setting1"), Is.EqualTo(false));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting("setting1"), Is.EqualTo(false));
            }
        }

        [Test]
        public void ApplyCustomIntSettingToPackage([Values(false, true)] bool recursive)
        {
            _package.ApplySetting("setting1", 99, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting("setting1"), Is.EqualTo(99));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting("setting1"), Is.EqualTo(99));
            }

            _package.ApplySetting("setting1", 42, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            Assert.That(_package.Settings.GetSetting("setting1"), Is.EqualTo(42));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(recursive ? 1 : 0));
                if (recursive)
                    Assert.That(subPackage.Settings.GetSetting("setting1"), Is.EqualTo(42));
            }
        }

        [Test]
        public void RemoveStandardSettingFromPackage([Values(false, true)] bool recursive)
        {
            _package.AddSetting(MAX_AGENTS_99, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            _package.RemoveSetting(MAX_AGENTS_99.Name, recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(0));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void RemoveCustomSettingFromPackage([Values(false, true)] bool recursive)
        {
            _package.AddSetting("setting1", "value1", recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(1));
            _package.RemoveSetting("setting1", recursive);
            Assert.That(_package.Settings.Count, Is.EqualTo(0));
            foreach (TestPackage subPackage in _package.SubPackages)
            {
                Assert.That(subPackage.Settings.Count, Is.EqualTo(0));
            }
        }
    }
}
