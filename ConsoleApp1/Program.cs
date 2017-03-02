using Akka.Actor;
using Akka.Configuration;
using ConsoleApp1.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var akkaConfig = @"
akka {  
    stdout-loglevel = DEBUG
    loglevel = DEBUG
    log-config-on-start = on        
    actor {                
        debug {  
              receive = on 
              autoreceive = on
              lifecycle = on
              event-stream = on
              unhandled = on
        }
    }";
            var config = ConfigurationFactory.ParseString(akkaConfig);
            // Create a new actor system (a container for your actors)
            var system = ActorSystem.Create("schedulerSystem", config);
            var coordinatorProps = Props.Create<MetaCoordinatorActor>();
            var coordinator = 
                system.ActorOf(coordinatorProps, "metaCoordinator");
           
            coordinator.Tell(new NewWorkPartner("CCI"));
            coordinator.Tell(new NewWorkPartner("AMI"));

            Thread.Sleep(1000);
            //coordinator.Tell(new LogStatus());
            
            var actor= system.ActorSelection("/user/metaCoordinator/CCI");
            actor.Tell(new UsageState(disabled: false));

            coordinator.Tell(new LogStatus());
            system.WhenTerminated.Wait();
            // This prevents the app from exiting
            // before the async work is done
            Console.ReadLine();
        }
    }
}
