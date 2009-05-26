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
            Console.WriteLine ("RENDER!");
            QueueRedraw ();
        }
        
        protected override void OnPainted ()
        {
            Cogl.General.PushMatrix ();

            Cogl.Path.RoundRectangle (0, 0, Width, Height, 10, 5);
            
            Cogl.Path.Ellipse (0, 0, 60, 40);
            Cogl.General.SetSourceColor4ub (200, 0, 0, 128);
            Cogl.Path.Fill ();

            Cogl.General.PopMatrix ();
        }
    }
}
