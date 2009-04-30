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
        // Major Layout Components
        private VBox primary_vbox;
        private Alignment header_box;
        private Alignment view_box;
        private Toolbar header_toolbar;
        private Toolbar footer_toolbar;
        private HBox footer;
        private ViewContainer view_container;
        private SearchEntry search_box;
        private MainMenu main_menu;
        private ToolButton forward_button;
        private ToolButton back_button;
        private Alignment playback_align;
        private Alignment source_align;
        private Alignment track_info_align;
        private CubanoVisualizer visualizer;
        private CubanoWindowDecorator window_decorator;
        
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
            ConfigureTheme ();
        
            BuildPrimaryLayout ();
            ConnectEvents ();

            // ActionService.SourceActions.SourceView = this;
            
            composite_view.TrackView.HasFocus = true;
            
            InitialShowPresent ();
            
            visualizer = new CubanoVisualizer ();
            visualizer.RenderRequest += (o, e) => {
                var damage = e.Damage;
                if (damage.Width == 0 || damage.Height == 0) {
                    damage = new Gdk.Rectangle (
                        0,
                        0, 
                        Allocation.Width, 
                        visualizer.Height
                    );
                }
                
                QueueDrawArea (
                    damage.X,
                    Allocation.Height - damage.Height,
                    damage.Width,
                    damage.Height
                );
            };
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
            // Header Bar
            var title_box = new HBox ();
            title_box.Spacing = 15;
            title_box.PackEnd (new CanvasHost () { Child = new CubanoMainMenu () }, false, false, 0);
            title_box.PackEnd (search_box = new SearchEntry (), false, false, 0);
            title_box.ShowAll ();
            
            // Tool Bar
            var header_toolbar = (Toolbar)ActionService.UIManager.GetWidget ("/HeaderToolbar");
            header_toolbar.ExposeEvent += OnCubanoToolbarExposeEvent;
            header_toolbar.ShowArrow = false;
            header_toolbar.ToolbarStyle = ToolbarStyle.BothHoriz;
            
            var children = header_toolbar.Children;
            header_toolbar.Insert (new GenericToolItem<Widget> (new RepeatActionButton ()), 0);
            
            var filler = new Alignment (0.5f, 0.5f, 0.0f, 0.0f);
            ActionService.PopulateToolbarPlaceholder (header_toolbar, "/HeaderToolbar/TrackInfoDisplay", filler, true);
            
            header_toolbar.ShowAll ();
            for (int i = 0; i < children.Length; i++) {
                children[i].Visible = false;
            }
            
            // Pack it in
            var box = new VBox ();
            box.Show ();
            box.PackStart (title_box, false, false, 0);
            box.PackStart (header_toolbar, false, false, 0);
            
            header_box = new Alignment (0.0f, 0.0f, 1.0f, 1.0f) {
                LeftPadding = 10,
                RightPadding = 10,
                TopPadding = 10
            };
            header_box.Add (box);
            header_box.Show ();
            
            primary_vbox.PackStart (header_box, false, false, 0);
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
            
            /*footer_toolbar = (Toolbar)ActionService.UIManager.GetWidget ("/FooterToolbar");
            footer_toolbar.ShowArrow = false;
            footer_toolbar.ToolbarStyle = ToolbarStyle.BothHoriz;

            Widget task_status = new Banshee.Gui.Widgets.TaskStatusIcon ();

            EventBox status_event_box = new EventBox ();
            status_event_box.ButtonPressEvent += OnStatusBoxButtonPress;
            
            status_event_box.Add (status_label);

            HBox status_hbox = new HBox (true, 0);
            status_hbox.PackStart (status_event_box, false, false, 0);
            
            Alignment status_align = new Alignment (0.5f, 0.5f, 1.0f, 1.0f);
            status_align.Add (status_hbox);

            RepeatActionButton repeat_button = new RepeatActionButton ();
            repeat_button.SizeAllocated += delegate (object o, Gtk.SizeAllocatedArgs args) {
                status_align.LeftPadding = (uint)args.Allocation.Width;
            };

          //  ActionService.PopulateToolbarPlaceholder (footer_toolbar, "/FooterToolbar/TaskStatus", task_status, false);
            ActionService.PopulateToolbarPlaceholder (footer_toolbar, "/FooterToolbar/StatusBar", status_align, true);
            ActionService.PopulateToolbarPlaceholder (footer_toolbar, "/FooterToolbar/RepeatButton", repeat_button);

            footer_toolbar.ShowAll ();
            primary_vbox.PackStart (footer_toolbar, false, true, 0);*/
            
            footer = new HBox ();
            
            source_align = new Alignment (0.0f, 0.5f, 0.0f, 0.0f) { LeftPadding = 20 };
            source_align.SizeAllocated += OnFooterGroupSizeAllocated;
            var source_box = new HBox ();
            var source_combo = new SourceComboBox ();
            /*source_combo.ExposeEvent += (o, e) => {
                RenderBackground (e.Event.Window, e.Event.Region);
                source_combo.Cells[0].Render (e.Event.Window, source_combo, 
                    source_combo.Allocation, 
                    source_combo.Allocation,
                    source_combo.Allocation,
                    CellRendererState.Focused);
            };*/
            source_box.PackStart (back_button = new Gtk.ToolButton (Stock.GoBack), false, false, 0);
            source_box.PackStart (source_combo, false, false, 0);
            forward_button = new Gtk.ToolButton (Stock.GoForward);
            //source_box.PackStart (forward_button = new Gtk.ToolButton (Stock.GoForward), false, false, 0);
            source_align.Add (source_box);
            
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
            
            footer.PackStart (source_align, false, false, 0);
            footer.PackStart (track_info_align, true, true, 0);
            
            var actions = ServiceManager.Get<InterfaceActionService> ().PlaybackActions;
            
            playback_align = new Alignment (0.0f, 0.5f, 0.0f, 0.0f) { RightPadding = 20 };
            playback_align.SizeAllocated += OnFooterGroupSizeAllocated;
            var playback_box = new HBox ();
            playback_box.PackStart (actions["PreviousAction"].CreateToolItem (), false, false, 0);
            playback_box.PackStart (actions["PlayPauseAction"].CreateToolItem (), false, false, 0);
            playback_box.PackStart (new NextButton (ActionService), false, false, 0);
            playback_box.PackStart (new ConnectedVolumeButton (), false, false, 10);
            playback_align.Add (playback_box);
            footer.PackEnd (playback_align, false, false, 0);
            
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
                    = header_box.TopPadding
                    = header_box.BottomPadding
                    = header_box.LeftPadding 
                    = header_box.RightPadding = 0;
            } else {
                view_box.LeftPadding = view_box.RightPadding = 25;
                header_box.TopPadding = 15;
                header_box.BottomPadding = 20;
                header_box.LeftPadding = header_box.RightPadding = 25;
            }
        }
        
        private void OnFooterGroupSizeAllocated (object o, SizeAllocatedArgs args)
        {
            //track_info_align.Xalign = 1.0f -
            //    (float)Math.Min (source_align.Allocation.Width, playback_align.Allocation.Width) /
            //    (float)Math.Max (source_align.Allocation.Width, playback_align.Allocation.Width);
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
            search_box.Entry.Changed += OnSearchEntryChanged;
        }
        
