// 
// CubanoHeader.cs
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
using Banshee.I18n;

using Banshee.Gui;
using Banshee.Gui.Widgets;
using Banshee.Sources.Gui;
using Banshee.Widgets;
using Banshee.ServiceStack;

namespace Cubano.Client
{
    public class CubanoHeader : Alignment
    {
        public SearchEntry SearchEntry { get; private set; }
        public Toolbar Toolbar { get; private set; }
    
        public CubanoHeader () : base (0.0f, 0.0f, 1.0f, 0.0f)
        {
            var action_service = ServiceManager.Get<InterfaceActionService> ();
            
            Toolbar = (Toolbar)action_service.UIManager.GetWidget ("/HeaderToolbar");
            Toolbar.ShowArrow = false;
            Toolbar.ToolbarStyle = ToolbarStyle.BothHoriz;
            Toolbar.IconSize = IconSize.Menu;
            
            // Capture the default children so we can hide them
            // after we add Cubano-specific items to the toolbar
            var children = Toolbar.Children;
            
            // Build the Cubano Toolbar
            Toolbar.Insert (new GenericToolItem<Widget> (new RepeatActionButton ()), 0);
            
            var filler = new Alignment (0.5f, 0.5f, 0.75f, 0.0f);
            filler.Add (SearchEntry = new SearchEntry ());
            action_service.PopulateToolbarPlaceholder (Toolbar, "/HeaderToolbar/TrackInfoDisplay", filler, true);

            var import = (ToolButton)action_service.GlobalActions["ImportAction"].CreateToolItem ();
            import.IsImportant = true;
            import.Label = Banshee.I18n.Catalog.GetString ("banshee-1", "Import");
            Toolbar.Add (import);
            
            var prefs = (ToolButton)action_service.GlobalActions["PreferencesAction"].CreateToolItem ();
            prefs.IsImportant = false;
            Toolbar.Add (prefs);
            
            var quit = (ToolButton)action_service.GlobalActions["QuitAction"].CreateToolItem ();
            quit.IsImportant = false;
            quit.StockId = Stock.Close;
            Toolbar.Add (quit);

            // Show the Cubano items, hide the defaults we captured
            Toolbar.ShowAll ();
            for (int i = 0; i < children.Length; i++) {
                children[i].Visible = false;
            }
            
            var vbox = new VBox ();
            vbox.PackStart (Toolbar);
            vbox.PackStart (new CategorySourceView ());
            Add (vbox);
            ShowAll ();
        }
    }
}
