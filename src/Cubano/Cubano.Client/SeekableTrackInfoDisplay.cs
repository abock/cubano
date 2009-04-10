//
// SeekableTrackInfoDisplay.cs
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
using Gdk;
using Gtk;
using Cairo;

using Hyena.Gui;
using Hyena.Gui.Theming;
using Hyena.Gui.Theatrics;
using Hyena.Gui.Canvas;

using Banshee.Collection;
using Banshee.Collection.Gui;

namespace Banshee.Gui.Widgets
{
    public class SeekableTrackInfoDisplay : ClassicTrackInfoDisplay
    {
        private uint transition_timeout_id;
        private int text_y;
        private Pango.Layout text_layout;
        private GtkTheme theme;
        private CanvasSeekSlider slider;
        private CanvasPositionLabel position_label;
        
        public SeekableTrackInfoDisplay () : base ()
        {
            children.Add (slider = new CanvasSeekSlider () {
                PaddingTop = 4,
                PaddingBottom = 4
            });
            
            children.Add (position_label = new CanvasPositionLabel (this));
            
            foreach (ICanvasItem child in children) {
                child.Rerender += (o, e) => QueueDraw ();
            }
        }
        
        protected SeekableTrackInfoDisplay (IntPtr native) : base (native)
        {
        }
        
        protected override void OnRealized ()
        {
            base.OnRealized ();
            
            Gdk.Window [] children = GdkWindow.Children;
            for (int i = 0; i < children.Length; i++) {
                children[i].Events |= EventMask.ButtonPressMask | EventMask.ButtonReleaseMask;
            }
        }
        
        protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
        {
            double x = evnt.X + Allocation.X, y = evnt.Y + Allocation.Y;
            WithChildAt (x, y, child => child.ButtonPress (x - child.Left, y - child.Top, evnt.Button));
            return base.OnButtonPressEvent (evnt);
        }
        
        protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
        {
            double x = evnt.X + Allocation.X, y = evnt.Y + Allocation.Y;
            WithChildAt (x, y, child => child.ButtonRelease ());
            return base.OnButtonReleaseEvent (evnt);
        }
        
        protected override bool OnMotionNotifyEvent (EventMotion evnt)
        {
            double x = evnt.X + Allocation.X, y = evnt.Y + Allocation.Y;
            WithChildAt (x, y, child => child.PointerMotion (x - child.Left, y - child.Top));
            return base.OnMotionNotifyEvent (evnt);
        }
        
        protected override void OnMapped ()
        {
            base.OnMapped ();

            if (transition_timeout_id == 0) {
                transition_timeout_id = GLib.Timeout.Add (4000, OnTransitionTimeout);
            }
        }

        protected override void OnUnmapped ()
        {
            base.OnUnmapped ();

            if (transition_timeout_id > 0) {
                GLib.Source.Remove (transition_timeout_id);
                transition_timeout_id = 0;
            }
        }
        
        protected override void OnThemeChanged ()
        {
            theme = new GtkTheme (this);
            foreach (ICanvasItem child in children) {
                child.Theme = theme;
            }
        
            if (text_layout != null) {
                text_layout.Dispose ();
                text_layout = null;
            }
        }
        
        protected override void OnSizeRequested (ref Requisition requisition)
        {
            double slider_width, slider_height;
            slider.SizeRequest (out slider_width, out slider_height);
            requisition.Height = (int)Math.Ceiling (2 * ComputeTextHeight () + slider_height);
        }
        
        protected override void OnSizeAllocated (Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated (allocation);
            
            slider.Left = ContentXOffset;
            slider.Top = Allocation.Y + text_y;
            slider.Width = ContentWidth;
            
            position_label.Left = slider.Left;
            position_label.Top = slider.Top + slider.Height;
            position_label.Width = slider.Width;
        }

        private int ComputeTextHeight ()
        {
            using (var metrics = PangoContext.GetMetrics (Style.FontDescription, PangoContext.Language)) {
                return ((int)(metrics.Ascent + metrics.Descent) + 512) >> 10; // PANGO_PIXELS(d)
            }
        }
        
        private void EnsureLayout (Context cr)
        {
            if (text_layout == null) {
                text_layout = CairoExtensions.CreateLayout (this, cr);
                text_layout.Ellipsize = Pango.EllipsizeMode.End;
                text_y = ComputeTextHeight ();
            }
        }

        private static int transition_states = 3;
        private bool transition_actor_reset;
        private int transition_index;
        private SingleActorStage transition_stage;
        
        private bool OnTransitionTimeout ()
        {
            if (transition_stage == null) {
                transition_stage = new SingleActorStage ();
                transition_stage.Iteration += OnTransitionIteration;
            }

            transition_actor_reset = true;
            transition_stage.Reset ();
            return true;
        }

        private void OnTransitionIteration (object o, EventArgs args)
        {
            if (transition_stage.Actor != null && transition_actor_reset && transition_stage.Actor.Percent >= 0.5) {
                transition_actor_reset = false;
                transition_index = (transition_index + 1) % transition_states;
            }
            
            Invalidate ();
        }

        protected override void RenderIdle (Context cr)
        {
            RenderSlider (cr);
        }
        
        protected override void RenderTrackInfo (Context cr, TrackInfo track, bool renderTrack, bool renderArtistAlbum)
        {
            RenderSlider (cr);
            
            if (track == null) {
                return;
            }
            
            double x = ContentXOffset;
            double y = Allocation.Y;
            double width = ContentWidth;
            int pango_width = (int)(width * Pango.Scale.PangoScale);
            
            string display = String.Empty;
            switch (transition_index) {
                case 0: display = track.DisplayTrackTitle; break;
                case 1: display = track.DisplayArtistName; break;
                case 2: display = track.DisplayAlbumTitle; break;
            }

            text_layout.Width = pango_width;
            text_layout.SetMarkup (display);
            
            double percent = transition_stage != null && transition_stage.Actor != null 
                ? transition_stage.Actor.Percent : 1.0;
            
            Cairo.Color color = TextColor;
            color.A = percent <= 0.5
                ? 1.0 - (percent * 2.0)
                : (percent - 0.5) * 2.0;
            
            cr.Color = color;
            cr.MoveTo (x, y);
            cr.Antialias = Cairo.Antialias.Default;
            
            PangoCairoHelper.ShowLayout (cr, text_layout);
        }

        private void RenderSlider (Context cr)
        {
            EnsureLayout (cr);
            
            foreach (ICanvasItem child in children) {
                child.Render (cr);
            }
        }

        private const double ArtworkSpacing = 10;

        private double ContentXOffset {
            get { return Allocation.X + Allocation.Height + ArtworkSpacing; }
        }

        private double ContentWidth {
            get { return Allocation.Width - (Allocation.Height + ArtworkSpacing); }
        }
        
        protected override bool CanRenderIdle {
            get { return true; }
        }
        
#region Should be CanvasContainer : ICanvasItem

        private List<ICanvasItem> children = new List<ICanvasItem> ();
        
        public delegate void CanvasItemHandler (ICanvasItem item);
        
        private ICanvasItem GetChildAt (double x, double y)
        {   
            foreach (ICanvasItem child in children) {
                if (x >= child.Left && x <= child.Left + child.Width &&
                    y >= child.Top && y <= child.Top + child.Height) {
                    return child;
                }
            }
            
            return null;
        }
        
        private ICanvasItem WithChildAt (double x, double y, CanvasItemHandler handler)
        {
            ICanvasItem child = GetChildAt (x, y);
            if (child != null && handler != null) {
                handler (child);
            }
            return child;
        }
        
#endregion

    }
}
