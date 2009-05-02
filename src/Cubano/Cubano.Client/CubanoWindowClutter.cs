// 
// CubanoWindowClutter.cs
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
using Gtk;
using Clutter;
using Clutter.Gtk;

namespace Cubano.Client
{
    public class Flower : CairoTexture
    {
        private const int PETAL_MIN = 20;
        private const int PETAL_VAR = 40;
        
        private static Random rand = new Random ();
        
        private int x, y, rot, v, rv;
        
        public Flower () : base (1, 1)
        {
            Render ();
        }
        
        private void Render ()
        {
            var colors = new Cairo.Color [] {
                new Cairo.Color (0.71, 0.81, 0.83),
                new Cairo.Color ( 1.0,  0.78, 0.57),
                new Cairo.Color ( 0.64, 0.30, 0.35),
                new Cairo.Color ( 0.73, 0.40, 0.39),
                new Cairo.Color ( 0.91, 0.56, 0.64),
                new Cairo.Color ( 0.70, 0.47, 0.45),
                new Cairo.Color ( 0.92, 0.75, 0.60),
                new Cairo.Color ( 0.82, 0.86, 0.85),
                new Cairo.Color ( 0.51, 0.56, 0.67),
                new Cairo.Color ( 1.0, 0.79, 0.58)
            };
            
            int petal_size = PETAL_MIN + rand.Next () % PETAL_VAR;
            int size = petal_size * 8;
            int n_groups = rand.Next () % 3 + 1;
            
            SurfaceWidth = SurfaceHeight = (uint)size;

            using (var cr = CreateContext ()) {
                cr.Operator = Cairo.Operator.Clear;
                cr.Paint ();
                cr.Operator = Cairo.Operator.Over;
                
                cr.Tolerance = 0.1;
                
                cr.Translate (size / 2, size / 2);
                
                for (int i = 0; i < n_groups; i++) {
                    cr.Save ();
                    cr.Rotate (rand.Next () % 6);
                    cr.Color = colors[rand.Next () % colors.Length];

                    int n_petals = rand.Next () % 5 + 4;
                    int pm1 = rand.Next () % 20;
                    int pm2 = rand.Next () % 4;
                    
                    for (int j = 1; j <= n_petals; j++) {
                        cr.Save ();
                        cr.Rotate ((2 * Math.PI / n_petals) * j);
      
                        cr.NewPath ();
                        cr.MoveTo (0, 0);
                        cr.RelCurveTo (
                            petal_size, petal_size, 
                            (pm2 + 2) * petal_size, petal_size,
                            (2 * petal_size) + pm1, 0);
                        cr.RelCurveTo (
                            0 + (pm2 * petal_size), -petal_size,
                            -petal_size, -petal_size,
                            -((2 * petal_size) + pm1), 0);
                        cr.ClosePath ();
                        
                        cr.Fill ();
                        cr.Restore ();
                    }

                    petal_size -= rand.Next () % (size / 8);

                    cr.Restore ();
                }
                
                if (petal_size < 0) {
                    petal_size = rand.Next () % 10;
                }
                
                cr.Color = colors[rand.Next () % colors.Length];
                cr.Arc (0, 0, petal_size, 0, 2 * Math.PI);
                cr.Fill ();
            }
        }
    }

    public class CubanoWindowClutter : Window
    {
        public static void Start (string [] args)
        {
            Gtk.Application.Init ();
            Clutter.Application.InitForToolkit ();
            
            new CubanoWindowClutter ();
            
            Gtk.Application.Run ();
        }
        
        private Embed clutter_host;
    
        public CubanoWindowClutter () : base ("Cubano")
        {
            Add (clutter_host = new Embed ());
            clutter_host.Stage.Add (new Flower ());
            ShowAll ();
        }
    }
}
