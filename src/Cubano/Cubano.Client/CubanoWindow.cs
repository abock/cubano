// 
// CubanoWindow.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2009 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;

using Hyena.Gui;
using Hyena.Data;
using Hyena.Data.Gui;
using Hyena.Widgets;
using Hyena.Gui.Canvas;

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
    public class CubanoWindow : BaseClientWindow, IClientWindow, IDBusObjectName, IService, IDisposable /*, IHasSourceView */
    {
        private VBox primary_vbox;
        private CubanoHeader header;
    
        // Major Layout Components
        private Alignment view_box;
        private HBox footer;
        private ViewContainer view_container;
        private MainMenu main_menu;
        private ToolButton forward_button;
        private ToolButton back_button;
        private Alignment playback_align;
        private Alignment source_align;
        private Alignment track_info_align;
        private CubanoWindowDecorator window_decorator;
        private CubanoSourcePopupWindow source_popup;
        
        // Major Interaction Components
        private LtrTrackSourceContents composite_view;
        private ObjectListSourceContents object_view;
        private Label status_label;

        protected CubanoWindow (IntPtr ptr) : base (ptr)
        {
        }
        
        public CubanoWindow () : base ("Cubano", "cubano.window", 1000, 500)
        {
        }
        
        protected override void Initialize ()
        {
            new Cubano.NowPlaying.NowPlayingSource ();
            
            Hyena.Gui.Theming.ThemeEngine.SetCurrentTheme<CubanoTheme> ();

            BuildPrimaryLayout ();
            ConnectEvents ();

            // ActionService.SourceActions.SourceView = this;
            
            composite_view.TrackView.HasFocus = true;
            
            InitialShowPresent ();
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

#region Interface Construction
        
        private void BuildPrimaryLayout ()
        {
            window_decorator = new CubanoWindowDecorator (this);
            primary_vbox = new VBox ();
            
            source_popup = new CubanoSourcePopupWindow () {
                WidthRequest = 250,
                HeightRequest = 300
            };
            
            main_menu = new MainMenu ();
            main_menu.Hide ();
            primary_vbox.PackStart (main_menu, false, false, 0);
            
            BuildHeader ();
            BuildViews ();
            BuildFooter ();
            
            ConfigureMargins (false);
            UpdateSourceHistory (null, null);
            
            
            primary_vbox.Show ();
            Add (primary_vbox);
        }
        
        private void BuildHeader ()
        {
            primary_vbox.PackStart (header = new CubanoHeader (), false, false, 0);
            header.Toolbar.ButtonPressEvent += (o, e) => window_decorator.CheckWindow = false;
            header.Toolbar.ExposeEvent += OnCubanoToolbarExposeEvent;
        }
        
        private void BuildViews ()
        {
            VBox source_box = new VBox ();
            source_box.PackStart (new UserJobTileHost (), false, false, 0);
            
            view_container = new ViewContainer ();
            composite_view = new LtrTrackSourceContents ();     
            composite_view.TrackView.HeaderVisible = false;
            view_container.Content = composite_view;
            view_container.Show ();
            
            view_box = new Alignment (0.0f, 0.0f, 1.0f, 1.0f);
            view_box.Add (view_container);
            view_box.ShowAll ();
            
            primary_vbox.PackStart (view_box, true, true, 0);
        }

        private void BuildFooter ()
        {
            status_label = new Label ();
            
            footer = new HBox (true, 0);
            
            var left_align = new Alignment (0.0f, 0.5f, 0.0f, 0.0f) { LeftPadding = 20 };
            left_align.Add (new CubanoTaskStatusButton ());

            track_info_align = new Alignment (0.5f, 0.5f, 0.0f, 0.0f) {
                TopPadding = 20,
                BottomPadding = 12,
                RightPadding = 10,
                LeftPadding = 10
            };
            
            track_info_align.Add (new CanvasHost () {
                Child = new ConnectedSeekableTrackInfoDisplay (),
                HeightRequest = 60,
                WidthRequest = 300,
                Visible = true
            });
            
            new CubanoTaskStatusButton () {
                IntermittentVisibility = false,
                ShowOnlyBackgroundTasks = false
            };
            
            footer.PackStart (left_align, true, true, 0);
            footer.PackStart (track_info_align, true, true, 0);
            
            var actions = ServiceManager.Get<InterfaceActionService> ().PlaybackActions;
            
            playback_align = new Alignment (1.0f, 0.5f, 0.0f, 0.0f) { RightPadding = 20 };
            var playback_box = new HBox ();
            // playback_box.PackStart (new RepeatActionButton ());
            playback_box.PackStart (actions["PreviousAction"].CreateToolItem (), false, false, 0);
            playback_box.PackStart (actions["PlayPauseAction"].CreateToolItem (), false, false, 0);
            playback_box.PackStart (new NextButton (ActionService), false, false, 0);
            playback_box.PackStart (new ConnectedVolumeButton (), false, false, 10);
            playback_align.Add (playback_box);
            footer.PackStart (playback_align, true, true, 0);
            
            footer.ShowAll ();
            primary_vbox.PackStart (footer, false, true, 0);
        }

        private void OnStatusBoxButtonPress (object o, ButtonPressEventArgs args) 
        {
            Source source = ServiceManager.SourceManager.ActiveSource;
            if (source != null) {
                source.CycleStatusFormat ();
                UpdateSourceInformation ();
            }
        }
        
        private void ConfigureMargins (bool zero)
        {
            if (zero) {
                view_box.LeftPadding 
                    = view_box.RightPadding
                    = header.TopPadding
                    = header.BottomPadding
                    = header.LeftPadding 
                    = header.RightPadding = 0;
            } else {
                view_box.LeftPadding = view_box.RightPadding = 25;
                header.BottomPadding = 20;
                header.TopPadding = 6;
                header.RightPadding = 7;
                header.LeftPadding = 20;
            }
        }

#endregion
        
#region Events and Logic Setup
        
        protected override void ConnectEvents ()
        {
            base.ConnectEvents ();

            // Service events
            ServiceManager.SourceManager.ActiveSourceChanged += OnActiveSourceChanged;
            ServiceManager.SourceManager.SourceUpdated += OnSourceUpdated;
            
            ActionService.TrackActions ["SearchForSameArtistAction"].Activated += OnProgrammaticSearch;
            ActionService.TrackActions ["SearchForSameAlbumAction"].Activated += OnProgrammaticSearch;

            // UI events
            header.SearchEntry.Entry.Changed += OnSearchEntryChanged;
        }
        
#endregion

#region Service Event Handlers

        private void OnProgrammaticSearch (object o, EventArgs args)
        {
            Source source = ServiceManager.SourceManager.ActiveSource;
            header.SearchEntry.Entry.Ready = false;
            header.SearchEntry.Entry.Query = source.FilterQuery;
            header.SearchEntry.Entry.Ready = true;
        }
        
        private Source previous_source = null;
        private TrackListModel previous_track_model = null;
        private void OnActiveSourceChanged (SourceEventArgs args)
        {
            Banshee.Base.ThreadAssist.ProxyToMain (delegate {
                Source source = ServiceManager.SourceManager.ActiveSource;
    
                header.SearchEntry.SearchSensitive = source != null && source.CanSearch;
                
                if (source == null) {
                    return;
                }
                
                header.SearchEntry.Entry.Ready = false;
                header.SearchEntry.Entry.CancelSearch ();
    
                if (source.FilterQuery != null) {
                    header.SearchEntry.Entry.Query = source.FilterQuery;
                    header.SearchEntry.Entry.ActivateFilter ((int)source.FilterType);
                }
    
                if (view_container.Content != null) {
                    view_container.Content.ResetSource ();
                }
    
                if (previous_track_model != null) {
                    previous_track_model.Reloaded -= HandleTrackModelReloaded;
                    previous_track_model = null;
                }
    
                if (source is ITrackModelSource) {
                    previous_track_model = (source as ITrackModelSource).TrackModel;
                    previous_track_model.Reloaded += HandleTrackModelReloaded;
                }
                
                if (previous_source != null) {
                    previous_source.Properties.PropertyChanged -= OnSourcePropertyChanged;
                }
                
                UpdateSourceHistory (previous_source, source);
                
                previous_source = source;
                previous_source.Properties.PropertyChanged += OnSourcePropertyChanged;
                
                UpdateSourceContents (source);
                
                UpdateSourceInformation ();
                header.SearchEntry.Entry.Ready = true;
            });
        }
        
        private void OnSourcePropertyChanged (object o, PropertyChangeEventArgs args)
        {
            if (args.PropertyName == "Nereid.SourceContents") {
                Banshee.Base.ThreadAssist.ProxyToMain (delegate {
                    UpdateSourceContents (previous_source);
                });
            }
        }
        
        private void UpdateSourceContents (Source source)
        {
            if (source == null) {
                return;
            }
            
            // Connect the source models to the views if possible
            ISourceContents contents = source.GetProperty<ISourceContents> ("Nereid.SourceContents",
                source.GetInheritedProperty<bool> ("Nereid.SourceContentsPropagate"));
                
            bool remove_margins = false;
                
            view_container.ClearFooter ();
            
            if (contents != null) {
                if (view_container.Content != contents) {
                    view_container.Content = contents;
                }
                view_container.Content.SetSource (source);
                view_container.Show ();
             
                remove_margins = contents is Cubano.NowPlaying.NowPlayingInterface || 
                    contents.GetType ().FullName == "Banshee.NowPlaying.NowPlayingInterface";
            } else if (source is ITrackModelSource) {
                view_container.Content = composite_view;
                view_container.Content.SetSource (source);
                view_container.Show ();
            } else if (source is Hyena.Data.IObjectListModel) {
                if (object_view == null) {
                    object_view = new ObjectListSourceContents ();
                }
                
                view_container.Content = object_view;
                view_container.Content.SetSource (source);
                view_container.Show ();
            } else {
                view_container.Hide ();
            }

            // Associate the view with the model
            if (view_container.Visible && view_container.Content is ITrackModelSourceContents) {
                ITrackModelSourceContents track_content = view_container.Content as ITrackModelSourceContents;
                source.Properties.Set<IListView<TrackInfo>>  ("Track.IListView", track_content.TrackView);
            }

            header.Visible = source.Properties.Contains ("Nereid.SourceContents.HeaderVisible") ?
                source.Properties.Get<bool> ("Nereid.SourceContents.HeaderVisible") : true;

            Widget footer_widget = null;
            if (source.Properties.Contains ("Nereid.SourceContents.FooterWidget")) {
                footer_widget = source.Properties.Get<Widget> ("Nereid.SourceContents.FooterWidget");
            }
            
            if (footer_widget != null) {
                view_container.SetFooter (footer_widget);
            }
            
            ConfigureMargins (remove_margins);
        }

        private void OnSourceUpdated (SourceEventArgs args)
        {
            if (args.Source == ServiceManager.SourceManager.ActiveSource) {
                Banshee.Base.ThreadAssist.ProxyToMain (delegate {
                    UpdateSourceInformation ();
                });
            }
        }

#endregion

#region UI Event Handlers
        
        private void OnSearchEntryChanged (object o, EventArgs args)
        {
            Source source = ServiceManager.SourceManager.ActiveSource;
            if (source == null)
                return;
            
            source.FilterType = (TrackFilterType)header.SearchEntry.Entry.ActiveFilterID;
            source.FilterQuery = header.SearchEntry.Entry.Query;
        }
        
#endregion

#region Implement Interfaces
/*
        // IHasSourceView
        public Source HighlightedSource {
            get { return source_view.HighlightedSource; }
        }

        public void BeginRenameSource (Source source)
        {
            source_view.BeginRenameSource (source);
        }
        
        public void ResetHighlight ()
        {
            source_view.ResetHighlight ();
        }*/

        public override Box ViewContainer {
            get { return view_container; }
        }

#endregion
        
#region Gtk.Window Overrides

        private bool accel_group_active = true;

        private void OnEntryFocusOutEvent (object o, FocusOutEventArgs args)
        {
            if (!accel_group_active) {
                AddAccelGroup (ActionService.UIManager.AccelGroup);
                accel_group_active = true;
            }

            (o as Widget).FocusOutEvent -= OnEntryFocusOutEvent;
        }
        
        protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
        {
            bool focus_search = false;
            
            if (Focus is Gtk.Entry && (GtkUtilities.NoImportantModifiersAreSet () && 
                evnt.Key != Gdk.Key.Control_L && evnt.Key != Gdk.Key.Control_R)) {
                if (accel_group_active) {
                    RemoveAccelGroup (ActionService.UIManager.AccelGroup);
                    accel_group_active = false;

                    // Reinstate the AccelGroup as soon as the focus leaves the entry
                    Focus.FocusOutEvent += OnEntryFocusOutEvent;
                 }
            } else {
                if (!accel_group_active) {
                    AddAccelGroup (ActionService.UIManager.AccelGroup);
                    accel_group_active = true;
                }
            }
            
            switch (evnt.Key) {
                case Gdk.Key.f:
                    if (Gdk.ModifierType.ControlMask == (evnt.State & Gdk.ModifierType.ControlMask)) {
                        focus_search = true;
                    }
                    break;

                case Gdk.Key.S:  case Gdk.Key.s:
                case Gdk.Key.F3: case Gdk.Key.slash:
                    focus_search = true;
                    break;
                case Gdk.Key.F11:
                    ActionService.ViewActions["FullScreenAction"].Activate ();
                    break;
                case Gdk.Key.F4:
                    main_menu.Visible = !main_menu.Visible;
                    break;
            }

            if (focus_search && !header.SearchEntry.Entry.HasFocus/* && !source_view.EditingRow*/) {
                header.SearchEntry.Entry.HasFocus = true;
                return true;
            }
            
            return base.OnKeyPressEvent (evnt);
        }

#endregion

#region Helper Functions

        private void HandleTrackModelReloaded (object sender, EventArgs args)
        {
            Banshee.Base.ThreadAssist.ProxyToMain (UpdateSourceInformation);
        }

        private void UpdateSourceInformation ()
        {
            Source source = ServiceManager.SourceManager.ActiveSource;
            if (source == null) {
                status_label.Text = String.Empty;
                return;
            }

            status_label.Text = source.GetStatusText ();
        }

#endregion

#region Cubano Theme/UI

        private void OnCubanoToolbarExposeEvent (object o, ExposeEventArgs args)
        {
            Toolbar toolbar = (Toolbar)o;

            // This forces the toolbar to look like it's just a regular part
            // of the window since the stock toolbar look makes Banshee look ugly.
            RenderBackground (toolbar.GdkWindow, args.Event.Region);

            // Manually expose all the toolbar's children
            foreach (Widget child in toolbar.Children) {
                toolbar.PropagateExpose (child, args.Event);
            }
        }
        
        protected override bool OnExposeEvent (Gdk.EventExpose evnt)
        {
            if (!Visible || !IsMapped) {
                return true;
            }
            
            RenderBackground (evnt.Window, evnt.Region);
            PropagateExpose (Child, evnt);
            return true;
        }
        
        private bool render_gradient = Environment.GetEnvironmentVariable ("CUBANO_DISABLE_BACKGROUND") == null;
        private bool render_debug = Environment.GetEnvironmentVariable ("CUBANO_RENDER_DEBUG") != null;
        private Random rand;
        
        private struct Circle { public double X; public double Y; public double R; public double A; }
        private Circle [] circles;
        
        private void RenderBackground (Gdk.Window window, Gdk.Region region)
        {
            rand = rand ?? new Random ();
        
            if (circles == null) {
                for (int i = 0, n = rand.Next (100, 150); i < n; i++) {
                    circles = circles ?? new Circle[n];
                    circles[i] = new Circle {
                        X = rand.Next (0, Screen.Width),
                        Y = rand.Next (0, Screen.Height),
                        R = rand.Next (10, 70),
                        A = rand.NextDouble () * 0.08
                    };
                }
            }
            
            Cairo.Context cr = Gdk.CairoHelper.Create (window);

            foreach (Gdk.Rectangle damage in region.GetRectangles ()) {
                cr.Rectangle (damage.X, damage.Y, damage.Width, damage.Height);
                cr.Clip ();
                
                cr.Translate (Allocation.X, Allocation.Y);
                cr.Rectangle (0, 0, Allocation.Width, Allocation.Height);
                
                if (render_gradient) {
                    var grad = new Cairo.LinearGradient (0, 0, 0, Allocation.Height);
                    grad.AddColorStop (0.7, CairoExtensions.GdkColorToCairoColor (Style.Base (StateType.Normal)));
                    grad.AddColorStop (1, CairoExtensions.GdkColorToCairoColor (Style.Background (StateType.Normal)));
                    
                    cr.Pattern = grad;
                    cr.Fill ();
                    grad.Destroy ();
                 
                    foreach (var circle in circles) {
                        cr.Color = new Cairo.Color (0, 0, 0, circle.A);
                        cr.Arc (circle.X + circle.R, circle.Y + circle.R, circle.R, 0, 2 * Math.PI);
                        cr.Fill ();
                    }
                } else {
                    cr.Color = new Cairo.Color (1, 1, 1);
                    cr.Fill ();
                }

                if (window_decorator != null) {
                    window_decorator.Render (cr);
                }
                
                if (render_debug) {
                    cr.LineWidth = 1.0;
                    cr.Color = CairoExtensions.RgbToColor (
                        (uint)rand.Next (0, 0xffffff));
                    cr.Rectangle (damage.X + 0.5, damage.Y + 0.5, damage.Width - 1, damage.Height - 1);
                    cr.Stroke ();
                }
                
                cr.ResetClip ();
            }
            
            CairoExtensions.DisposeContext (cr);
        }
        
#endregion

#region Source History

        private Hyena.UndoManager source_history;

        private void UpdateSourceHistory (Source oldSource, Source newSource)
        {
            if (source_history == null) {
                source_history = new Hyena.UndoManager ();
                // header.SourceForwardButton.Activated += (o, e) => source_history.Redo ();
                header.SourceBackButton.Activated += (o, e) => source_history.Undo ();
            }
            
            if (newSource != null && oldSource != null) {
                source_history.AddUndoAction (new SourceUndoAction (oldSource, newSource));
            }
            
            // header.SourceForwardButton.Sensitive = source_history.CanRedo;
            header.SourceBackButton.Sensitive = source_history.CanUndo;
        }
        
        private class SourceUndoAction : Hyena.IUndoAction
        {
            private Source new_source;
            private Source old_source;
            
            public SourceUndoAction (Source oldSource, Source newSource)
            {
                this.new_source = newSource;
                this.old_source = oldSource;
            }
            
            public void Undo ()
            {
                if (ServiceManager.SourceManager.ContainsSource (old_source)) {
                    ServiceManager.SourceManager.SetActiveSource (old_source);
                }
            }
            
            public void Redo ()
            {
                if (ServiceManager.SourceManager.ContainsSource (new_source)) {
                    ServiceManager.SourceManager.SetActiveSource (new_source);
                }
            }
            
            public bool CanMerge (Hyena.IUndoAction action)
            {
                return false;
            }
            
            public void Merge (Hyena.IUndoAction action)
            {
            }
        }

#endregion

        IDBusExportable IDBusExportable.Parent {
            get { return null; }
        }
        
        string IDBusObjectName.ExportObjectName {
            get { return "CubanoWindow"; }
        }

        string IService.ServiceName {
            get { return "CubanoPlayerInterface"; }
        }
    }
}
