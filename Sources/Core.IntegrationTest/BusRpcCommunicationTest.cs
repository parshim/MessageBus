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
    public class BusRpcCommunicationTest
    {
        [Test]
        public void Bus_MakeSyncRpcCall()
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

                    using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher())
                    {
                        ResponseMessage response = rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                        {
                            Data = "Hello, world!"
                        }, TimeSpan.FromSeconds(10));
                        
                        response.ShouldBeEquivalentTo(new ResponseMessage
                        {
                            Code = 13
                        });
                    }
                }
            }
        }

        [Test]
        public void Bus_MakeSyncRpcCall_NoFastReply()
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

                    using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher(c => c.DisableFastReply()))
                    {
                        ResponseMessage response = rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                        {
                            Data = "Hello, world!"
                        }, TimeSpan.FromSeconds(10));
                        
                        response.ShouldBeEquivalentTo(new ResponseMessage
                        {
                            Code = 13
                        });
                    }
                }
            }
        }

        [Test]
        public void Bus_MakeSyncRpcCall_NoFastReply_CustomExchange()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish()))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.SetReplyExchange("amq.direct")))
                {
                    subscriber.Subscribe((RequestMessage m) => new ResponseMessage
                    {
                        Code = m.Data.Length
                    });

                    subscriber.Open();

                    using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher(c => c.DisableFastReply().SetReplyExchange("amq.direct").SetReplyTo("MyReplyKey")))
                    {
                        ResponseMessage response = rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                        {
                            Data = "Hello, world!"
                        }, TimeSpan.FromSeconds(10));
                        
                        response.ShouldBeEquivalentTo(new ResponseMessage
                        {
                            Code = 13
                        });
                    }
                }
            }
        }

        [Test]
        public void Bus_MakeSyncRpcCall_NoFastReply_CustomExchange_NoReplyTo()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish()))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.SetReplyExchange("amq.direct")))
                {
                    subscriber.Subscribe((RequestMessage m) => new ResponseMessage
                    {
                        Code = m.Data.Length
                    });

                    subscriber.Open();

                    using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher(c => c.DisableFastReply().SetReplyExchange("amq.direct")))
                    {
                        ResponseMessage response = rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                        {
                            Data = "Hello, world!"
                        }, TimeSpan.FromSeconds(10));
                        
                        response.ShouldBeEquivalentTo(new ResponseMessage
                        {
                            Code = 13
                        });
                    }
                }
            }
        }

        [Test]
        public void Bus_MakeSyncRpcCall_NoFastReply_BusLevelConfig()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish().DisableFastReply().SetReplyExchange("amq.direct")))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber())
                {
                    subscriber.Subscribe((RequestMessage m) => new ResponseMessage
                    {
                        Code = m.Data.Length
                    });

                    subscriber.Open();

                    using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher())
                    {
                        ResponseMessage response = rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                        {
                            Data = "Hello, world!"
                        }, TimeSpan.FromSeconds(10));
                        
                        response.ShouldBeEquivalentTo(new ResponseMessage
                        {
                            Code = 13
                        });
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

                    using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher())
                    {
                        ResponseMessage response = null;

                        using (ManualResetEvent ev = new ManualResetEvent(false))
                        {
                            rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                            {
                                Data = "Hello, world!"
                            }, TimeSpan.FromSeconds(10), message =>
                            {
                                response = message;

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

                    using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher())
                    {
                        var expected = new RequestMessage
                        {
                            Data = "Hello, world!"
                        };

                        rpcPublisher.Send(expected, TimeSpan.FromSeconds(10));

                        actual.ShouldBeEquivalentTo(expected);
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

                    using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher())
                    {
                        var expected = new RequestMessage
                        {
                            Data = "Hello, world!"
                        };

                        rpcPublisher.Send(expected, TimeSpan.FromSeconds(10));

                        actual.ShouldBeEquivalentTo(expected);
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
                    subscriber.Subscribe((RequestMessage m) => {
                                                                   throw new Exception("ooops");
                    });

                    subscriber.Open();

                    using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher())
                    {
                        try
                        {
                            rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                            {
                                Data = "Hello, world!"
                            }, TimeSpan.FromSeconds(10));

                            Assert.Fail("No exception");
                        }
                        catch (RpcCallException ex)
                        {
                            ex.Reason.Should().Be(RpcFailureReason.HandlerError);
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
                    subscriber.Subscribe((RequestMessage m) => {
                                                                   throw new RejectMessageException();
                    });

                    subscriber.Open();

                    using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher())
                    {
                        try
                        {
                            rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                            {
                                Data = "Hello, world!"
                            }, TimeSpan.FromSeconds(10));

                            Assert.Fail("No exception");
                        }
                        catch (RpcCallException ex)
                        {
                            ex.Reason.Should().Be(RpcFailureReason.Reject);
                        }
                    }
                }
            }
        }

        [Test]
        public void Bus_MakeRpcCall_TimeOutOnReply()
        {
            using (IBus bus = new RabbitMQBus(c => c.SetReceiveSelfPublish()))
            {
                using (ISubscriber subscriber = bus.CreateSubscriber())
                {
                    subscriber.Subscribe((RequestMessage m) => Thread.Sleep(TimeSpan.FromSeconds(20)));

                    subscriber.Open();

                    using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher())
                    {
                        try
                        {
                            rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                            {
                                Data = "Hello, world!"
                            }, TimeSpan.FromSeconds(10));

                            Assert.Fail("No exception");
                        }
                        catch (RpcCallException ex)
                        {
                            ex.Reason.Should().Be(RpcFailureReason.TimeOut);
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
                using (IRpcPublisher rpcPublisher = bus.CreateRpcPublisher())
                {
                    try
                    {
                        rpcPublisher.Send<RequestMessage, ResponseMessage>(new RequestMessage
                        {
                            Data = "Hello, world!"
                        }, TimeSpan.FromSeconds(10));

                        Assert.Fail("No exception");
                    }
                    catch (RpcCallException ex)
                    {
                        ex.Reason.Should().Be(RpcFailureReason.NotRouted);
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

                    const int calls = 20;

                    using (IRpcPublisher rpcPublisher1 = client1Bus.CreateRpcPublisher(), rpcPublisher2 = client2Bus.CreateRpcPublisher())
                    {
                        List<ResponseMessage> c1Responses = new List<ResponseMessage>(), c2Responses = new List<ResponseMessage>();

                        Task t1 = Task.Factory.StartNew(() =>
                        {
                            for (int i = 0; i < calls; i++)
                            {
                                ResponseMessage response =
                                    rpcPublisher1.Send<RequestMessage, ResponseMessage>(new RequestMessage
                                    {
                                        Data = "1"
                                    }, TimeSpan.FromSeconds(10));

                                c1Responses.Add(response);
                            }
                        });
                        
                        Task t2 = Task.Factory.StartNew(() =>
                        {
                            for (int i = 0; i < calls; i++)
                            {
                                ResponseMessage response =
                                    rpcPublisher2.Send<RequestMessage, ResponseMessage>(new RequestMessage
                                    {
                                        Data = "2"
                                    }, TimeSpan.FromSeconds(10));


                                c2Responses.Add(response);
                            }
                        });

                        Task.WaitAll(t1, t2);

                        c1Responses.Should().HaveCount(calls);
                        c2Responses.Should().HaveCount(calls);

                        c1Responses.All(message => message.Code == 1).Should().BeTrue();
                        c2Responses.All(message => message.Code == 2).Should().BeTrue();
                    }
                }
            }
        }
    }

    public class RequestMessage
    {
        public string Data { get; set; }
    }

    public class ResponseMessage
    {
        public int Code { get; set; }
    }
}
