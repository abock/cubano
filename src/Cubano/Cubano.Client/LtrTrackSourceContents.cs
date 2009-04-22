//
// CompositeTrackSourceContents.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//   Gabriel Burt <gburt@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
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
using System.Reflection;
using System.Collections.Generic;

using Gtk;
using Mono.Unix;

using Hyena.Data;
using Hyena.Data.Gui;
using Hyena.Widgets;

using Banshee.Sources;
using Banshee.ServiceStack;
using Banshee.Collection;
using Banshee.Collection.Database;
using Banshee.Configuration;
using Banshee.Gui;
using Banshee.Collection.Gui;
using Banshee.Sources.Gui;

namespace Cubano.Client
{
    public class LtrTrackSourceContents : CubanoSourceContents, ITrackModelSourceContents
    {
        private ArtistListView artist_view;
        private AlbumListView album_view;
        private CubanoTrackTitleView track_view;

        public LtrTrackSourceContents () : base ("cubanoalbumartist")
        {
        }

        protected override void InitializeViews ()
        {
            SetupMainView (track_view = new CubanoTrackTitleView ());
            SetupFilterView (artist_view = new ArtistListView ());
            SetupFilterView (album_view = new AlbumListView ());
        }
        
        protected override void ClearFilterSelections ()
        {
            if (artist_view.Model != null) {
                artist_view.Selection.Clear ();
            }
            
            if (album_view.Model != null) {
                album_view.Selection.Clear ();
            }
        }

        public void SetModels (TrackListModel track, IListModel<ArtistInfo> artist, 
            IListModel<AlbumInfo> album, IListModel<QueryFilterInfo<string>> genre)
        {
            SetModel (track);
            SetModel (artist);
            SetModel (album);
        }
        
        IListView<TrackInfo> ITrackModelSourceContents.TrackView {
            get { return track_view; }
        }

        public CubanoTrackTitleView TrackView {
            get { return track_view; }
        }

        public TrackListModel TrackModel {
            get { return (TrackListModel)track_view.Model; }
        }

        protected override bool ActiveSourceCanHasBrowser {
            get {
                if (!(ServiceManager.SourceManager.ActiveSource is ITrackModelSource)) {
                    return false;
                }
                
                return ((ITrackModelSource)ServiceManager.SourceManager.ActiveSource).ShowBrowser;
            }
        }

#region Implement ISourceContents

        public override bool SetSource (ISource source)
        {
            ITrackModelSource track_source = source as ITrackModelSource;
            IFilterableSource filterable_source = source as IFilterableSource;
            if (track_source == null) {
                return false;
            }
            
            this.source = source;
            
            SetModel (track_view, track_source.TrackModel);
            
            if (filterable_source != null && filterable_source.CurrentFilters != null) {
                foreach (IListModel model in filterable_source.CurrentFilters) {
                    if (model is IListModel<ArtistInfo>)
                        SetModel (artist_view, (model as IListModel<ArtistInfo>));
                    else if (model is IListModel<AlbumInfo>)
                        SetModel (album_view, (model as IListModel<AlbumInfo>));
                }
            }
            
            return true;
        }

        public override void ResetSource ()
        {
            source = null;
            SetModel (track_view, null);
            SetModel (artist_view, null);
            SetModel (album_view, null);
        }

#endregion

    }
}
