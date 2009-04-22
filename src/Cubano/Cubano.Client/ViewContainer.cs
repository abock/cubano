// 
// ViewContainer.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
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
using Gtk;
using Mono.Unix;

using Banshee.Widgets;
using Banshee.Gui.Widgets;
using Banshee.Sources.Gui;
using Banshee.Collection;

using Banshee.Gui;
using Banshee.ServiceStack;

namespace Cubano.Client
{
    public class ViewContainer : VBox
    {
        private VBox footer;
        
        private ISourceContents content;
        
        public ViewContainer ()
        {
            BuildHeader ();           
            
            Spacing = 8;
        }
        
        private void BuildHeader ()
        {
            footer = new VBox ();
            PackEnd (footer, false, false, 0);
            PackEnd (new ConnectedMessageBar (), false, true, 0);
        }

        public void SetFooter (Widget contents)
        {
            if (contents != null) {
                footer.PackStart (contents, false, false, 0);
                contents.Show ();
                footer.Show ();
            }
        }
        
        public void ClearFooter ()
        {
            footer.Hide ();
            foreach (Widget child in footer.Children) {
                footer.Remove (child);
            }
        }

        public ISourceContents Content {
            get { return content; }
            set {
                if (content == value) {
                    return;
                }

                // Hide the old content widget
                if (content != null && content.Widget != null) {
                    content.Widget.Hide ();
                }

                // Add and show the new one
                if (value != null && value.Widget != null) {
                    PackStart (value.Widget, true, true, 0);
                    value.Widget.Show ();
                }
                
                // Remove the old one
                if (content != null && content.Widget != null) {
                    Remove (content.Widget);
                }
                
                content = value;
            }
        }
    }
}
