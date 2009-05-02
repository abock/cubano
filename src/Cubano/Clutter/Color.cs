//
// Color.cs
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

namespace Clutter
{
    public struct Color
    {
        public static Color Zero = new Color (0, 0, 0, 0);
        public static Color Black = new Color (0, 0, 0);
        public static Color White = new Color (0xff, 0xff, 0xff);
    
        // The ClutterColor struct; do not add any other fields
        private byte red;
        private byte green;
        private byte blue;
        private byte alpha;
        
        public Color (byte red, byte green, byte blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.alpha = 0xff;
        }
        
        public Color (byte red, byte green, byte blue, byte alpha)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.alpha = alpha;
        }
        
        public Color (double red, double green, double blue)
            : this ((byte)(red * 255.0), (byte)(green * 255.0), (byte)(blue * 255.0))
        {
        }
        
        public Color (double red, double green, double blue, double alpha)
            : this ((byte)(red * 255.0), (byte)(green * 255.0), (byte)(blue * 255.0), (byte)(alpha * 255.0))
        {
        }
        
        public Color (uint rgbaColor)
        {
            this.red = (byte)(rgbaColor >> 24);
            this.green = (byte)((rgbaColor >> 16) & 0xff);
            this.blue = (byte)((rgbaColor >> 8) & 0xff);
            this.alpha = (byte)(rgbaColor & 0xff);
        }
        
        public static Color FromRgb (uint rgbColor)
        {
            return new Color ((rgbColor << 8) | 0xff);
        }
        
        public static Color FromRgba (uint rgbaColor)
        {
            return new Color (rgbaColor);
        }
        
        public byte Red {
            get { return red; }
            set { red = value; }
        }
        
        public byte Green {
            get { return green; }
            set { green = value; }
        }
        
        public byte Blue {
            get { return blue; }
            set { blue = value; }
        }
        
        public byte Alpha {
            get { return alpha; }
            set { alpha = value; }
        }
    }
}
