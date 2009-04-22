//
// CubanoSourceContents.cs
//
// Authors:
//   Aaron Bockover <abockover@novell.com>
//   Gabriel Burt <gburt@novell.com>
//
// Copyright (C) 2007-2009 Novell, Inc.
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
using System.Reflection;
using System.Collections.Generic;

using Gtk;
using Mono.Unix;

using Hyena.Data;
using Hyena.Data.Gui;
using Hyena.Widgets;

using Banshee.Sources;
using Banshee.ServiceStack;
using Banshee.Collection;
using Banshee.Configuration;
using Banshee.Gui;
using Banshee.Collection.Gui;
using Banshee.Sources.Gui;

using ScrolledWindow=Gtk.ScrolledWindow;

namespace Cubano.Client
{
    public abstract class CubanoSourceContents : VBox, ISourceContents
    {
        private string name;
        private object main_view;
        private Gtk.ScrolledWindow main_scrolled_window;
        
        private List<object> filter_views = new List<object> ();
        private List<ScrolledWindow> filter_scrolled_windows = new List<ScrolledWindow> ();
        
        private Dictionary<object, double> model_positions = new Dictionary<object, double> ();
        
        private Paned container;
        private Widget browser_container;

        public CubanoSourceContents (string name)
        {
            this.name = name;
            InitializeViews ();
            Layout ();
            NoShowAll = true;
            
            ServiceManager.SourceManager.ActiveSourceChanged += delegate {
                Banshee.Base.ThreadAssist.ProxyToMain (delegate {
                    browser_container.Visible = ActiveSourceCanHasBrowser;
                });
            };
        }
        
        protected abstract void InitializeViews ();
        
        protected void SetupMainView<T> (ListView<T> main_view)
        {
            this.main_view = main_view;
            main_scrolled_window = SetupView (main_view);
        }
        
        protected void SetupFilterView<T> (ListView<T> filter_view)
        {
            ScrolledWindow window = SetupView (filter_view);
            filter_scrolled_windows.Add (window);
            filter_view.HeaderVisible = false;
            filter_view.SelectionProxy.Changed += OnBrowserViewSelectionChanged;
        }
        
        private ScrolledWindow SetupView (Widget view)
        {
            ScrolledWindow window = null;

            //if (!Banshee.Base.ApplicationContext.CommandLine.Contains ("no-smooth-scroll")) {
            if (Banshee.Base.ApplicationContext.CommandLine.Contains ("smooth-scroll")) {
                window = new SmoothScrolledWindow ();
            } else {
                window = new ScrolledWindow ();
            }
            
            window.Add (view);
            window.HscrollbarPolicy = PolicyType.Automatic;
            window.VscrollbarPolicy = PolicyType.Automatic;

            return window;
        }
        
        private void Reset ()
        {
            // Unparent the views' scrolled window parents so they can be re-packed in 
            // a new layout. The main container gets destroyed since it will be recreated.
            
            foreach (ScrolledWindow window in filter_scrolled_windows) {
                Paned filter_container = window.Parent as Paned;
                if (filter_container != null) {
                    filter_container.Remove (window);
                }
            }

            if (container != null && main_scrolled_window != null) {
                container.Remove (main_scrolled_window);
            }
            
            if (container != null) {
                Remove (container);
            }
        }

        private void Layout ()
        {
            //Hyena.Log.Information ("ListBrowser LayoutLeft");
            Reset ();
            
            container = new HPaned ();
            Paned filter_box = new HPaned ();
            filter_box.PositionSet = true;
            Paned current_pane = filter_box;
            
            for (int i = 0; i < filter_scrolled_windows.Count; i++) {
                ScrolledWindow window = filter_scrolled_windows[i];
                bool last_even_filter = (i == filter_scrolled_windows.Count - 1 && filter_scrolled_windows.Count % 2 == 0);
                if (i > 0 && !last_even_filter) {
                    Paned new_pane = new HPaned ();
                    current_pane.Pack2 (new_pane, true, false);
                    current_pane.Position = 350;
                    PersistentPaneController.Control (current_pane, ControllerName (i));
                    current_pane = new_pane;
                }
               
                if (last_even_filter) {
                    current_pane.Pack2 (window, true, false);
                    current_pane.Position = 350;
                    PersistentPaneController.Control (current_pane, ControllerName (i));
                } else {
                    /*if (i == 0)
                        current_pane.Pack1 (window, false, false);
                    else*/
                        current_pane.Pack1 (window, true, false);
                }
                    
            }
            
            container.Pack1 (filter_box, true, false);
            container.Pack2 (main_scrolled_window, true, false);
            browser_container = filter_box;
            
            container.Position = 275;
            PersistentPaneController.Control (container, ControllerName (-1));
            ShowPack ();
        }
        
        private string ControllerName (int filter)
        {
            return filter == -1 
                ? String.Format ("{0}.browser", name)
                : String.Format ("{0}.browser.{1}", name, filter);
        }

        private void ShowPack ()
        {
            PackStart (container, true, true, 0);
            NoShowAll = false;
            ShowAll ();
            NoShowAll = true;
            browser_container.Visible = true;
        }
        
        protected abstract void ClearFilterSelections ();
        
        protected virtual void OnBrowserViewSelectionChanged (object o, EventArgs args)
        {
            // If the All item is now selected, scroll to the top
            Hyena.Collections.Selection selection = (Hyena.Collections.Selection) o;
            if (selection.AllSelected) {
                // Find the view associated with this selection; a bit yuck; pass view in args?
                foreach (IListView view in filter_views) {
                    if (view.Selection == selection) {
                        view.ScrollTo (0);
                        break;
                    }
                }
            }
        }

        protected void SetModel<T> (IListModel<T> model)
        {
            ListView<T> view = FindListView <T> ();
            if (view != null) {
                SetModel (view, model);
            } else {
                Hyena.Log.DebugFormat ("Unable to find view for model {0}", model);
            }
        }
        
        protected void SetModel<T> (ListView<T> view, IListModel<T> model)
        {
            if (view.Model != null) {
                model_positions[view.Model] = view.Vadjustment.Value;
            }

            if (model == null) {
                view.SetModel (null);
                return;
            }
            
            if (!model_positions.ContainsKey (model)) {
                model_positions[model] = 0.0;
            }
            
            view.SetModel (model, model_positions[model]);
        }
        
        private ListView<T> FindListView<T> ()
        {
            if (main_view is ListView<T>)
                return (ListView<T>) main_view;
        
            foreach (object view in filter_views)
                if (view is ListView<T>)
                    return (ListView<T>) view;

            return null;
        }
        
        protected virtual string ForcePosition {
            get { return null; }
        }

        protected abstract bool ActiveSourceCanHasBrowser { get; }

#region Implement ISourceContents

        protected ISource source;

        public abstract bool SetSource (ISource source);

        public abstract void ResetSource ();

        public ISource Source {
            get { return source; }
        }

        public Widget Widget {
            get { return this; }
        }

#endregion

    }
}
