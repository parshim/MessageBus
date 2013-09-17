using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MessageBus.Binding.RabbitMQ
{
	public class ReplyToBehavior : IEndpointBehavior
	{
		public class ReplyToInspector : IDispatchMessageInspector
		{
			public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
			{
				var reply = request.Headers.ReplyTo;
				OperationContext.Current.OutgoingMessageHeaders.To = reply.Uri;
				OperationContext.Current.OutgoingMessageHeaders.RelatesTo = request.Headers.MessageId;
				return null;
			}

			public void BeforeSendReply(ref Message reply, object correlationState)
			{
			}
		}

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new ReplyToInspector());
		}

		public void Validate(ServiceEndpoint endpoint)
		{
		}
	}
}