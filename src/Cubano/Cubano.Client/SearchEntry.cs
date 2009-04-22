// 
// SearchEntry.cs
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
using Gtk;
using Mono.Unix;

using Banshee.ServiceStack;
using Banshee.Gui;
using Banshee.Collection;

namespace Cubano.Client
{
    public class SearchEntry : Banshee.Widgets.SearchEntry
    {
        public SearchEntry ()
        {
            BuildSearchEntry ();
            
            InterfaceActionService uia = ServiceManager.Get<InterfaceActionService> ();
            if (uia != null) {
                Gtk.Action action = uia.GlobalActions["WikiSearchHelpAction"];
                if (action != null) {
                    MenuItem item = new SeparatorMenuItem ();
                    item.Show ();
                    Menu.Append (item);
                    
                    item = new ImageMenuItem (Stock.Help, null);
                    item.Activated += delegate { action.Activate (); };
                    item.Show ();
                    Menu.Append (item);
                }
            }
            
            SearchSensitive = false;
            Show ();
        }
        
        private struct SearchFilter
        {
            public int Id;
            public string Field;
            public string Title;
        }
        
        private Dictionary<int, SearchFilter> search_filters = new Dictionary<int, SearchFilter> ();
        
        private void AddSearchFilter (TrackFilterType id, string field, string title)
        {
            SearchFilter filter = new SearchFilter ();
            filter.Id = (int)id;
            filter.Field = field;
            filter.Title = title;
            search_filters.Add (filter.Id, filter);
        }
        
        private void BuildSearchEntry ()
        {
            AddSearchFilter (TrackFilterType.None, String.Empty, Catalog.GetString ("Artist, Album, or Title"));
            AddSearchFilter (TrackFilterType.SongName, "title", Catalog.GetString ("Track Title"));
            AddSearchFilter (TrackFilterType.ArtistName, "artist", Catalog.GetString ("Artist Name"));
            AddSearchFilter (TrackFilterType.AlbumTitle, "album", Catalog.GetString ("Album Title"));
            AddSearchFilter (TrackFilterType.Genre, "genre", Catalog.GetString ("Genre"));
            AddSearchFilter (TrackFilterType.Year, "year", Catalog.GetString ("Year"));

            SetSizeRequest (300, -1);

            foreach (SearchFilter filter in search_filters.Values) {
                AddFilterOption (filter.Id, filter.Title);
                if (filter.Id == (int)TrackFilterType.None) {
                    AddFilterSeparator ();
                }
            }

            FilterChanged += OnSearchEntryFilterChanged;
            ActivateFilter ((int)TrackFilterType.None);

            OnSearchEntryFilterChanged (this, EventArgs.Empty);
        }

        private void OnSearchEntryFilterChanged (object o, EventArgs args)
        {
            /* Translators: this is a verb (command), not a noun (things) */
            EmptyMessage = String.Format (Catalog.GetString ("Filter Results"));
            /*search_entry.EmptyMessage = String.Format (Catalog.GetString ("Filter on {0}"),
                search_entry.GetLabelForFilterID (search_entry.ActiveFilterID));*/

            string query = search_filters.ContainsKey (ActiveFilterID)
                ? search_filters[ActiveFilterID].Field
                : String.Empty;

            Query = String.IsNullOrEmpty (query) ? String.Empty : query + ":";

            Editable editable = InnerEntry as Editable;
            if (editable != null) {
                editable.Position = Query.Length;
            }
        }
               
        public bool SearchSensitive {
            get { return Sensitive; }
            set { Sensitive = value; }
        }
        
        public Banshee.Widgets.SearchEntry Entry {
            get { return this; }
        }
    }
}
