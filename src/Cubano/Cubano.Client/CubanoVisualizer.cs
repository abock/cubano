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
using System.Collections.Generic;

using Cairo;

using Banshee.ServiceStack;
using Banshee.MediaEngine;

namespace Cubano.Client
{
    public class CubanoVisualizer : IDisposable
    {
        public class RenderRequestArgs : EventArgs
        {
            private Gdk.Rectangle damage;
            public Gdk.Rectangle Damage {
                get { return damage; }
                set { damage = value; }
            }
        }
    
        private object render_points_mutext = new object ();
        private Queue<float []> points = new Queue<float []> ();
        private Gdk.Rectangle render_damage;
        
        public EventHandler<RenderRequestArgs> RenderRequest;
        
        private float [] render_points;
        protected float [] RenderPoints {
            get { return render_points; }
        }
        
        private float render_loudness;
        protected float RenderLoudness {
            get { return render_loudness; }
        }
        
        private int smoothing_passes = 5;
        public int SmoothingPasses {
            get { return smoothing_passes; }
            set { smoothing_passes = value; }
        }
        
        private int sample_points = 25;
        public int SamplePoints {
            get { return sample_points; }
            set { sample_points = value; }
        }
        
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
        
        private Color color = new Color (0.2, 0.2, 0.2);
        public Color Color {
            get { return color; }
            set { color = value; }
        }
    
        public CubanoVisualizer ()
        {
            ConnectOrDisconnect (true);
            RequestRender ();
        }
        
        public void Dispose ()
        {
            ConnectOrDisconnect (false);
            RequestRender ();
        }
        
        public void Connect ()
        {
            ConnectOrDisconnect (true);
            RequestRender ();
        }
        
        public void Disconnect ()
        {
            ConnectOrDisconnect (false);
            RequestRender ();
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
            var handler = RenderRequest;
            if (handler != null) {
                handler (this, new RenderRequestArgs () { Damage = render_damage });
            }
        }
        
        private void RequestRender ()
        {
            Banshee.Base.ThreadAssist.ProxyToMain (delegate {
                OnRenderRequest ();
            });
        }
        
        private DateTime last_render = DateTime.MinValue;
        private TimeSpan max_framerate = TimeSpan.FromSeconds (1.0 / 20.0);
        
        private void OnVisualizationDataAvailable (float [][] pcm, float [][] spectrum)
        {
            if (spectrum == null || DateTime.Now - last_render < max_framerate) {
                return;
            }

            lock (render_points_mutext) {
                UpdatePoints (spectrum[0]);
            }
            
            RequestRender ();
            
            last_render = DateTime.Now;
        }
        
        private float [] AnalyzeSpectrum (float [] spectrum, int sampleCount)
        {   
            int sample_idx = 0;
            var samples = new float[sampleCount * 2];
            
            // Sample the spectrum to produce 2 * sampleCount points, creating
            // a smoothed mirrored audio frame spectrum for averaging with
            // previous AnalyzeSpectrum passes or suitable for direct rendering
            for (int i = 0, n = spectrum.Length; i < n; i++) {
                sample_idx = (int)Math.Round ((i / (double)n) * (sampleCount - 1));
                int sample_idx_ofs = sampleCount - sample_idx - 1;
                samples[sample_idx_ofs] += spectrum[i] / (n / (sampleCount - 1));
                samples[sample_idx + sampleCount] = samples[sample_idx_ofs];
            }
            
            // Find the maximum point in the computed points
            float max = 0;
            for (int i = 0; i < samples.Length; i++) {
                max = Math.Max (max, samples[i]);
            }
            
            // Fudge the mid point to be the max
            samples[(int)Math.Floor (samples.Length / 2.0)] = max;
            samples[(int)Math.Ceiling (samples.Length / 2.0)] = max;
            
            // Scale the set by the maximum point
            for (int i = 0; i < samples.Length; i++) {
                samples[i] /= (float)(1.0f - Math.Sqrt (max));
            }
            
            return samples;
        }
        
        private void UpdatePoints (float [] spectrum)
        {
            // Add the current analyzed audio frame and dequeue any expired frame
            points.Enqueue (AnalyzeSpectrum (spectrum, SamplePoints));
            if (points.Count > SmoothingPasses) {
                points.Dequeue ();
            }
            
            render_points = null;
            render_loudness = 0;
            
            // Average all the queued frames into a frame suitable for direct rendering,
            // creating very smooth visualization data; the higher SmoothingPasses is
            // the smoother the data, but the more static (less motion)
            foreach (var iter in points) {
                if (render_points == null) {
                    render_points = new float[iter.Length];
                }
                
                for (int i = 0, n = Math.Min (iter.Length, render_points.Length); i < n; i++) {
                    render_points[i] += iter[i] / (float)points.Count;
                }
            }
            
            for (int i = 0; i < render_points.Length / 2; i++) {
                render_loudness = (float)Math.Max (render_loudness, render_points[i]);
            }
        }
        
        private void RenderDebug (Context cr)
        {
            double x = 320;
            for (int i = 0, n = render_points.Length; i < n; i++) {
                cr.Color = new Color (0, 1, 0);
                cr.Rectangle (x, Height - Height * render_points[i], 10, Height * render_points[i]);
                cr.Fill ();
                cr.Color = new Color (0, 0, 0, 0.3);
                cr.LineWidth = 1;
                cr.Rectangle (x + 0.5, 0.5, 10 - 1, Height - 1);
                cr.Stroke ();
                x += 12;
            }
        }
        
        public void Render (Context cr)
        {
            lock (render_points_mutext) {
                if (render_points == null) {
                    return;
                }
                
                RenderGoo (cr);
                // RenderDebug (cr);
            }
        }
        
        private void RenderGoo (Context cr)
        {
            double max_r = Height / 2;
            double x_ofs = Width / RenderPoints.Length;
            double xc = 0, yc = Height;
            double r;
            
            double min_x = Width, max_x = 0, min_y = Height, max_y = yc;
            
            for (int i = 0, n = RenderPoints.Length; i < n; i++) {
                xc += x_ofs;
                r = Height * RenderPoints[i];
                cr.MoveTo (xc, yc);
                cr.Arc (xc, yc, r, 0, 2 * Math.PI);
                
                if (r > 0) {
                    min_x = Math.Min (min_x, xc - r);
                    max_x = Math.Max (max_x, xc + r);
                    min_y = Math.Min (min_y, yc - r);
                }
            }
            
            render_damage = new Gdk.Rectangle (
                (int)Math.Floor (min_x),
                (int)Math.Floor (min_y),
                (int)Math.Ceiling (max_x - min_x),
                (int)Math.Ceiling (max_y - min_y)
            );
            
            cr.ClosePath ();
            
            var grad = new LinearGradient (0, 0, 0, Height);
            
            Color c = Color;
            c.A = 0;
            grad.AddColorStop (0, c);
            
            c.A = RenderLoudness / 2;
            grad.AddColorStop (1, c);
            
            cr.Pattern = grad;
            cr.Fill ();
            
            //cr.Color = new Color (1, 0, 0);
            //cr.Rectangle (render_damage.X + 0.5, render_damage.Y + 0.5, render_damage.Width - 1, render_damage.Height - 1);
            //cr.Stroke ();
            
            grad.Destroy ();
        }
    }
}
