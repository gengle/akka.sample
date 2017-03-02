using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Actors
{
    public class SqlDataInfo
    {
        public string Server { get; }
        public string Database { get; }
        public SqlDataInfo(string server, string database)
        {
            this.Server = server;
            this.Database = database;
        }
    }

    public class CwsAbbreviation
    {
        public string Name { get; }
        public CwsAbbreviation(string name)
        {
            this.Name = name;
        }
    }
    public class UsageState
    {
        public bool Disabled { get; }
        public UsageState(bool disabled)
        {
            this.Disabled = disabled;
        }
    }
    public class WorkPartnerActor: ReceiveActor
    {
        private SqlDataInfo sqlInfo;
        private CwsAbbreviation cwsAbbreviation;
        private bool Disabled { get; set; }


        public WorkPartnerActor(string abbreviation, bool disabled)
        {
            cwsAbbreviation = new CwsAbbreviation(abbreviation);
            this.Disabled = disabled;
            Context.ActorOf(Props.Create(() => new CapabilityCoordinatorActor()), "capabilities");
            this.Receive<SqlDataInfo>(x =>
            {
                sqlInfo = x;
            });
            this.Receive<CwsAbbreviation>(x =>
            {
                cwsAbbreviation = x;
            });
            this.Receive<UsageState>(x =>
            {
                Disabled = x.Disabled;
            });
            this.Receive<LogStatus>(x =>
            {
                Console.WriteLine($"{cwsAbbreviation.Name} Disabled:{Disabled}");
                foreach (var child in Context.GetChildren())
                    child.Forward(x);
            });
            this.Receive<DisableWorkPartner>(x =>
            {
                Disabled = true;
            });
            this.Receive<EnableWorkPartner>(x =>
            {
                Disabled = false;
            });
        }
    }
    public class LogStatus
    {

    }
    public class WorkCapabilityActor: TypedActor
    {
        public WorkCapabilityActor()
        {

        }
    }
    public class NewWorkPartner
    {
        public string Name { get; }
        public NewWorkPartner(string name)
        {
            this.Name = name;
        }
    }
    public class DisableWorkPartner
    {
        public string Name { get; }
        public DisableWorkPartner(string name)
        {
            this.Name = name;
        }
    }
    public class EnableWorkPartner
    {
        public string Name { get; }
        public EnableWorkPartner(string name)
        {
            this.Name = name;
        }
    }

    public class MetaCoordinatorActor: ReceiveActor
    {
        HashSet<string> _workPartners = new HashSet<string>();

        public MetaCoordinatorActor()
        {
            Receive<NewWorkPartner>(x =>
            {
                if (!_workPartners.Any(y=>y == x.Name))
                {
                    _workPartners.Add(x.Name);
                    var props = Props.Create(() => new WorkPartnerActor(x.Name ,true));
                    Context.ActorOf(props, x.Name);
                }
            });
            Receive<LogStatus>(x =>
            {
                var actor = Context.ActorSelection("/user/metaCoordinator/*");
                actor.Tell(x);
            });
            Receive<DisableWorkPartner>(x =>
            {
                var actor = Context.Child(x.Name);
                actor.Forward(x);
            });
            Receive<EnableWorkPartner>(x =>
            {
                var actor = Context.Child(x.Name);
                actor.Forward(x);
            });
        }
    }

    public class CapabilityCoordinatorActor: ReceiveActor
    {
        public CapabilityCoordinatorActor()
        {
            Receive<LogStatus>(x =>
            {
                Console.WriteLine(Self.Path);
            });
        }
    }
}
