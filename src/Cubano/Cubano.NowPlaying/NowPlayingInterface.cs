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
        
        public NowPlayingInterface ()
        {
            display = new Embed ();
            display.Show ();
            PackStart (display, true, true, 0);
            
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
            if (stage == null) {
                stage = new NowPlayingStage () { Visible = true };
                display.Stage.Color = new Color (0, 0, 0);
                display.Stage.Add (stage);
            }
            
            Show ();
        }
        
        public void DeactivateDisplay ()
        {
            if (stage != null) {
               // display.Stage.Remove (stage);
            }
            
            Hide ();
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
