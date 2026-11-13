namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests.Logic;

public sealed class SuspensionDurationTests
{
    [Test]
    public async Task ParseDuration_Valid_Hours_Returns_Expected_TimeSpan()
    {
        TimeSpan duration = SuspensionsController.ParseDuration("24H");

        await Assert.That(duration).IsEqualTo(TimeSpan.FromHours(24));
    }

    [Test]
    public async Task ParseDuration_Valid_Days_Returns_Expected_TimeSpan()
    {
        TimeSpan duration = SuspensionsController.ParseDuration("7D");

        await Assert.That(duration).IsEqualTo(TimeSpan.FromDays(7));
    }

    [Test]
    public async Task ParseDuration_Valid_Weeks_Returns_Expected_TimeSpan()
    {
        TimeSpan duration = SuspensionsController.ParseDuration("2W");

        await Assert.That(duration).IsEqualTo(TimeSpan.FromDays(14));
    }

    [Test]
    public async Task ParseDuration_Valid_Months_Returns_Expected_TimeSpan()
    {
        TimeSpan duration = SuspensionsController.ParseDuration("1M");

        await Assert.That(duration).IsEqualTo(TimeSpan.FromDays(30));
    }

    [Test]
    public async Task ParseDuration_Valid_Years_Returns_Expected_TimeSpan()
    {
        TimeSpan duration = SuspensionsController.ParseDuration("1Y");

        await Assert.That(duration).IsEqualTo(TimeSpan.FromDays(365));
    }

    [Test]
    public async Task ParseDuration_Invalid_Or_Empty_Returns_TimeSpan_Zero()
    {
        using (Assert.Multiple())
        {
            await Assert.That(SuspensionsController.ParseDuration(string.Empty)).IsEqualTo(TimeSpan.Zero);
            await Assert.That(SuspensionsController.ParseDuration(" ")).IsEqualTo(TimeSpan.Zero);
            await Assert.That(SuspensionsController.ParseDuration("D")).IsEqualTo(TimeSpan.Zero);
            await Assert.That(SuspensionsController.ParseDuration("INVALID")).IsEqualTo(TimeSpan.Zero);
            await Assert.That(SuspensionsController.ParseDuration("-5D")).IsEqualTo(TimeSpan.Zero);
            await Assert.That(SuspensionsController.ParseDuration("0D")).IsEqualTo(TimeSpan.Zero);
        }
    }
}

