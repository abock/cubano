// 
// CanvasPositionLabel.cs
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
using Cairo;
using Mono.Unix;

using Hyena.Gui;
using Hyena.Gui.Theming;
using Hyena.Gui.Canvas;

using Banshee.Widgets;

namespace Banshee.Gui.Widgets
{
    public class CanvasPositionLabel : CanvasLabel
    {
        private double duration;
        private double position;
        private double buffering_progress;
        private bool is_live;
        private Pango.Layout layout;
        private StreamLabelState state;
        private string label;
        
        public CanvasPositionLabel (Gtk.Widget widget) : base (widget)
        {
            UpdateLabel ();
        }
        
        protected override void ClippedRender (Cairo.Context cr)
        {
            EnsureLayout (cr);
            
            if (String.IsNullOrEmpty (label)) {
                return;
            }
            
            layout.Width = (int)(InnerWidth * Pango.Scale.PangoScale);
            layout.SetText (label);
            
            cr.Color = Theme.Colors.GetWidgetColor (GtkColorClass.Foreground, Gtk.StateType.Normal);
            PangoCairoHelper.ShowLayout (cr, layout);
        } 
        
        public override void SizeRequest (out double width, out double height)
        {
            base.SizeRequest (out width, out height);
            height += Height;
        }
        
        public override double Height {
            get { return ComputeLineHeight (); }
            set { }
        }
        
        private void EnsureLayout (Context cr)
        {
            if (layout == null) {
                layout = CairoExtensions.CreateLayout (Widget, cr);
                layout.Ellipsize = Pango.EllipsizeMode.End;
            }
        }

        private void UpdateLabel ()
        {
            if (IsBuffering) {
                double progress = buffering_progress * 100.0;
                UpdateLabel (String.Format ("{0}: {1}%", Catalog.GetString ("Buffering"), progress.ToString ("0.0")));
            } else if (IsContacting) {
                UpdateLabel (Catalog.GetString ("Contacting..."));
            } else if (IsLoading) {
                UpdateLabel (Catalog.GetString ("Loading..."));
            } else if (IsIdle) {
                UpdateLabel (Catalog.GetString ("Idle"));
            } else if (duration == Int64.MaxValue) {
                UpdateLabel (FormatDuration ((long)position));
            } else if (position == 0 && duration == 0) {
                // nop
            } else {
                UpdateLabel (String.Format (Catalog.GetString ("{0} of {1}"),
                    FormatDuration ((long)position), FormatDuration ((long)duration)));
            }
        }
        
        private void UpdateLabel (string text)
        {
            label = text;
            OnResize ();
            OnRerender ();
        }
        
        private static string FormatDuration (long time)
        {
            time /= 1000;
            return (time > 3600 ? 
                    String.Format ("{0}:{1:00}:{2:00}", time / 3600, (time / 60) % 60, time % 60) :
                    String.Format ("{0}:{1:00}", time / 60, time % 60));
        }
        
        public double BufferingProgress {
            get { return buffering_progress; }
            set {
                buffering_progress = Math.Max (0.0, Math.Min (1.0, value));
                UpdateLabel ();
            }
        }
        
        public double Position {
            get { return position; }
            set { 
                duration = value; 
                UpdateLabel ();
            }
        }
        
        public double Duration {
            get { return duration; }
            set { 
                duration = value; 
                UpdateLabel ();
            }
        }
        
        public StreamLabelState StreamState {
            get { return state; }
            set { 
                if (state != value) {
                    state = value;
                    UpdateLabel ();
                }
            }
        }

        public bool IsLive {
            get { return is_live; }
            set { 
                if (is_live != value) {
                    is_live = value;
                    UpdateLabel ();
                }
            }
        }
        
        public bool IsIdle {
            get { return StreamState == StreamLabelState.Idle; }
        }

        public bool IsBuffering {
            get { return StreamState == StreamLabelState.Buffering; }
        }
        
        public bool IsContacting {
            get { return StreamState == StreamLabelState.Contacting; }
        }

        public bool IsLoading {
            get { return StreamState == StreamLabelState.Loading; }
        }
    }
}
