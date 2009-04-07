using System;

//using Hyena;
//using Hyena.CommandLine;

//using Banshee.Base;
using Banshee.ServiceStack;

namespace Cubano.Client
{
    public class CubanoClient : Banshee.Gui.GtkBaseClient
    {
        public static void Main (string [] args)
        {
            Startup<CubanoClient> (args);
        }
        
        protected override void OnRegisterServices ()
        {
            ServiceManager.RegisterService<CubanoWindow> ();
        }

        public override string ClientId {
            get { return "cubano"; }
        }
    }
}
