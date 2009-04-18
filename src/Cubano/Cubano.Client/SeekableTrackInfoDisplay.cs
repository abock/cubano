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
        private TextBlock title;
        private Slider seek_bar;
        private StackPanel time_bar;
        private TextBlock elapsed;
        private TextBlock seek_to;
        private TextBlock remaining;
        
        public SeekableTrackInfoDisplay ()
        {
            Spacing = 5;
            Orientation = Orientation.Vertical;
            
            Children.Add (title = new TextBlock ());
            Children.Add (seek_bar = new Slider ());
            Children.Add (time_bar = new StackPanel () {
                Spacing = 10,
                Children = {
                    (elapsed = new TextBlock ()   { HorizontalAlignment = 0.0 }),
                    (seek_to = new TextBlock ()   { HorizontalAlignment = 0.5 }),
                    (remaining = new TextBlock () { HorizontalAlignment = 1.0 })
                }
            });
           
            int str_idx = 0;
            string [] strings = {
                "The Brighter side of Suffering",
                "As Blood Runs Black",
                "Allegiance"
            };
              
            var fade = new DoubleAnimation ("Opacity")
                .Throttle (500)
                .Compose ((a, p) => {
                    var opacity = a.StartState == 0 ? p : 1 - p;
                    if (p == 1) {
                        if (a.StartState == 1) {
                            title.Text = strings[(str_idx = (str_idx + 1) % strings.Length)];
                        }
                        
                        if (a.ToValue == 1) {
                            a.Expire ();
                        } else {
                            a.Reverse ();
                        }
                    }

                    return opacity;
                }).Ease (Easing.QuadraticInOut);
                
            title.AnimateDouble ("HorizontalAlignment")
                .From (0)
                .To (1)
                .Compose ((a, p) =>
                    p <= 0.5 
                        ? 2 * p
                        : 1 - (2 * (p - 0.5)))
                .Ease (Easing.QuadraticInOut);
            
            GLib.Timeout.Add (5000, () => {
                title.Animate (fade).From (1).To (0);
                return true;
            });
                                    
            title.Text = strings[str_idx];
            elapsed.Text = "0:35";
            seek_to.Text = "1:59";
            remaining.Text = "-3:18";
        }
    }
}
