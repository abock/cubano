// 
// CubanoVisualizer.cs
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

using Banshee.ServiceStack;
using Banshee.MediaEngine;

namespace Cubano.Client
{
    public class CubanoVisualizer : IDisposable
    {
        public EventHandler<EventArgs> RenderRequest;
        
        private float [][] last_pcm_data;
        private float [][] last_spectrum_data;
        
        private int height = 80;
        public int Height {
            get { return height; }
            set { height = value; }
        }

        private int width = 160;
        public int Width {
            get { return width; }
            set { width = value; }
        }
    
        public CubanoVisualizer ()
        {
            ConnectOrDisconnect (true);
        }
        
        public void Dispose ()
        {
            ConnectOrDisconnect (false);
        }
        
        private void ConnectOrDisconnect (bool connect)
        {
            foreach (var engine in ServiceManager.PlayerEngine.Engines) {
                var vis_engine = engine as IVisualizationDataSource;
                if (vis_engine != null) {
                    if (connect) {
                        vis_engine.DataAvailable += OnVisualizationDataAvailable;
                    } else {
                        vis_engine.DataAvailable -= OnVisualizationDataAvailable;
                    }
                }
            }
        }
        
        protected virtual void OnRenderRequest ()
        {
            EventHandler<EventArgs> handler = RenderRequest;
            if (handler != null) {
                handler (this, EventArgs.Empty);
            }
        }
        
        private void OnVisualizationDataAvailable (float [][] pcm, float [][] spectrum)
        {
            last_pcm_data = pcm;
            last_spectrum_data = spectrum;
            Banshee.Base.ThreadAssist.ProxyToMain (delegate {
                OnRenderRequest ();
            });
        }
        
        public void Render (Context cr)
        {
            if (last_pcm_data == null && last_spectrum_data == null) {
                return;
            }

            cr.MoveTo (0, Height);
            for (int i = 0; i < last_spectrum_data[0].Length; i+=2) {
                cr.LineTo (i, Height - (Height * 1.5 * last_spectrum_data[0][i]));
            }
            cr.Color = new Color (1, 0, 0, 0.5);
            cr.LineTo (0, Height);
            cr.Fill ();
        }
    }
}
