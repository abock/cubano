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

namespace Banshee.Gui.Widgets
{
    public class SeekableTrackInfoDisplay : StackPanel
    {
        private TextBlock title;
        private TestTile seek_bar;
        private StackPanel time_bar;
        private TextBlock elapsed;
        private TextBlock seek_to;
        private TextBlock remaining;
        
        public SeekableTrackInfoDisplay ()
        {
            Spacing = 5;
            Orientation = Orientation.Vertical;
            
            Children.Add (title = new TextBlock ());
            Children.Add (seek_bar = new TestTile ());
            Children.Add (time_bar = new StackPanel () {
                Spacing = 10,
                Children = {
                    (elapsed = new TextBlock ()),
                    (seek_to = new TextBlock ()),
                    (remaining = new TextBlock ())
                }
           });
           
           title.Markup = "Some Awesome Title";
           elapsed.Markup = "0:35";
           seek_to.Markup = "1:59";
           remaining.Markup = "-3:18";
        }
    }
}
