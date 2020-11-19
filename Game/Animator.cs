using Nez;
using System;
using System.Collections.Generic;

namespace Game
{
	interface IFrame
    {
		void OnEnter();
    }

	class Animation<T>
		where T : IFrame
    {
		public T[] GetFrames() => _frames;
		T[] _frames;
		public float FrameRate { get; }

		public Animation(T[] frames, float fps)
        {
			_frames = frames;
			FrameRate = fps;
        }
    }

	class Animator<T> : Component, IUpdatable
		where T : IFrame
	{
		public enum LoopMode
		{
			/// <summary>
			/// Play the sequence in a loop forever [A][B][C][A][B][C][A][B][C]...
			/// </summary>
			Loop,

			/// <summary>
			/// Play the sequence once [A][B][C] then pause and set time to 0 [A]
			/// </summary>
			Once,

			/// <summary>
			/// Plays back the animation once, [A][B][C]. When it reaches the end, it will keep playing the last frame and never stop playing
			/// </summary>
			ClampForever,

			/// <summary>
			/// Play the sequence in a ping pong loop forever [A][B][C][B][A][B][C][B]...
			/// </summary>
			PingPong,

			/// <summary>
			/// Play the sequence once forward then back to the start [A][B][C][B][A] then pause and set time to 0
			/// </summary>
			PingPongOnce
		}

		public enum State
		{
			None,
			Running,
			Paused,
			Completed
		}

		/// <summary>
		/// fired when an animation completes, includes the animation name;
		/// </summary>
		public event Action<string> OnAnimationCompletedEvent;

		/// <summary>
		/// animation playback speed
		/// </summary>
		public float Speed = 1;

		/// <summary>
		/// the current state of the animation
		/// </summary>
		public State AnimationState { get; private set; } = State.None;

		/// <summary>
		/// the current animation
		/// </summary>
		//public SpriteAnimation CurrentAnimation { get; private set; }
		public Animation<T> CurrentAnimation { get; private set; }

		/// <summary>
		/// the name of the current animation
		/// </summary>
		public string CurrentAnimationName { get; private set; }

		/// <summary>
		/// index of the current frame in sprite array of the current animation
		/// </summary>
		public int CurrentFrame { get; set; }

		/// <summary>
		/// checks to see if the CurrentAnimation is running
		/// </summary>
		public bool IsRunning => AnimationState == State.Running;

		readonly Dictionary<string, Animation<T>> _animations = new Dictionary<string, Animation<T>>();

		float _elapsedTime;
		LoopMode _loopMode;
		int _lastFrameIndex = -1;


		public Animator()
		{ }

		public virtual void Update()
		{
			if (AnimationState != State.Running || CurrentAnimation == null)
				return;

			var animation = CurrentAnimation;
			var secondsPerFrame = 1 / (animation.FrameRate * Speed);
			var frames = animation.GetFrames();
			var iterationDuration = secondsPerFrame * frames.Length;
			var pingPongIterationDuration = frames.Length < 3 ? iterationDuration : secondsPerFrame * (frames.Length * 2 - 2);

			_elapsedTime += Time.DeltaTime;
			var time = Math.Abs(_elapsedTime);

			// Once and PingPongOnce reset back to Time = 0 once they complete
			if (_loopMode == LoopMode.Once && time > iterationDuration ||
				_loopMode == LoopMode.PingPongOnce && time > pingPongIterationDuration)
			{
				AnimationState = State.Completed;
				_elapsedTime = 0;
				CurrentFrame = 0;
				PlayFrame();
				OnAnimationCompletedEvent?.Invoke(CurrentAnimationName);
				return;
			}

			if (_loopMode == LoopMode.ClampForever && time > iterationDuration)
			{
				AnimationState = State.Completed;
				CurrentFrame = frames.Length - 1;
				PlayFrame();
				OnAnimationCompletedEvent?.Invoke(CurrentAnimationName);
				return;
			}

			// figure out which frame we are on
			int i = Mathf.FloorToInt(time / secondsPerFrame);
			int n = frames.Length;
			if (n > 2 && (_loopMode == LoopMode.PingPong || _loopMode == LoopMode.PingPongOnce))
			{
				// create a pingpong frame
				int maxIndex = n - 1;
				CurrentFrame = maxIndex - Math.Abs(maxIndex - i % (maxIndex * 2));
			}
			else
				// create a looping frame
				CurrentFrame = i % n;

			//Sprite = animation.Sprites[CurrentFrame];
			PlayFrame();
		}

		void PlayFrame()
        {
			var frame = CurrentAnimation.GetFrames()[CurrentFrame];
			if (CurrentFrame != _lastFrameIndex)
            {
				frame.OnEnter();
				_lastFrameIndex = CurrentFrame;
            }
			//frame.Animate();
        }

		public Animator<T> AddAnimation(string name, Animation<T> animation)
        {
			_animations[name] = animation;
			return this;
        }

		#region Playback

		/// <summary>
		/// plays the animation with the given name. If no loopMode is specified it is defaults to Loop
		/// </summary>
		public void Play(string name, LoopMode? loopMode = null)
		{
			CurrentAnimation = _animations[name];
			CurrentAnimationName = name;
			CurrentFrame = 0;
			_lastFrameIndex = -1;
			AnimationState = State.Running;

			PlayFrame();
			_elapsedTime = 0;
			_loopMode = loopMode ?? LoopMode.Loop;
		}

		/// <summary>
		/// checks to see if the animation is playing (i.e. the animation is active. it may still be in the paused state)
		/// </summary>
		public bool IsAnimationActive(string name) => CurrentAnimation != null && CurrentAnimationName.Equals(name);

		/// <summary>
		/// pauses the animator
		/// </summary>
		public void Pause() => AnimationState = State.Paused;

		/// <summary>
		/// unpauses the animator
		/// </summary>
		public void UnPause() => AnimationState = State.Running;

		/// <summary>
		/// stops the current animation and nulls it out
		/// </summary>
		public void Stop()
		{
			CurrentAnimation = null;
			CurrentAnimationName = null;
			CurrentFrame = 0;
			_lastFrameIndex = -1;
			AnimationState = State.None;
		}

		#endregion
	}
}
