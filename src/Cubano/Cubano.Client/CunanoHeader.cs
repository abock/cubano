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
        public ActionLabel SourceBackButton { get; private set; }
    
        public CubanoHeader () : base (0.0f, 0.0f, 1.0f, 0.0f)
        {
            var action_service = ServiceManager.Get<InterfaceActionService> ();
            
            var table = new Table (2, 3, false) {
                ColumnSpacing = 20,
                RowSpacing = 8
            };
            
            table.Attach (new Alignment (0.0f, 0.0f, 0.0f, 0.0f) {
                (SourceBackButton = new ActionLabel () {
                    Text = "\u2190", 
                    CurrentFontSizeEm = 2.0,
                    DefaultFontSizeEm = 2.0,
                    FontFamily = "DejaVu Sans"
                })
            }, 0, 1, 0, 1, AttachOptions.Shrink, AttachOptions.Fill | AttachOptions.Expand, 0, 0);
            
            table.Attach (new Alignment (1.0f, 0.0f, 0.0f, 0.0f) {
                (SearchEntry = new SearchEntry ())
            }, 1, 2, 0, 1, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink, 0, 0);
                
            var action_box = new HBox ();
            var align = new Alignment (0.0f, 0.0f, 0.0f, 0.0f) { action_box };
            table.Attach (align, 2, 3, 0, 1, AttachOptions.Shrink, AttachOptions.Fill | AttachOptions.Expand, 0, 0);
            
            var import = new ActionLabel () { Text = Banshee.I18n.Catalog.GetString ("banshee-1", "Import") };
            import.Activated += (o, e) => action_service.GlobalActions["ImportAction"].Activate ();
            action_box.PackStart (import, false, false, 0);
            
            action_box.PackStart (new ActionLabel () { Text = "|", CanActivate = false }, false, false, 6);
            
            var preferences = new ActionLabel () { Text = Banshee.I18n.Catalog.GetString ("banshee-1", "Preferences") };
            preferences.Activated += (o, e) => action_service.GlobalActions["PreferencesAction"].Activate ();
            action_box.PackStart (preferences, false, false, 0);
            
            var close = new ActionLabel () { Text = "   \u2715", CurrentFontSizeEm = 1.2 };
            close.Activated += (o, e) => action_service.GlobalActions["QuitAction"].Activate ();
            action_box.PackStart (close, false, false, 0);
            
            // Second row
            Toolbar = (Toolbar)action_service.UIManager.GetWidget ("/HeaderToolbar");
            Toolbar.ShowArrow = false;
            Toolbar.ToolbarStyle = ToolbarStyle.BothHoriz;
            Toolbar.IconSize = IconSize.Menu;
            
            // Hide all default toolbar items, we only want the Banshee toolbar
            // for sources that install action items there... ultimately sucky
            var children = Toolbar.Children;
            for (int i = 0; i < children.Length; i++) {
                children[i].Visible = false;
            }
            
            var box = new HBox ();
            box.PackStart (new CategorySourceView (), false, false, 0);
            box.PackEnd (Toolbar, false, false, 0);
            table.Attach (box, 0, 3, 1, 2, 
                AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink, 0, 0);
            
            Add (table);
            ShowAll ();
        }
    }
}
