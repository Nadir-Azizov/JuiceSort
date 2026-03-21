using System;
using NUnit.Framework;
using JuiceSort.Core;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class ServicesTests
    {
        private interface ITestService
        {
            string Name { get; }
        }

        private class TestService : ITestService
        {
            public string Name { get; }

            public TestService(string name)
            {
                Name = name;
            }
        }

        [SetUp]
        public void SetUp()
        {
            Services.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            Services.Clear();
        }

        [Test]
        public void Register_AndGet_ReturnsRegisteredService()
        {
            var service = new TestService("test");
            Services.Register<ITestService>(service);

            var retrieved = Services.Get<ITestService>();
            Assert.AreEqual("test", retrieved.Name);
        }

        [Test]
        public void Get_UnregisteredService_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Services.Get<ITestService>());
        }

        [Test]
        public void TryGet_RegisteredService_ReturnsTrueAndService()
        {
            var service = new TestService("test");
            Services.Register<ITestService>(service);

            var found = Services.TryGet<ITestService>(out var retrieved);
            Assert.IsTrue(found);
            Assert.AreEqual("test", retrieved.Name);
        }

        [Test]
        public void TryGet_UnregisteredService_ReturnsFalseAndNull()
        {
            var found = Services.TryGet<ITestService>(out var retrieved);
            Assert.IsFalse(found);
            Assert.IsNull(retrieved);
        }

        [Test]
        public void Clear_RemovesAllServices()
        {
            Services.Register<ITestService>(new TestService("test"));
            Services.Clear();

            Assert.Throws<InvalidOperationException>(() => Services.Get<ITestService>());
        }

        [Test]
        public void Register_OverwritesPreviousRegistration()
        {
            Services.Register<ITestService>(new TestService("first"));
            Services.Register<ITestService>(new TestService("second"));

            var retrieved = Services.Get<ITestService>();
            Assert.AreEqual("second", retrieved.Name);
        }
    }
}
