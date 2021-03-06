namespace Gu.State.Tests.Settings
{
    using System;

    using NUnit.Framework;

    public class PropertiesSettingsBuilderTests
    {
        [Test]
        public void AddPropertyThrowsOnNested()
        {
            var builder = new PropertiesSettingsBuilder();
            var exception = Assert.Throws<ArgumentException>(() => builder.IgnoreProperty<ChangeTrackerTypes.Level>(x => x.Next.Value));
            var expected = "property must be a property expression like foo => foo.Bar\r\n" +
                           "Nested properties are not allowed";
            Assert.AreEqual(expected, exception.Message);
        }
    }
}