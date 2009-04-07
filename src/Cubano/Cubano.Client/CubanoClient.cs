using System;

using Banshee.Base;
using Banshee.ServiceStack;
using Mono.Addins;
using Moonlight.Gtk;

namespace Cubano.Client
{
    public class CubanoClient : Banshee.Gui.GtkBaseClient
    {
        public static void Main (string [] args)
        {
            AddinManager.Initialize (ApplicationContext.CommandLine.Contains ("uninstalled") 
                ? "." : Paths.Combine (Paths.ApplicationData, "cubano"),
                typeof (ServiceManager).Assembly);

            Startup<CubanoClient> (args);
        }
        
        protected override void OnRegisterServices ()
        {
            XamlRuntime.Init ();
            ServiceManager.RegisterService<CubanoWindow> ();
        }

        public override string ClientId {
            get { return "cubano"; }
        }
    }
}
