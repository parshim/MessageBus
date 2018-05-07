using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusAsyncRpcCommunicationTest
    {
        [Test]
        public async Task Bus_MakeRpcCall()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish()))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber())
                {
                    subscriber.Subscribe((RequestMessage m) => new ResponseMessage
                    {
                        Code = m.Data.Length
                    });

                    subscriber.Open();

                    using (IRpcAsyncPublisher rpcPublisher = bus.CreateAsyncRpcPublisher())
                    {
                        using (var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                        {
                            ResponseMessage response = await rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                            {
                                Data = "Hello, world!"
                            }, ctx.Token);

                            response.ShouldBeEquivalentTo(new ResponseMessage
                            {
                                Code = 13
                            });
                        }
                    }
                }
            }
        }

        [Test]
        public void Bus_MakeAsyncRpcCall()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish()))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber())
                {
                    subscriber.Subscribe((RequestMessage m) => new ResponseMessage
                    {
                        Code = m.Data.Length
                    });

                    subscriber.Open();

                    using (IRpcAsyncPublisher rpcPublisher = bus.CreateAsyncRpcPublisher())
                    {
                        ResponseMessage response = null;

                        using (ManualResetEvent ev = new ManualResetEvent(false))
                        {
                            using (var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                            {
                                rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                                {
                                    Data = "Hello, world!"
                                }, ctx.Token).ContinueWith(t =>
                                {
                                    response = t.Result;

                                    ev.Set();
                                });

                                bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(20));

                                if (!waitOne)
                                {
                                    Assert.Fail("No response received");
                                }

                                response.ShouldBeEquivalentTo(new ResponseMessage
                                {
                                    Code = 13
                                });
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void Bus_MakeRpcVoidCall_SubscriberReturnData()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish()))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber())
                {
                    RequestMessage actual = null;

                    subscriber.Subscribe((RequestMessage m) =>
                    {
                        actual = m;

                        return new ResponseMessage();
                    });

                    subscriber.Open();

                    using (IRpcAsyncPublisher rpcPublisher = bus.CreateAsyncRpcPublisher())
                    {
                        var expected = new RequestMessage
                        {
                            Data = "Hello, world!"
                        };

                        using (var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                        {
                            var task = rpcPublisher.Send(expected, ctx.Token);

                            task.Wait(TimeSpan.FromSeconds(30));

                            task.IsCompleted.Should().BeTrue();

                            actual.ShouldBeEquivalentTo(expected);
                        }
                    }
                }
            }
        }

        [Test]
        public void Bus_MakeRpcVoidCall_SubscriberDoNotReturnData()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish()))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber())
                {
                    RequestMessage actual = null;

                    subscriber.Subscribe((RequestMessage m) =>
                    {
                        actual = m;
                    });

                    subscriber.Open();

                    using (IRpcAsyncPublisher rpcPublisher = bus.CreateAsyncRpcPublisher())
                    {
                        var expected = new RequestMessage
                        {
                            Data = "Hello, world!"
                        };

                        using (var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                        {
                            var task = rpcPublisher.Send(expected, ctx.Token);

                            task.Wait(TimeSpan.FromSeconds(30));

                            actual.ShouldBeEquivalentTo(expected);

                            task.IsCompleted.Should().BeTrue();
                        }
                    }
                }
            }
        }

        [Test]
        public void Bus_MakeRpcCall_ExceptionOnHandler()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish()))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber())
                {
                    subscriber.Subscribe((RequestMessage m) =>
                    {
                        throw new Exception("ooops");
                    });

                    subscriber.Open();

                    using (IRpcAsyncPublisher rpcPublisher = bus.CreateAsyncRpcPublisher())
                    {
                        using (var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                        {
                            var task = rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                            {
                                Data = "Hello, world!"
                            }, ctx.Token);
                            
                            try
                            {
                                var result = task.Result;

                                Assert.Fail("No exception");
                            }
                            catch (AggregateException ex) when (ex.InnerException is RpcCallException)
                            {
                                (ex.InnerException as RpcCallException).Reason.Should().Be(RpcFailureReason.HandlerError);
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void Bus_MakeRpcCall_RejectedByHandler()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish()))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.UseTransactionalDelivery()))
                {
                    subscriber.Subscribe((RequestMessage m) =>
                    {
                        throw new RejectMessageException();
                    });

                    subscriber.Open();

                    using (IRpcAsyncPublisher rpcPublisher = bus.CreateAsyncRpcPublisher())
                    {
                        using (var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                        {
                            var task = rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                            {
                                Data = "Hello, world!"
                            }, ctx.Token);

                            try
                            {
                                task.Wait(TimeSpan.FromSeconds(30));

                                Assert.Fail("No exception");
                            }
                            catch (AggregateException ex) when (ex.InnerException is RpcCallException)
                            {
                                (ex.InnerException as RpcCallException).Reason.Should().Be(RpcFailureReason.Reject);
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public async Task Bus_MakeRpcCall_TimeOutOnReply()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish()))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber())
                {
                    subscriber.Subscribe((RequestMessage m) => Thread.Sleep(TimeSpan.FromSeconds(20)));

                    subscriber.Open();

                    using (IRpcAsyncPublisher rpcPublisher = bus.CreateAsyncRpcPublisher())
                    {
                        using (var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                        {
                            try
                            {
                                var result = await rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                                {
                                    Data = "Hello, world!"
                                }, ctx.Token);

                                Assert.Fail("No exception");
                            }
                            catch (TaskCanceledException ex)
                            {
                                ex.CancellationToken.ShouldBeEquivalentTo(ctx.Token);
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void Bus_MakeRpcCall_NotRoutedToAnySubscriber()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish()))
            {
                using (IRpcAsyncPublisher rpcPublisher = bus.CreateAsyncRpcPublisher())
                {
                    using (var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                    {
                        var task = rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                        {
                            Data = "Hello, world!"
                        }, ctx.Token);

                        try
                        {
                            task.Wait(TimeSpan.FromSeconds(30));

                            Assert.Fail("No exception");
                        }
                        catch (AggregateException ex) when (ex.InnerException is RpcCallException)
                        {
                            (ex.InnerException as RpcCallException).Reason.Should().Be(RpcFailureReason.NotRouted);
                        }
                    }
                }
            }
        }

        [Test]
        public void Bus_MultiClientRpcCall()
        {
            using (IBus serviceBus = new RabbitMQBus(), client1Bus = new RabbitMQBus(), client2Bus = new RabbitMQBus())
            {
                using (ISubscriber subscriber = serviceBus.CreateSubscriber())
                {
                    subscriber.Subscribe((RequestMessage m) => new ResponseMessage
                    {
                        Code = int.Parse(m.Data)
                    });

                    subscriber.Open();
                    
                    using (IRpcAsyncPublisher rpcPublisher1 = client1Bus.CreateAsyncRpcPublisher(), rpcPublisher2 = client2Bus.CreateAsyncRpcPublisher())
                    {
                        List<ResponseMessage> c1Responses = new List<ResponseMessage>(), c2Responses = new List<ResponseMessage>();

                        using (var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                        {
                            Task t1 = rpcPublisher1.Send<RequestMessage, ResponseMessage>(new RequestMessage
                            {
                                Data = "1"
                            }, ctx.Token).ContinueWith(task => c1Responses.Add(task.Result));

                            Task t2 = rpcPublisher2.Send<RequestMessage, ResponseMessage>(new RequestMessage
                            {
                                Data = "2"
                            }, ctx.Token).ContinueWith(task => c2Responses.Add(task.Result));
                            
                            Task.WaitAll(t1, t2);
                        }
                        
                        c1Responses.All(message => message.Code == 1).Should().BeTrue();
                        c2Responses.All(message => message.Code == 2).Should().BeTrue();
                    }
                }
            }
        }
    }
}