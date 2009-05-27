// 
// CubanoClutterVisualizer.cs
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

using Clutter;

namespace Cubano.NowPlaying
{
    public class CubanoClutterVisualizer : Rectangle
    {
        private CubanoVisualizer visualizer = new CubanoVisualizer ();
        
        public CubanoClutterVisualizer ()
        {
            Color = new Color (0, 0, 0, 0);
            visualizer.RenderRequest += OnVisualizerRenderRequest;
        }
        
        private void OnVisualizerRenderRequest (object o, EventArgs args)
        {
            QueueRedraw ();
        }
        
        protected override void OnPainted ()
        {
            lock (visualizer.RenderPointsMutex) {
                if (visualizer.RenderPoints == null || visualizer.RenderPoints.Length == 0) {
                    return;
                }
            
                Cogl.General.PushMatrix ();
                
                float max_r = Height / 2;
                float x_ofs = Width / visualizer.RenderPoints.Length;
                float xc = 0, yc = Height;
                float r;
                
                float min_x = Width, max_x = 0, min_y = Height, max_y = yc;
                
                for (int i = 0, n = visualizer.RenderPoints.Length; i < n; i++) {
                    xc += x_ofs;
                    r = Height * visualizer.RenderPoints[i];
                    
                    Cogl.Path.Ellipse (xc, yc, r, r);
                    
                    if (r > 0) {
                        min_x = Math.Min (min_x, xc - r);
                        max_x = Math.Max (max_x, xc + r);
                        min_y = Math.Min (min_y, yc - r);
                    }
                }
                
                Cogl.General.SetSourceColor4ub (255, 128, 0, 
                    (byte)((visualizer.RenderLoudness * 128) + 30));
                Cogl.Path.Fill ();
                
                Cogl.General.PopMatrix ();
            }
        }
    }
}
