// 
// CubanoWindowDecorator.cs
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
using Gdk;
using Gtk;

namespace Hyena.Gui
{
    public class WindowDecorator : IDisposable
    {
        private Cursor [] cursors;
        private bool default_cursor = true;
        private Gtk.Window window;
        private bool resizing = false;
        private WindowEdge last_edge;
        private int resize_width = 4;
        private int top_move_height = 80;
    
        public WindowDecorator (Gtk.Window window)
        {
            this.window = window;
            window.Decorated = false;
            
            window.MotionNotifyEvent += OnMotionNotifyEvent;
            window.EnterNotifyEvent += OnEnterNotifyEvent;
            window.LeaveNotifyEvent += OnLeaveNotifyEvent;
            window.ButtonPressEvent += OnButtonPressEvent;
            window.ButtonReleaseEvent += OnButtonReleaseEvent;
            window.SizeAllocated += OnSizeAllocated;
            
            if (!window.IsRealized) {
                window.Events |= 
                    EventMask.ButtonPressMask | 
                    EventMask.ButtonReleaseMask |
                    EventMask.EnterNotifyMask |
                    EventMask.LeaveNotifyMask |
                    EventMask.PointerMotionMask;
            }
        }
        
        public void Dispose ()
        {
            if (window == null) {
                return;
            }
            
            DestroyCursors ();
            ResetCursor ();
            
            window.MotionNotifyEvent -= OnMotionNotifyEvent;
            window.EnterNotifyEvent -= OnEnterNotifyEvent;
            window.LeaveNotifyEvent -= OnLeaveNotifyEvent;
            window.ButtonPressEvent -= OnButtonPressEvent;
            window.ButtonReleaseEvent -= OnButtonReleaseEvent;
            window.SizeAllocated -= OnSizeAllocated;
            
            window.Decorated = true;
            window.QueueDraw ();
            window = null;
        }
        
        public virtual void Render (Cairo.Context cr)
        {
        }
        
        private void OnSizeAllocated (object o, SizeAllocatedArgs args)
        {
            window.QueueDraw ();
        }
        
        private void OnMotionNotifyEvent (object o, MotionNotifyEventArgs args)
        {
            UpdateCursor (args.Event.X, args.Event.Y, false);
        }
        
        private void OnEnterNotifyEvent (object o, EnterNotifyEventArgs args)
        {
            UpdateCursor (args.Event.X, args.Event.Y, false);
        }
        
        private void OnLeaveNotifyEvent (object o, LeaveNotifyEventArgs args)
        {
            ResetCursor ();
        }
        
        private void OnButtonPressEvent (object o, ButtonPressEventArgs args)
        {
            int x_root = (int)args.Event.XRoot;
            int y_root = (int)args.Event.YRoot;
            
            UpdateCursor (args.Event.X, args.Event.Y, true);
            
            if (resizing && args.Event.Button == 1) {
                window.BeginResizeDrag (last_edge, 1, x_root, y_root, args.Event.Time);
            } else if ((resizing && args.Event.Button == 2) || 
                (args.Event.Button == 1 && args.Event.Y <= TopMoveHeight)) {
                window.BeginMoveDrag ((int)args.Event.Button, x_root, y_root, args.Event.Time);
            }
        }
        
        private void OnButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
        {
            if (resizing) {
                UpdateCursor (args.Event.X, args.Event.Y, true);
            }
        }
        
#region Cursor Management
        
        private bool InTop    (double y) { return y <= ResizeWidth; }
        private bool InLeft   (double x) { return x <= ResizeWidth; }
        private bool InBottom (double y) { return y >= window.Allocation.Height - ResizeWidth; }
        private bool InRight  (double x) { return x >= window.Allocation.Width - ResizeWidth; }
                
        private void UpdateCursor (double x, double y, bool updateResize)
        {
            if (updateResize) {
                resizing = true;
            }
            
            if (InTop (y) && InLeft (x)) {
                last_edge = WindowEdge.NorthWest;
                SetCursor (CursorType.TopLeftCorner);
            } else if (InTop (y) && InRight (x)) {
                last_edge = WindowEdge.NorthEast;
                SetCursor (CursorType.TopRightCorner);
            } else if (InBottom (y) && InLeft (x)) {
                last_edge = WindowEdge.SouthWest;
                SetCursor (CursorType.BottomLeftCorner);
            } else if (InBottom (y) && InRight (x)) {
                last_edge = WindowEdge.SouthEast;
                SetCursor (CursorType.BottomRightCorner);
            } else if (InTop (y)) {
                last_edge = WindowEdge.North;
                SetCursor (CursorType.TopSide);
            } else if (InLeft (x)) {
                last_edge = WindowEdge.West;
                SetCursor (CursorType.LeftSide);
            } else if (InBottom (y)) {
                last_edge = WindowEdge.South;
                SetCursor (CursorType.BottomSide);
            } else if (InRight (x)) {
                last_edge = WindowEdge.East;
                SetCursor (CursorType.RightSide);
            } else {
                if (updateResize) {
                    resizing = false;
                }
                
                ResetCursor ();
            }
        }
        
        private Cursor GetCursor (CursorType type)
        {
            int index;
            
            switch (type) {
                case CursorType.TopSide: index = 0; break;
                case CursorType.LeftSide: index = 1; break;
                case CursorType.BottomSide: index = 2; break;
                case CursorType.RightSide: index = 3; break;
                case CursorType.TopLeftCorner: index = 4; break;
                case CursorType.TopRightCorner: index = 5; break;
                case CursorType.BottomLeftCorner: index = 6; break;
                case CursorType.BottomRightCorner: index = 7; break;
                default: return null;
            }
            
            if (cursors == null) {
                cursors = new Cursor[8];
            }
            
            if (cursors[index] == null) {
                cursors[index] = new Cursor (type);
            }
            
            return cursors[index];
        }
        
        private void SetCursor (CursorType type)
        {
            Gdk.Cursor cursor = GetCursor (type);
            if (cursor == null) {
                ResetCursor ();
            } else {
                default_cursor = false;
                window.GdkWindow.Cursor = cursor;
            }
        }
        
        private void ResetCursor ()
        {
            if (!default_cursor) {
                window.GdkWindow.Cursor = null;
                default_cursor = true;
            }
        }
        
        private void DestroyCursors ()
        {
            if (cursors == null) {
                return;
            }
            
            for (int i = 0; i < cursors.Length; i++) {
                if (cursors[i] != null) {
                    cursors[i].Dispose ();
                }
            }
            
            cursors = null;
        }
        
#endregion

        protected Gtk.Window Window {
            get { return window; }
        }
        
        protected Gdk.Rectangle Allocation {
            get { return window.Allocation; }
        }

        public int TopMoveHeight {
            get { return top_move_height; }
            set { top_move_height = value; }
        }
        
        public int ResizeWidth {
            get { return resize_width; }
            set { resize_width = value; }
        }
    }
}
