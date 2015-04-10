[![Build Status](https://travis-ci.org/parshim/MessageBus.svg)](https://travis-ci.org/parshim/MessageBus)

MessageBus
==========

Message Bus Framework based on RabbitMQ with intuitive interface and built in Json serialization.

## Features

 * Publish\Subscribe
 * Workers
 * Transactional delivery
 * Message persistence
 * POCO messages support
 * Subscription for individual messages	
 * Message headers and header based subscription filter
 * Connection string support
 * Most RabbitMQ features are supported and provided via convenient and user friendly API
 * Fast binary data transfer (pass through without serialization)
 
## Example

##### Messages are just POCOs

```c#
public class Person
{
    public int Id { get; set; }
}
```

Optionally, message classes may be annotated with DataContract and DataMemebr attributes to be able to generate contract classes on publisher and subscriber parties by using svcutil.exe.
	
##### Publish your first message	
	
```c#
using (IBus bus = new RabbitMQBus(busId))
{
	using (IPublisher publisher = bus.CreatePublisher())
    {
        publisher.Send(new Person { Id = 5 });
    }
}
```

Bus instance holds connection to RabbitMQ broker, thus once disposed connection is closed. 
Publisher creates dedicated AMQP model and can be used to publish messages from single thread. If publication is required from multiple threads, each required to create its own publisher instance.
All messages are sent by default to amq.headers exchange with two headers: Name and Namespace. By default it is type name and name space, however id class is decorated with DataContractAttribute Name and Namespace will be taken from it (if specified). 

##### Subscribe for messages

```c#
using (IBus bus = new RabbitMQBus(busId))
{
	using (ISubscriber subscriber = bus.CreateSubscriber())
    {
		subscriber.Subscribe((Action<Person>) (p =>
        {
            // Do your work here
        }));

		// Start consuming messages
        subscriber.Open();
	}
}
```

Subscriber creates by default temporary queue, and each subscription binds this queue to amq.headers exchange with Name and Namespace headers. 
Thus only this particular message type will arrive to the subscriber.

## Documentation

There are a lot of features inside, most covered by tests and you are welcome to take a look.

I am working on documentation, so stay tuned.

Enjoy!
