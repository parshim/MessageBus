using Microsoft.AspNet.SignalR;
using NUnit.Framework;

namespace Core.SignalR.IntegrationTests
{
    [TestFixture]
    public class SignalRBackplaneTest
    {
        [Test]
        public void ConectToSignalR_Send_ReceiveMessage()
        {
            GlobalHost.DependencyResolver.UseRabbit(new RabbitScaleoutConfiguration
            {
                ConnectionString = "amqp://localhost"
            });

            GlobalHost.ConnectionManager.GetHubContext<TestHub>().Clients.All.updateData("Hello, World!");

            GlobalHost.ConnectionManager.GetHubContext<TestHub>().Clients.Group("G").updateData("Hello, World!");
        }
    }

    public class TestHub : Hub
    {
    }
}