#endregion

#region Service Event Handlers

        private void OnProgrammaticSearch (object o, EventArgs args)
        {
            Source source = ServiceManager.SourceManager.ActiveSource;
            search_box.Entry.Ready = false;
            search_box.Entry.Query = source.FilterQuery;
            search_box.Entry.Ready = true;
        }
        
        private Source previous_source = null;
        private TrackListModel previous_track_model = null;
        private void OnActiveSourceChanged (SourceEventArgs args)
        {
            Banshee.Base.ThreadAssist.ProxyToMain (delegate {
                Source source = ServiceManager.SourceManager.ActiveSource;
    
                search_box.SearchSensitive = source != null && source.CanSearch;
                
                if (source == null) {
                    return;
                }
                
                search_box.Entry.Ready = false;
                search_box.Entry.CancelSearch ();
    
                if (source.FilterQuery != null) {
                    search_box.Entry.Query = source.FilterQuery;
                    search_box.Entry.ActivateFilter ((int)source.FilterType);
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
                search_box.Entry.Ready = true;
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
             
                remove_margins = contents.GetType ().FullName == "Banshee.NowPlaying.NowPlayingInterface";
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

            header_box.Visible = source.Properties.Contains ("Nereid.SourceContents.HeaderVisible") ?
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
            
            source.FilterType = (TrackFilterType)search_box.Entry.ActiveFilterID;
            source.FilterQuery = search_box.Entry.Query;
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

            if (focus_search && !search_box.Entry.HasFocus/* && !source_view.EditingRow*/) {
                search_box.Entry.HasFocus = true;
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

        private void ConfigureTheme ()
        {
            // Banshee 1.4.3 and older does not provide a way for Hyena controls
            // to use any other theme except the GtkTheme; a hack was added for 1.4.3+
            // that allows reflection to be used to specify a theme provider; this allows
            // Cubano to run on 1.4.x but will look different on 1.4.3 or older;
            // This is the client side of the hack. Sigh.
            
            var asm = typeof (Hyena.Gui.Theming.Theme).Assembly;
            var engine_type = asm.GetType ("Hyena.Gui.Theming.ThemeEngine");
            if (engine_type != null) {
                var provider_method = engine_type.GetMethod ("SetProvider");
                if (provider_method != null) {
                    provider_method.Invoke (null, new object [] { new EventHandler (OnProvideTheme) });
                }
            }
        }
        
        private void OnProvideTheme (object o, EventArgs args)
        {
            var type = args.GetType ();
            var theme_prop = type.GetProperty ("Theme");
            var widget_prop = type.GetProperty ("Widget");
            if (theme_prop != null && widget_prop != null) {
                theme_prop.SetValue (args, new CubanoTheme ((Widget)widget_prop.GetValue (args, null)), null);
            }
        }
        
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
        
        private void RenderBackground (Gdk.Window window, Gdk.Region region)
        {   
            Cairo.Context cr = Gdk.CairoHelper.Create (window);

            foreach (Gdk.Rectangle damage in region.GetRectangles ()) {
                cr.Rectangle (damage.X, damage.Y, damage.Width, damage.Height);
                cr.Clip ();
                
                cr.Translate (Allocation.X, Allocation.Y);
                cr.Rectangle (0, 0, Allocation.Width, Allocation.Height);
                
                if (render_gradient) {
                    var grad = new Cairo.LinearGradient (0, 0, 0, Allocation.Height);
                    grad.AddColorStop (0.00, new Cairo.Color (210 / 255.0, 210 / 255.0, 210 / 255.0));
                    grad.AddColorStop (0.20, new Cairo.Color (245 / 255.0, 245 / 255.0, 245 / 255.0));
                    grad.AddColorStop (0.78, new Cairo.Color (253 / 255.0, 253 / 255.0, 253 / 255.0));
                    grad.AddColorStop (1.00, new Cairo.Color (250 / 255.0, 250 / 255.0, 250 / 255.0));
                    cr.Pattern = grad;
                    cr.Fill ();
                    grad.Destroy ();
                } else {
                    cr.Color = new Cairo.Color (1, 1, 1);
                    cr.Fill ();
                }
               
                if (visualizer != null) {
                    cr.Save ();
                    cr.Translate (0, Allocation.Height - visualizer.Height);
                    visualizer.Width = Allocation.Width;
                    visualizer.Render (cr);
                    cr.Restore ();
                }
                
                if (window_decorator != null) {
                    window_decorator.Render (cr);
                }
                
                if (render_debug) {
                    cr.LineWidth = 1.0;
                    cr.Color = CairoExtensions.RgbToColor (
                        (uint)(rand = rand ?? new Random ()).Next (0, 0xffffff));
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
                forward_button.Clicked += (o, e) => source_history.Redo ();
                back_button.Clicked += (o, e) => source_history.Undo ();
            }
            
            if (newSource != null && oldSource != null) {
                source_history.AddUndoAction (new SourceUndoAction (oldSource, newSource));
            }
            
            forward_button.Sensitive = source_history.CanRedo;
            back_button.Sensitive = source_history.CanUndo;
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
