using Farmelo.Business.Handlers.Health;
using Farmelo.Business.Queries.Health;

namespace Farmelo.Test.Health;

[TestFixture]
public sealed class HealthQueryHandlerTests
{
    [Test]
    public async Task Handle_ReturnsHealthyStatus()
    {
        var handler = new GetHealthStatusQueryHandler();

        var result = await handler.Handle(new GetHealthStatusQuery(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Payload, Is.Not.Null);
        Assert.That(result.Payload!.Status, Is.EqualTo("Healthy"));
        Assert.That(result.Payload.Service, Is.EqualTo("Farmelo.API"));
    }
}
