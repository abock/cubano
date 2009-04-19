// 
// AnimationManager.cs
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
using Hyena.Gui.Theatrics;

namespace Hyena.Gui.Canvas
{
    public abstract class Animation
    {
        private Actor<Animation> actor;
        internal protected Actor<Animation> Actor {
            get { return actor; }
            set { actor = value; }
        }
        
        private uint duration = 1000;
        public uint Duration {
            get { return duration; }
            set {
                duration = value;
                if (Actor != null) {
                    Actor.Reset (duration);
                }
            }
        }
    
        private CanvasItem item;
        public CanvasItem Item {
            get { return item; }
            set { item = value; }
        }
        
        private string property;
        public string Property {
            get { return property; }
            set { property = value; }
        }
        
        private bool is_expired;
        public bool IsExpired {
            get { return is_expired; }
            set { 
                is_expired = value;
                iterations = 0;
            }
        }
        
        private int iterations;
        
        private int repeat_times;
        public int RepeatTimes {
            get { return repeat_times; }
            set { repeat_times = value; }
        }
            
        public virtual bool Step (Actor<Animation> actor)
        {
            if (RepeatTimes > 0 && actor.Percent == 1) {
                if (iterations++ >= RepeatTimes) {
                    IsExpired = true;
                }
            }
            
            return !IsExpired;
        }
    }
    
    public delegate T AnimationComposeHandler<T> (Animation<T> animation, double percent);
    
    public abstract class Animation<T> : Animation
    {
        private AnimationComposeHandler<T> compose_handler;
        protected AnimationComposeHandler<T> ComposeHandler {
            get { return compose_handler; }
        }
        
        private Easing easing = Easing.Linear;
        protected Easing Easing {
            get { return easing; }
        }
        
        private bool from_set;
        protected bool FromSet {
            get { return from_set; }
        }
        
        private T from_value;
        public T FromValue {
            get { return from_value; }
            set {
                from_set = true;
                from_value = value;
            }
        }
    
        private T to_value;
        public T ToValue {
            get { return to_value; }
            set { to_value = value; }
        }
        
        private T start_state;
        public T StartState {
            get { return start_state; }
            protected set { start_state = value; }
        }
        
        public Animation<T> ClearFromValue ()
        {
            from_set = false;
            from_value = default (T);
            return this;
        }
        
        public Animation<T> Animate ()
        {
            StartState = FromSet ? FromValue : (T)Item[Property];
            return this;
        }
        
        public Animation<T> Animate (T toValue)
        {
            ClearFromValue ();
            ToValue = toValue;
            return Animate ();
        }
        
        public Animation<T> Animate (T fromValue, T toValue)
        {
            FromValue = fromValue;
            ToValue = toValue;
            return Animate ();
        }
                
        public Animation<T> Animate (string property, T toValue)
        {
            Property = property;
            return Animate (toValue);
        }
        
        public Animation<T> Animate (string property, T fromValue, T toValue)
        {
            Property = property;
            return Animate (fromValue, toValue);
        }
        
        public Animation<T> Compose (AnimationComposeHandler<T> handler)
        {
            compose_handler = handler;
            return this;
        }
        
        public Animation<T> ClearCompose ()
        {
            compose_handler = null;
            return this;
        }
        
        public Animation<T> To (T toValue)
        {
            ToValue = toValue;
            return Animate ();
        }
        
        public Animation<T> From (T fromValue)
        {
            FromValue = fromValue;
            return Animate ();
        }
        
        public Animation<T> Reverse ()
        {
            T from = FromValue;
            FromValue = ToValue;
            ToValue = from;
            return Animate ();
        }
        
        public Animation<T> Ease (Easing easing)
        {
            this.easing = easing;
            return this;
        }
        
        public Animation<T> Throttle (uint duration)
        {
            Duration = duration;
            return this;
        }
        
        public Animation<T> Repeat (int count)
        {
            RepeatTimes = count;
            return this;
        }
        
        public Animation<T> Expire ()
        {
            IsExpired = true;
            return this;
        }
    }
    
    public class DoubleAnimation : Animation<double>
    {
        public DoubleAnimation ()
        {
        }
        
        public DoubleAnimation (string property)
        {
            Property = property;
        }
        
        public override bool Step (Actor<Animation> actor)
        {
            if (!base.Step (actor)) {
                return false;
            }
            
            double result = ComposeHandler == null
                ? StartState + (ToValue * actor.Percent)
                : ComposeHandler (this, actor.Percent);
            
            result = Easing == Easing.Linear
                ? result
                : Choreographer.Compose (result, Easing);
            
            Item.SetValue<double> (Property, result);
            
            return true;
        }
    }
    
    public class AnimationManager
    {
        private static AnimationManager instance;
        public static AnimationManager Instance {
            get { return instance ?? (instance = new AnimationManager ()); }
        }
        
        private Stage<Animation> stage = new Stage<Animation> ();
    
        public AnimationManager ()
        {
            stage.Play ();
            stage.ActorStep += (actor) => actor.Target.Step (actor);
        }
        
        public void Animate (Animation animation)
        {
            animation.Actor = stage.Add (animation, animation.Duration);
            animation.IsExpired = false;
            animation.Actor.CanExpire = false;
        }
    }
}
