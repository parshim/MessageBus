Connection Strings
==========

## AMQP 

#####  URL schema  

  amqp://username:password@localhost:5672/virtualhost/exchange?routingKey=value
   \_/   \_______________/ \_______/ \__/ \_________/ \_____________/ \_______/
    |           |              |       |       |            |             |                
    |           |      broker hostname |       |            |         Specifies routing key value (optional)
    |           |                      |       |            |
    |           |                      |  virtual host (optional)
    |           |                      |                    | 
    |           |                      |                    |
    |           |       node port, if absent 5672 is used   |
    |           |                                           |
    |  rabbit mq user info, if absent guest:guest is used   |
    |                                                       |   
    |                                       exchange name used for dispatching messages (optional)
 scheme  
  name  

  
##### Optional values

* Virtual host and exchange are both optional, however if only one specified it is considered to be exchange name, rather then virtual host name

* Routing key will be used to publish and subscribe for messages, however by default amq.headers exchange is being used, which is routing key agnostic