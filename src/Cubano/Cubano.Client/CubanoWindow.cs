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

using Hyena.Gui.Canvas;

namespace Cubano.Client
{
    public class CubanoWindow : BaseClientWindow, IClientWindow, IDBusObjectName, IService, IDisposable
    {
        private Toolbar header_toolbar;
        private VBox primary_vbox;
        
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
            BuildInterface ();
            ConnectEvents ();
            InitialShowPresent ();
        }

#region Interface Construction

        private void BuildInterface ()
        {
            primary_vbox = new VBox ();

            BuildHeader ();

            primary_vbox.Show ();
            Add (primary_vbox);
        }

        private void BuildHeader ()
        {
            Alignment toolbar_alignment = new Alignment (0.0f, 0.0f, 1.0f, 1.0f);
            
            header_toolbar = (Toolbar)ActionService.UIManager.GetWidget ("/HeaderToolbar");
            header_toolbar.ShowArrow = false;
            header_toolbar.ToolbarStyle = ToolbarStyle.BothHoriz;
            
            toolbar_alignment.Add (header_toolbar);
            toolbar_alignment.ShowAll ();
            
           // primary_vbox.PackEnd (toolbar_alignment, false, false, 0);
            
            Widget next_button = new NextButton (ActionService);
            next_button.Show ();
            ActionService.PopulateToolbarPlaceholder (header_toolbar, "/HeaderToolbar/NextArrowButton", next_button);
            
            ConnectedSeekSlider seek_slider = new ConnectedSeekSlider ();
            seek_slider.Show ();
            ActionService.PopulateToolbarPlaceholder (header_toolbar, "/HeaderToolbar/SeekSlider", seek_slider);
            
            SeekableTrackInfoDisplay track_info_display = new SeekableTrackInfoDisplay ();
            track_info_display.Show ();
            ActionService.PopulateToolbarPlaceholder (header_toolbar, "/HeaderToolbar/TrackInfoDisplay", track_info_display, true);
            
            ConnectedVolumeButton volume_button = new ConnectedVolumeButton ();
            volume_button.Show ();
            ActionService.PopulateToolbarPlaceholder (header_toolbar, "/HeaderToolbar/VolumeButton", volume_button);
            
            var host = new CanvasHost ();
            
            var tile1 = new CanvasTestTile () { Width = 50, Height = 50 };
            var tile2_1 = new CanvasTestTile () { Width = 50, Height = 50 };
            var tile2_2 = new CanvasTestTile () { Width = 50, Height = 50, Expanded = true };
            var tile2_3 = new CanvasTestTile () { Width = 50, Height = 50 };
            var tile3 = new CanvasTestTile () { Width = 50, Height = 50 };
            var tile4 = new CanvasTestTile () { Width = 50, Height = 50, Expanded = true };
            
            var tile2_box = new CanvasHBox () { BorderWidth = 20, Spacing = 20, Expanded = true };
            tile2_box.Add (tile2_1);
            tile2_box.Add (tile2_2);
            tile2_box.Add (tile2_3);
            
            var container = new CanvasVBox () { BorderWidth = 10, Spacing = 10 };
            container.Add (tile1);
            container.Add (tile2_box);
            container.Add (tile3);
            
            host.Add (container);
            
            primary_vbox.PackStart (host, true, true, 0);
            host.Show ();
        }
        

        // NOTE: this is copied from GtkBaseClient, it was added in 
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

#endregion

#region Events and Logic Setup
        
        protected override void ConnectEvents ()
        {
            base.ConnectEvents ();

            /*// Service events
            ServiceManager.SourceManager.ActiveSourceChanged += OnActiveSourceChanged;
            ServiceManager.SourceManager.SourceUpdated += OnSourceUpdated;
            
            ActionService.TrackActions ["SearchForSameArtistAction"].Activated += OnProgrammaticSearch;
            ActionService.TrackActions ["SearchForSameAlbumAction"].Activated += OnProgrammaticSearch;

            // UI events
            view_container.SearchEntry.Changed += OnSearchEntryChanged;
            views_pane.SizeRequested += delegate {
                SourceViewWidth.Set (views_pane.Position);
            };
            
            source_view.RowActivated += delegate {
                Source source = ServiceManager.SourceManager.ActiveSource;
                if (source is ITrackModelSource) {
                    ServiceManager.PlaybackController.NextSource = (ITrackModelSource)source;
                    // Allow changing the play source without stopping the current song by
                    // holding ctrl when activating a source. After the song is done, playback will
                    // continue from the new source.
                    if (GtkUtilities.NoImportantModifiersAreSet (Gdk.ModifierType.ControlMask)) {
                        ServiceManager.PlaybackController.Next ();
                    }
                }
            };*/
            
            header_toolbar.ExposeEvent += OnToolbarExposeEvent;
            //footer_toolbar.ExposeEvent += OnToolbarExposeEvent;
        }
        
#endregion

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
