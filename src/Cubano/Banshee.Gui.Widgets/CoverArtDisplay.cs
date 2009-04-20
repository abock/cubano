// 
// CoverArtDisplay.cs
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

using Hyena.Gui;
using Hyena.Gui.Canvas;
using Hyena.Gui.Theming;
using Hyena.Gui.Theatrics;

using Banshee.Collection;
using Banshee.Collection.Gui;
using Banshee.ServiceStack;

namespace Banshee.Gui.Widgets
{
    public class CoverArtDisplay : FixedPanel
    {
        private class CoverArtImage : Image
        {
            private CoverArtBrush brush;
            public CoverArtBrush Brush {
                get { return brush; }
                set { Background = brush = value; }
            }
            
            public CoverArtImage ()
            {
                Opacity = 1;
                Brush = new CoverArtBrush ();
            }
        }
    
        private TrackInfo current_track;
        
        public CoverArtDisplay ()
        {
            InstallProperty<double> ("ImageSize", 60);
            UpdateImageSize ();
            
            Margin = new Thickness (5);
            MarginStyle = new ShadowMarginStyle {
                ShadowSize = 3,
                ShadowOpacity = 0.3,
                Fill = Brush.White
            };
            
            for (int i = 0; i < 2; i++) {
                Children.Add (new CoverArtImage ());
            }
        }
        
        protected override void ClippedRender (Cairo.Context cr)
        {
            base.ClippedRender (cr);
            
            cr.Color = new Color (0, 0, 0, 0.3);
            cr.LineWidth = 1.0;
            cr.Rectangle (0.5, 0.5, RenderSize.Width - 1, RenderSize.Height - 1);
            cr.Stroke ();
        }
        
        public void UpdateCurrentTrack (TrackInfo track)
        {
            if (track == null) {
                current_track = null;
                foreach (var image in Children) {
                    
                    /*image.Opacity = 0;
                    image.Brush.Clear ();*/
                }
                return;
            }
            
            CoverArtImage update_image = (CoverArtImage)Children[0];
            CoverArtImage current_image = (CoverArtImage)Children[1];
            int image_size = (int)Math.Ceiling (ImageSize);
            
            if (current_image.Brush.ShouldUpdate (track, image_size)) {
                update_image.Brush.Load (track, image_size);
                Children.Move (update_image, 1);
                
                update_image.AnimateDouble ("Opacity").From (0).To (1).Repeat (1).Compose ((a, p) => {
                    if (p == 1) {
                        current_image.Opacity = 0;
                    }
                    return Choreographer.Compose (p, Easing.QuadraticInOut);
                }).Start ();
            }
        }
        
        private void UpdateImageSize ()
        {
            Width = Height = ImageSize;
        }
        
        protected override bool OnPropertyChange (string property, object value)
        {
            switch (property) {
                case "ImageSize":
                    UpdateImageSize ();
                    return true;
            }
            
            return base.OnPropertyChange (property, value);
        }
        
        public double ShadowSize {
            get { return GetValue<double> ("ShadowSize"); }
            set { SetValue<double> ("ShadowSize", value); }
        }
        
        public double ImageSize {
            get { return GetValue<double> ("ImageSize"); }
            set { SetValue<double> ("ImageSize", value); }
        }
    }
}
