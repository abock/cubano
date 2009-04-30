//
// SeekableTrackInfoDisplay.cs
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

using Hyena.Gui.Canvas;
using Hyena.Gui.Theatrics;

using Banshee.Collection;
using Banshee.Sources;

namespace Banshee.Gui.Widgets
{
    public class SeekableTrackInfoDisplay : StackPanel
    {
        private static double text_opacity = 0.6;
    
        private CoverArtDisplay cover_art;
        private TextBlock title;
        private Slider seek_bar;
        private StackPanel time_bar;
        private TextBlock elapsed;
        private TextBlock seek_to;
        private TextBlock remaining;
        
        private DoubleAnimation transition_animation;
        private DoubleAnimation seek_to_animation;
        private uint transition_timeout;
        
        private int display_metadata_index;
        private int display_metadata_states = 3;
        
        public SeekableTrackInfoDisplay ()
        {
            Spacing = 3;
            
            Children.Add (cover_art = new CoverArtDisplay ());
            Children.Add (new StackPanel () {
                Orientation = Orientation.Vertical,
                Spacing = 4,
                Children = {
                    (title = new TextBlock () { Opacity = text_opacity }),
                    (seek_bar = new Slider ()),
                    (time_bar = new StackPanel () {
                        Spacing = 10,
                        Children = {
                            (elapsed = new TextBlock ()   { HorizontalAlignment = 0.0, Opacity = text_opacity + 0.25 }),
                            (seek_to = new TextBlock ()   { HorizontalAlignment = 0.5, Opacity = text_opacity + 0.25 }),
                            (remaining = new TextBlock () { HorizontalAlignment = 1.0, Opacity = text_opacity })
                        }
                    })
                }
            });
            
            seek_to.Opacity = 0;
            seek_to_animation = new DoubleAnimation ("Opacity");
            seek_to_animation.Repeat (1);
            
            seek_bar.PendingValueChanged += (o, e) => OnSeekPendingValueChanged (seek_bar.PendingValue);
            seek_bar.ValueChanged += (o, e) => OnSeekValueChanged (seek_bar.Value);
            
            UpdateMetadataDisplay ();
            BuildTransitionAnimation ();
        }
        
        private void BuildTransitionAnimation ()
        {
            transition_animation = new DoubleAnimation ("Opacity");
            transition_animation
                .Throttle (250)
                .Compose ((a, p) => {
                    var opacity = a.StartState == 0 ? p : 1 - p;
                    if (p == 1) {
                        if (a.StartState == 1) {
                            UpdateMetadataDisplay ();
                        }
                        
                        if (a.ToValue == 1) {
                            a.Expire ();
                        } else {
                            a.Reverse ();
                        }
                    }

                    return opacity * text_opacity;
                }).Ease (Easing.QuadraticInOut);
        }
        
        public void ResetTransitionTimeout ()
        {
        
        }
        
        public void StartTransitionTimeout ()
        {
            if (transition_timeout > 0) {
                StopTransitionTimeout ();
            }
            
            transition_timeout = GLib.Timeout.Add (5000, OnTransitionTimeout);
        }
        
        public void StopTransitionTimeout ()
        {
            if (transition_timeout > 0) {
                GLib.Source.Remove (transition_timeout);
                transition_timeout = 0;
            }
        }
        
        private bool OnTransitionTimeout ()
        {
            title.Animate (transition_animation).From (1).To (0).Start ();
            return true;
        }
        
        public void UpdateMetadataDisplay ()
        {
            if (CurrentTrack == null) {
                title.Text = String.Empty;
                return;
            }
            
            switch (display_metadata_index % display_metadata_states) {
                case 0: title.Text = CurrentTrack.DisplayTrackTitle; break;
                case 1: title.Text = CurrentTrack.DisplayArtistName; break;
                case 2: title.Text = CurrentTrack.DisplayAlbumTitle; break;
            }
            
            display_metadata_index++;
        }
        
        protected double TimeFromPercent (double percent)
        {
            return Duration * Math.Max (0, Math.Min (1, percent));
        }
        
        private void UpdateTick ()
        {
            seek_bar.InhibitValueChangeEvent ();
            seek_bar.Value = Math.Max (0, Math.Min (1, Duration > 0 ? Position / Duration : 0)); 
            seek_bar.UninhibitValueChangeEvent ();
            
            TimeSpan duration = TimeSpan.FromMilliseconds (Duration);
            TimeSpan position = TimeSpan.FromMilliseconds (Position);
            
            elapsed.Text = DurationStatusFormatters.ConfusingPreciseFormatter (position);
            remaining.Text = "-" + DurationStatusFormatters.ConfusingPreciseFormatter (duration - position);
        }
        
        public override void Arrange ()
        {
            base.Arrange ();
            time_bar.Margin = title.Margin = new Thickness (seek_bar.Margin.Left, 0, seek_bar.Margin.Right, 0);
        }
        
        public void UpdateCurrentTrack (TrackInfo track)
        {
            if (current_track != track) {
                current_track = track;
            }
            
            cover_art.UpdateCurrentTrack (track);
            display_metadata_index = 1;
            
            OnTransitionTimeout ();
            ResetTransitionTimeout ();
        }
        
        protected virtual void OnSeekPendingValueChanged (double value)
        {
            seek_to.Text = DurationStatusFormatters.ConfusingPreciseFormatter (
                TimeSpan.FromMilliseconds (TimeFromPercent (value)));
                
            ShowSeekToLabel ();
        }
        
        protected virtual void OnSeekValueChanged (double value)
        {
            HideSeekToLabel ();
        }
        
        private void ShowSeekToLabel ()
        {
            if (seek_bar.IsValueUpdatePending) {
                seek_to.Opacity = 1;
            }
        }
        
        private void HideSeekToLabel ()
        {
            seek_to.Opacity = 0;
        }
        
        private TrackInfo current_track;
        public TrackInfo CurrentTrack {
            get { return current_track; }
        }
        
        private double duration;
        public double Duration {
            get { return duration; }
            set {
                duration = value;
                UpdateTick ();
            }
        }
        
        private double position;
        public double Position {
            get { return position; }
            set { 
                position = value; 
                UpdateTick ();
            }
        }
    }
}
