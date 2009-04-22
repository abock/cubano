// 
// CubanoTrackTitleView.cs
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

using Hyena.Data;
using Hyena.Data.Gui;

using Banshee.Collection;
using Banshee.ServiceStack;
using Banshee.Gui;
using Banshee.Collection.Gui;
using Banshee.Query;

namespace Cubano.Client
{
    public class CubanoTrackTitleView : BaseTrackListView
    {
        private ColumnController column_controller;
        
        public CubanoTrackTitleView () : base ()
        {
            column_controller = new ColumnController ();

            CubanoTitleCell renderer = new CubanoTitleCell ();
            column_controller.Add (new Column (null, "indicator", new ColumnCellStatusIndicator (null), 0.05, true, 30, 30));
            column_controller.Add (new Column ("Track", renderer, 1.0));
            column_controller.Add (new Column ("Rating", new ColumnCellRating ("Rating", false), 0.15));
            
            ColumnController = column_controller;
            
            RowHeightProvider = renderer.ComputeRowHeight;
            HeaderVisible = false;
        }
    }
}
