// 
// CubanoWindow.cs
//  
// Author:
//       Aaron Bockover <abockover@novell.com>
// 
// Copyright 2009 Aaron Bockover
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Gtk;

using Hyena.Gui;
using Hyena.Data;
using Hyena.Data.Gui;
using Hyena.Widgets;

using Banshee.Base;
using Banshee.ServiceStack;
using Banshee.Sources;
using Banshee.Database;
using Banshee.Collection;
using Banshee.Collection.Database;
using Banshee.MediaEngine;
using Banshee.Configuration;

using Banshee.Gui;
using Banshee.Gui.Widgets;
using Banshee.Gui.Dialogs;
using Banshee.Widgets;
using Banshee.Collection.Gui;
using Banshee.Sources.Gui;

namespace Cubano.Client
{
    public class CubanoWindow : BaseClientWindow, IClientWindow, IDBusObjectName, IService, IDisposable
    {
        private CubanoXamlHost xaml_host;
        
        protected CubanoWindow (IntPtr ptr) : base (ptr)
        {
        }
        
        public CubanoWindow () : base ("Cubano", "cubano.window", 1000, 500)
        {
        }
        
#region System Overrides 
        
        public override void Dispose ()
        {
            lock (this) {
                Hide ();
                base.Dispose ();
                Gtk.Application.Quit ();
            }
        }
        
#endregion     

        protected override void Initialize ()
        {
            xaml_host = new CubanoXamlHost ();
            xaml_host.LoadXaml (@"
                <Canvas 
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                    >
                    <Button Content=""Hell Yes"" Width=""100"" Height=""50"" />
                </Canvas>
            ");

            Add (xaml_host);
            
            InitialShowPresent ();
        }

        // Note this is copied from GtkBaseClient, it was added in 
        // r5063 for 1.5.x, but I am aiming to make Cubano work on 1.4.x
        protected void InitialShowPresent ()
        {
            bool hide = ApplicationContext.CommandLine.Contains ("hide");
            bool present = !hide && !ApplicationContext.CommandLine.Contains ("no-present");
            
            if (!hide) {
                Show ();
            }
            
            if (present) {
                Present ();
            }
        } 

#region Service/Export Implementation

        IDBusExportable IDBusExportable.Parent {
            get { return null; }
        }
        
        string IDBusObjectName.ExportObjectName {
            get { return "ClientWindow"; }
        }

        string IService.ServiceName {
            get { return "CubanoPlayerInterface"; }
        }

#endregion

    }
}
