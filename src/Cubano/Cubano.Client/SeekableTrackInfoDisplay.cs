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

using Hyena.Gui.Canvas;
using Hyena.Gui.Theatrics;

namespace Banshee.Gui.Widgets
{
    public class SeekableTrackInfoDisplay : StackPanel
    {
        private static double text_opacity = 0.6;
    
        private TextBlock title;
        private Slider seek_bar;
        private StackPanel time_bar;
        private TextBlock elapsed;
        private TextBlock seek_to;
        private TextBlock remaining;
        
        private DoubleAnimation transition_animation;
        private uint transition_timeout;
        
        private int display_metadata_index;
        private int display_metadata_states = 3;
        
        public SeekableTrackInfoDisplay ()
        {
            Spacing = 3;
            Orientation = Orientation.Vertical;
            
            Children.Add (title = new TextBlock () { Opacity = text_opacity });
            Children.Add (seek_bar = new Slider () { Value = 0.28 });
            Children.Add (time_bar = new StackPanel () {
                Spacing = 10,
                Children = {
                    (elapsed = new TextBlock ()   { HorizontalAlignment = 0.0, Opacity = text_opacity + 0.25 }),
                    (seek_to = new TextBlock ()   { HorizontalAlignment = 0.5, Opacity = text_opacity + 0.25 }),
                    (remaining = new TextBlock () { HorizontalAlignment = 1.0, Opacity = text_opacity })
                }
            });
            
            seek_to.Visible = false;
            
            UpdateMetadataDisplay ();
            BuildTransitionAnimation ();
            StartTransitionTimeout ();
            
            elapsed.Text = "0:35";
            seek_to.Text = "1:59";
            remaining.Text = "-3:18";
        }
        
        private void BuildTransitionAnimation ()
        {
            transition_animation = new DoubleAnimation ("Opacity");
            transition_animation
                .Throttle (500)
                .Compose ((a, p) => {
                    var opacity = a.StartState == 0 ? p : 1 - p;
                    if (p == 1) {
                        if (a.StartState == 1) {
                            UpdateMetadataDisplay ();
                        }
                        
                        if (a.ToValue == 1) {
                            a.Expire ();
                        } else {
                            a.Reverse ();
                        }
                    }

                    return opacity * text_opacity;
                }).Ease (Easing.QuadraticInOut);
        }
        
        private void StartTransitionTimeout ()
        {
            if (transition_timeout > 0) {
                StopTransitionTimeout ();
            }
            
            transition_timeout = GLib.Timeout.Add (5000, OnTransitionTimeout);
        }
        
        private void StopTransitionTimeout ()
        {
            if (transition_timeout > 0) {
                GLib.Source.Remove (transition_timeout);
                transition_timeout = 0;
            }
        }
        
        private bool OnTransitionTimeout ()
        {
            title.Animate (transition_animation).From (1).To (0);
            return true;
        }
        
        private static string [] strings = {
            "The Brighter side of Suffering",
            "As Blood Runs Black",
            "Allegiance"
        };
        
        private void UpdateMetadataDisplay ()
        {
            display_metadata_index = (display_metadata_index + 1) % display_metadata_states;
            title.Text = strings[display_metadata_index];
        }
        
        public override void Arrange ()
        {
            base.Arrange ();
            time_bar.Margin = title.Margin = new Thickness (seek_bar.BarAlignment, 0);
        }
    }
}
