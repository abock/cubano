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

using Banshee.ServiceStack;
using Banshee.Sources;
using Banshee.Gui;
using Banshee.Sources.Gui;

using Clutter;

namespace Cubano.NowPlaying
{
    public class NowPlayingInterface : VBox, ISourceContents
    {   
        private NowPlayingSource source;

        private Embed display;
        private Stage stage;
        private Texture video_texture;
        private Rectangle visualizer_texture;
        
        public NowPlayingInterface ()
        {
            Clutter.Application.InitForToolkit ();
        
            var engine = ServiceManager.PlayerEngine.ActiveEngine;
            if (engine != null && engine.GetType ().FullName == "Banshee.GStreamer.PlayerEngine") {
                var enable_clutter = engine.GetType ().GetMethod ("EnableClutterVideoSink");
                if (enable_clutter == null) {
                    throw new ApplicationException ("Banshee GStreamer engine does not have Clutter support");
                }
                
                video_texture = new Texture ();
                video_texture.SizeChange += OnVideoTextureSizeChange;
                video_texture.SyncSize = false;
                
                visualizer_texture = new Rectangle (new Color (0, 0, 0, 0));
                visualizer_texture.Painted += OnVisualizerPaintEvent;
                
                enable_clutter.Invoke (engine, new object [] { video_texture.Handle });
        
                ServiceManager.SourceManager.SourceAdded += OnSourceAdded;
            }
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
        
        private void OnVideoTextureSizeChange (object o, SizeChangeArgs args)
        {
            ReallocateVideoTexture (args.Width, args.Height);
        }
        
        private void ReallocateVideoTexture (int textureWidth, int textureHeight)
        {
            int stage_width, stage_height;
            
            stage.GetSize (out stage_width, out stage_height);
            
            int new_x, new_y, new_width, new_height;
            
            new_height = (textureHeight * stage_width) / textureWidth;
            if (new_height <= stage_height) {
                new_width = stage_width;
                new_x = 0;
                new_y = (stage_height - new_height) / 2;
            } else {
                new_width = (textureWidth * stage_height) / textureHeight;
                new_height = stage_height;
                new_x = (stage_width - new_width) / 2;
                new_y = 0;
            }
            
            video_texture.SetPosition (new_x, new_y);
            video_texture.SetSize (new_width, new_height);
        }
        
        protected override void OnSizeAllocated (Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated (allocation);
            
            int texture_width, texture_height;
            video_texture.GetSize (out texture_width, out texture_height);
            if (texture_width > 0 && texture_height > 0) {
                ReallocateVideoTexture (texture_width, texture_height);
            }
            
            visualizer_texture.SetSize (allocation.Width, 100);
            visualizer_texture.SetPosition (0, (int)(allocation.Height - visualizer_texture.Height));
        }
        
        public void ActivateDisplay ()
        {
            if (display != null) {
                DeactivateDisplay ();
            }
            
            display = new Embed ();
            stage = display.Stage;
            stage.Color = new Color (0, 0, 0);
            stage.Add (video_texture);
            stage.Add (visualizer_texture);
            
            PackStart (display, true, true, 0);
            
            display.Show ();
            Show ();
        }
        
        public void DeactivateDisplay ()
        {
            if (stage != null) {
                stage.Remove (video_texture);
                stage.Remove (visualizer_texture);
                stage = null;
            }
            
            if (display != null) {
                Remove (display);
                display.Dispose ();
                display = null;
            }
            
            Hide ();
        }      
        
#region Visualizer

        private void OnVisualizerPaintEvent (object o, System.EventArgs args)
        {
            Cogl.General.PushMatrix ();

            Cogl.Path.RoundRectangle (0, 0, visualizer_texture.Width, visualizer_texture.Height, 10, 5);
            
            Cogl.Path.Ellipse (0, 0, 60, 40);
            Cogl.General.SetSourceColor4ub (200, 0, 0, 128);
            Cogl.Path.Fill ();

            Cogl.General.PopMatrix ();
        }

#endregion
        
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
