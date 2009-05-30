//
// NowPlayingInterface.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright 2008-2009 Novell, Inc.
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

using Mono.Unix;
using Gtk;

using Banshee.MediaEngine;
using Banshee.ServiceStack;
using Banshee.Sources;
using Banshee.Gui;
using Banshee.Sources.Gui;

using Clutter;

namespace Cubano.NowPlaying
{
    public class NowPlayingInterface : VBox, ISourceContents
    {
        private static readonly int visualizer_height = 120;
    
        private NowPlayingSource source;

        private Embed display;
        private NowPlayingStage stage;
        private Texture video_texture;
        
        public NowPlayingInterface ()
        {
            display = new Embed ();
            display.Show ();

            ServiceManager.PlayerEngine.EngineBeforeInitialize += engine => {
                var clutter_engine = ServiceManager.PlayerEngine.ActiveEngine as ISupportClutter;
                if (clutter_engine == null) {
                    throw new ApplicationException ("Banshee GStreamer engine does not have Clutter support");
                }

                video_texture = new Texture () { SyncSize = false };
                clutter_engine.EnableClutterVideoSink (video_texture.Handle);
            };
            
            ServiceManager.SourceManager.SourceAdded += OnSourceAdded;
        }
        
        private void OnSourceAdded (SourceAddedArgs args)
        {
            if (args.Source.GetType ().FullName == "Banshee.NowPlaying.NowPlayingSource") {
                ((IDisposable)args.Source).Dispose ();
                
                Gtk.Application.Invoke (delegate {
                    ServiceManager.SourceManager.RemoveSource (args.Source, true);
                });
            }
        }
        
        protected override void OnSizeAllocated (Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated (allocation);
            
            if (stage != null) {
                stage.SetPosition (0, 0);
                stage.SetSize (Allocation.Width, Allocation.Height);
            }
        }
        
        public void ActivateDisplay ()
        {
            // Create the stage the first time we are activated
            // This is not done when the source is created, even
            // though it would avoid a bit of flickering, to help
            // decrease startup time
            if (stage == null) {
                stage = new NowPlayingStage (video_texture) {
                    Visible = true
                };
                display.Stage.Color = new Color (0, 0, 0);
                display.Stage.Add (stage);
            }
            
            // The clutter embed gets reparented to the toplevel
            // child container when we go away, so if it's the
            // first time, we pack it, otherwise we reparent
            if (display.Parent == null) {
                PackStart (display, true, true, 0);
            } else {
                display.Reparent (this);
            }

            // Our stage supports pausing/resuming so when it's
            // not visible, we're not running timeouts/animations
            display.Show ();
            stage.Resume ();
        }
        
        public void DeactivateDisplay ()
        {
            // We're going away, so stop any clock work
            if (stage != null) {
                stage.Pause ();
            }
            
            // The clutter embed must never be unrealized, otherwise
            // the x server will freak out and abort us if data is
            // being rendered to the GL window inside the embed.
            //
            // To work around this, we reparent the embed to the
            // toplevel window's child container (there better be one).
            // This prevents GTK from unrealizing the window during
            // the reparent, and instead it just unmaps.
            var top_window = Toplevel as Gtk.Window;
            if (top_window != null) {
                var container = top_window.Child as Gtk.Container;
                if (container != null) {
                    display.Hide ();
                    display.Reparent (container);
                }
            }
        }
        
#region ISourceContents
        
        public bool SetSource (ISource src)
        {
            this.source = source as NowPlayingSource;
            if (display != null) {
                display.Show ();
            }
            return this.source != null;
        }

        public ISource Source {
            get { return source; }
        }

        public void ResetSource ()
        {
            source = null;
        }

        public Widget Widget {
            get { return this; }
        }
        
#endregion

    }
}
