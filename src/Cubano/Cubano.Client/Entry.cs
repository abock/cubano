using System;

using Cairo;
using Gtk;
using Moonlight.Gtk;
using System.Windows;

using Application = Gtk.Application;

namespace Cubano.Client
{
    public static class Entry
    {
        public static void Main ()
        {
            Application.Init ();
            XamlRuntime.Init ();

            Window window = new Window ("Cubano");
            window.SetDefaultSize (400, 300);
            window.DeleteEvent += (o, e) => Application.Quit ();

            XamlHost xaml_host = new XamlHost ();
            xaml_host.LoadXaml (@"
                <Canvas 
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                    >
                    <Button Content=""Hell Yes"" Width=""100"" Height=""50"" />
                </Canvas>
            ");

            window.Add (xaml_host);
            window.ShowAll ();

            Application.Run ();
        }
    }
}

