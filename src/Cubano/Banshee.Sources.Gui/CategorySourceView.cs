// 
// CategorySourceView.cs
//  
// Author:
//   Aaron Bockover <abockover@novell.com>
// 
// Copyright 2009 Novell, Inc.
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
using System.Collections.Generic;

using Gtk;

using Banshee.Sources;
using Banshee.ServiceStack;
using Banshee.Gui.Widgets;

namespace Banshee.Sources.Gui
{
    public class CategorySourceView : HBox, IDisposable
    {
        private List<SourceLabel> labels = new List<SourceLabel> ();
        
        private class SourceLabel : ActionLabel
        {
            public SourceLabel ()
            {
                CurrentFontSizeEm = DefaultFontSizeEm = 1.5;
                ActiveFontSizeEm = 1.6;
                
                Activated += (o, e) => {
                    if (Source != null && Source.CanActivate && 
                        ServiceManager.SourceManager.ActiveSource != Source) {
                        ServiceManager.SourceManager.SetActiveSource (Source);
                    }
                };
            }
        
            private Source source;
            public Source Source {
                get { return source; }
                set {
                    source = value;
                    Text = source.Name.ToLower ();
                }
            }
        }
    
        public CategorySourceView ()
        {
            ServiceManager.SourceManager.SourceAdded += OnSourceUpdated;
            ServiceManager.SourceManager.SourceRemoved += OnSourceUpdated;
            ServiceManager.SourceManager.ActiveSourceChanged += OnActiveSourceChanged;
            
            Spacing = 15;
        }
        
        public override void Dispose ()
        {
            ServiceManager.SourceManager.SourceAdded -= OnSourceUpdated;
            ServiceManager.SourceManager.SourceRemoved -= OnSourceUpdated;
            ServiceManager.SourceManager.ActiveSourceChanged -= OnActiveSourceChanged;
            base.Dispose ();
        }
        
        private void OnSourceUpdated (SourceEventArgs args)
        {
            Banshee.Base.ThreadAssist.ProxyToMain (delegate {
                UpdateSources ();
            });
        }
        
        private void OnActiveSourceChanged (SourceEventArgs args)
        {
            Banshee.Base.ThreadAssist.ProxyToMain (delegate {
                foreach (var label in labels) {
                    label.IsSelected = label.Source == args.Source;
                }
            });
        }
        
        private bool ShouldShowSource (Source source)
        {
            return source != null && source.Parent == null &&
                !source.UniqueId.StartsWith ("NowPlayingSource") &&
                !source.UniqueId.StartsWith ("PlayQueueSource");
        }

        private void UpdateSources ()
        {
            int count = 0;
        
            foreach (var source in ServiceManager.SourceManager.Sources) {
                if (!ShouldShowSource (source)) {
                    continue;
                }
                
                SourceLabel label = null;
                if (count < labels.Count) {
                    label = labels[count];
                } else {
                    labels.Add (label = new SourceLabel ());
                    PackStart (label, false, false, 0);
                    label.Show ();
                }
                
                label.Source = source;
                count++;
            }
            
            for (int i = count; i < labels.Count; i++) {
                Remove (labels[i]);
            }
            
            labels.RemoveRange (count, labels.Count - count);
        }
    }
}
