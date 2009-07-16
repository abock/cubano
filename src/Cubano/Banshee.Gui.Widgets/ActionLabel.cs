// 
// ActionLabel.cs
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
using Gdk;
using Gtk;

namespace Banshee.Gui.Widgets
{
    public class ActionLabel : Widget
    {
        private Pango.Layout layout;
        private int base_point_size;
        private int layout_width;
        private int layout_height;
        
        private Gdk.Window input_window;
        
        public event EventHandler Activated;
        
        public ActionLabel ()
        {
            CanFocus = true;
            CanActivate = true;
        }
        
#region Windowing/Widgetry
        
        protected override void OnRealized ()
        {
            WidgetFlags |= WidgetFlags.Realized | WidgetFlags.NoWindow;
            
            GdkWindow = Parent.GdkWindow;
            
            var attributes = new WindowAttr () {
                WindowType = Gdk.WindowType.Child,
                X = Allocation.X,
                Y = Allocation.Y,
                Width = Allocation.Width,
                Height = Allocation.Height,
                Wclass = WindowClass.InputOnly,
                EventMask = (int)(
                    EventMask.ButtonPressMask |
                    EventMask.ButtonReleaseMask |
                    EventMask.KeyPressMask | 
                    EventMask.KeyReleaseMask | 
                    EventMask.EnterNotifyMask |
                    EventMask.LeaveNotifyMask)
            };
            
            var attributes_mask = 
                WindowAttributesType.X | 
                WindowAttributesType.Y | 
                WindowAttributesType.Wmclass;
            
            input_window = new Gdk.Window (GdkWindow, attributes, attributes_mask) {
                UserData = Handle
            };
            
            base.OnRealized ();
        }
        
        protected override void OnUnrealized ()
        {
            WidgetFlags &= ~WidgetFlags.Realized;
            
            input_window.UserData = IntPtr.Zero;
            Hyena.Gui.GtkWorkarounds.WindowDestroy (input_window);
            input_window = null;
            
            base.OnUnrealized ();
        }
        
        protected override void OnMapped ()
        {
            WidgetFlags |= WidgetFlags.Mapped;
            input_window.Show ();
        }
        
        protected override void OnUnmapped ()
        {
            WidgetFlags &= ~WidgetFlags.Mapped;
            input_window.Hide ();
        }
        
        protected override void OnSizeAllocated (Rectangle allocation)
        {
            base.OnSizeAllocated (allocation);
            
            if (IsRealized) {
                input_window.MoveResize (allocation);
            }
        }
        
#endregion

#region Input

        private StateType pending_state;

        protected override bool OnEnterNotifyEvent (Gdk.EventCrossing evnt)
        {
            if (!CanActivate) {
                return base.OnEnterNotifyEvent (evnt);
            }
            
            pending_state = State;
            State = StateType.Prelight;
            
            return base.OnEnterNotifyEvent (evnt);
        }

        protected override bool OnLeaveNotifyEvent (Gdk.EventCrossing evnt)
        {
            if (CanActivate) {
                State = pending_state;
            }
            
            return base.OnLeaveNotifyEvent (evnt);
        }

        protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
        {
            return true;
        }

        protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
        {
            Activate ();
            return true;
        }
        
        protected override bool OnKeyReleaseEvent (Gdk.EventKey evnt)
        {
            if (evnt.Key == Gdk.Key.Return || evnt.Key == Gdk.Key.KP_Enter) {
                Activate ();
            }
            
            return true;
        }
        
        protected override bool OnFocusInEvent (Gdk.EventFocus evnt)
        {
            QueueDraw ();
            return base.OnFocusInEvent (evnt);
        }
        
        protected override bool OnFocusOutEvent (Gdk.EventFocus evnt)
        {
            QueueDraw ();
            return base.OnFocusOutEvent (evnt);
        }
        
        protected override void OnActivate ()
        {
            var handler = Activated;
            if (handler != null) {
                handler (this, EventArgs.Empty);
            }
        }
        
#endregion

#region UI/Text

        protected override void OnSizeRequested (ref Gtk.Requisition requisition)
        {
            layout.GetPixelSize (out layout_width, out layout_height);
            requisition.Width = layout_width;
            requisition.Height = layout_height;
        }

        protected override bool OnExposeEvent (Gdk.EventExpose evnt)
        {
            if (evnt.Window != GdkWindow || State == StateType.Insensitive) {
                return true;
            }
        
            int x, y;
            
            x = Allocation.X + (int)Math.Round ((Allocation.Width - layout_width) / 2.0);
            y = Allocation.Y + (int)Math.Round ((Allocation.Height - layout_height) / 2.0);
            
            if (HasFocus) {
                GdkWindow.DrawLine (Style.ForegroundGC (StateType.Normal), 
                    x + 2, y + layout_height - 2,
                    x + layout_width - 4, y + layout_height - 2);
            }
            
            Gtk.Style.PaintLayout (Style, GdkWindow, State, false, 
                evnt.Area, this, null, x, y, layout);
                
            return true;
        }
        
        private bool changing_style = false;
    
        protected override void OnStyleSet (Style previous)
        {
            base.OnStyleSet (previous);
            if (changing_style) {
                return;
            }
            
            changing_style = true;
            
            base_point_size = Style.FontDescription.Size;
            
            ModifyFg (StateType.Selected, Style.Text (StateType.Normal));
            ModifyFg (StateType.Normal, Hyena.Gui.Theming.GtkTheme.GetGdkTextMidColor (this));
            
            CreateLayout ();
            UpdateLayout ();
        
            changing_style = false;
        }
        
        private void CreateLayout ()
        {
            if (layout != null) {
                layout.Dispose ();
            }
            
            layout = new Pango.Layout (PangoContext);
            layout.FontDescription = Style.FontDescription.Copy ();
        }
        
        private void UpdateLayout ()
        {
            if (layout == null) {
                CreateLayout ();
            }
            
            layout.SetText (String.IsNullOrEmpty (text) ? String.Empty : text);
            layout.FontDescription.Size = (int)Math.Round (base_point_size * CurrentFontSizeEm);
            layout.FontDescription.Family = FontFamily;
            QueueResize ();
        }
        
#endregion
        
#region Properties

        private double default_font_size_em = 1.0;
        public double DefaultFontSizeEm {
            get { return default_font_size_em; }
            set { default_font_size_em = value; }
        }
        
        private double current_font_size_em = 1.0;
        public double CurrentFontSizeEm {
            get { return current_font_size_em; }
            set {
                current_font_size_em = value;
                UpdateLayout ();
            }
        }
                
        public double active_font_size_em = 1.0;
        public double ActiveFontSizeEm {
            get { return active_font_size_em; }
            set { active_font_size_em = value; }
        }
        
        private bool is_selected = false;
        public bool IsSelected {
            get { return is_selected; }
            set {
                is_selected = value;
                if (is_selected) {
                    pending_state = StateType.Selected;
                    if (State != StateType.Prelight) {
                        State = StateType.Selected;
                    }
                    CurrentFontSizeEm = ActiveFontSizeEm;
                } else {
                    pending_state = StateType.Normal;
                    if (State != StateType.Prelight) {
                        State = StateType.Normal;
                    }
                    CurrentFontSizeEm = DefaultFontSizeEm;
                }
            }
        }
        
        public bool CanActivate { get; set; }
        
        private string text;
        public string Text {
            get { return text; }
            set {
                text = value;
                UpdateLayout ();
            }
        }
        
        private string font_family;
        public string FontFamily {
            get { return font_family; }
            set {
                font_family = value;
                UpdateLayout ();
            }
        }

#endregion

    }
}
