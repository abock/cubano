using System;
using System.Reflection;

using Banshee.Base;
using Banshee.ServiceStack;
using Mono.Addins;

namespace Cubano.Client
{
    public class CubanoClient : Banshee.Gui.GtkBaseClient
    {
        public static void Main (string [] args)
        {
            /*string addin_path = ApplicationContext.CommandLine.Contains ("uninstalled") 
                ? "." : Paths.Combine (Paths.ApplicationData, "cubano");

            MethodInfo method = typeof (AddinManager).GetMethod ("Initialize", 
                new Type [] { typeof (string), typeof (Assembly) });
            if (method != null) {
                method.Invoke (null, new object [] { addin_path, typeof (ServiceManager).Assembly });
            } else {
                typeof (AddinManager).GetMethod ("Initialize", 
                    new Type [] { typeof (string) }).Invoke (null, new object [] { addin_path });
            }

            Startup<CubanoClient> (args);*/
            
            CubanoWindowClutter.Start (args);
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
