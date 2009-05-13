// 
// CubanoMainMenu.cs
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

using Banshee.Gui;
using Banshee.ServiceStack;

namespace Cubano.Client
{
    public class CubanoMainMenu : HBox
    {
        public CubanoMainMenu ()
        {
            var action_service = ServiceManager.Get<InterfaceActionService> ();
            
            var import_button = new Button () {
                Image = new Image (Stock.Open, IconSize.Menu),
                Relief = ReliefStyle.None
            };
            
            var preferences_button = new Button (new Image (Stock.Preferences, IconSize.Menu)) {
                Relief = ReliefStyle.None
            };
            
            preferences_button.ShowAll ();
            
            PackStart (import_button, false, false, 0);
            PackStart (preferences_button, false, false, 0);
            
            action_service.GlobalActions["ImportAction"].ConnectProxy (import_button);
            action_service.GlobalActions["PreferencesAction"].ConnectProxy (preferences_button);
        }
    }
}
