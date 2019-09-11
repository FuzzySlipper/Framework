using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Sirenix.OdinInspector;
using UnityEngine.Playables;
using Mathf = UnityEngine.Mathf;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PixelComrades {

    [System.Serializable]
    public class AnimationClipHolder {
        [ValueDropdown("SignalsList")]
        public string Id;
        public AnimationClip[] Clips = new AnimationClip[0];
        public NormalizedFloatRange PlaySpeed = new NormalizedFloatRange(1,1);
        public int Fps;

        [HideInInspector] public bool EventTriggered;
        [HideInInspector] public bool Complete;
        [HideInInspector] public int CurrentIndex = 0;

        private ValueDropdownList<string> SignalsList() {
            return AnimationIds.GetDropdownList();
        }

        public void SetRandomIndex() {
            CurrentIndex = Clips.RandomIndex();
            Start = TimeManager.TimeUnscaled;
        }

        public AnimationClip CurrentClip { get { return Clips[CurrentIndex]; } }
        public float Start { get; private set; }
        public float Length { get { return CurrentClip != null ? CurrentClip.length : 1; } }
        public float Remaining { get { return (Start + Length) - TimeManager.TimeUnscaled; } }
    }

    public class PlayableClipState {
        public string Id { get; }
        public PlayableAsset Clip { get; }
        public bool EventTriggered;
        public bool Complete;
        public float TimeStarted;

        public float TimeRemaining {
            get { return Mathf.Clamp((TimeStarted + (float) Clip.duration) - TimeManager.Time, 0, 9999); }
        }

        public void Start() {
            Complete = EventTriggered = false;
            TimeStarted = TimeManager.Time;
        }

        public void Reset() {
            Complete = EventTriggered = false;
        }

        public PlayableClipState(PlayableAsset clip) {
            Clip = clip;
            Id = clip.name;
        }
    }


    [System.Serializable]
    public class AnimationClipState : ISerializable {
        public string Id;
        public string ClipName;
        public AnimationClip Clip;
        public float Fps = 12;
        public float PlaySpeedMultiplier;
        public bool[] RenderFrames = new bool[0];
        public string[] Events = new string[0];
        public float[] FrameLengths = new float[0];

        [HideInInspector] public bool EventTriggered;
        [HideInInspector] public bool Complete;

        public void ResetBools() {
            Complete = EventTriggered = false;
        }

        public AnimationClipState(){}

        public AnimationClipState(SerializationInfo info, StreamingContext context) {
            Id = info.GetValue(nameof(Id), Id);
            ClipName = info.GetValue(nameof(ClipName), ClipName);
            Fps = info.GetValue(nameof(Fps), Fps);
            RenderFrames = info.GetValue(nameof(RenderFrames), RenderFrames);
            Events = info.GetValue(nameof(Events), Events);
            FrameLengths = info.GetValue(nameof(FrameLengths), FrameLengths);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Id), Id);
            info.AddValue(nameof(ClipName), ClipName);
            info.AddValue(nameof(Fps), Fps);
            info.AddValue(nameof(RenderFrames), RenderFrames);
            info.AddValue(nameof(Events), Events);
            info.AddValue(nameof(FrameLengths), FrameLengths);
        }

        public int EventTotal() {
            int cnt = 0;
            for (int i = 0; i < Events.Length; i++) {
                if (!string.IsNullOrEmpty(Events[i])) {
                    cnt++;
                }
            }
            return cnt;
        }

        public void Play(Animator animator, float time) {
#if UNITY_EDITOR
            if (Clip != null && animator != null && !Application.isPlaying && AnimationMode.InAnimationMode()) {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(animator.gameObject, Clip, time);
                AnimationMode.EndSampling();
            }
            //else if (animator != null){
            //    var animancer = animator.gameObject.GetComponent<AnimancerController>();
            //    if (animancer != null) {
            //        animancer.StopAll();
            //        var state = animancer.GetOrCreateState(Clip);
            //        state.Resume();
            //        state.NormalizedTime = time;
            //        state.Pause();
            //    }
            //}
#endif
        }

        public void CheckArraysLength() {
            if (RenderFrames.Length == IndividualFrameLength) {
                if (Events.Length != RenderFrames.Length) {
                    System.Array.Resize(ref Events, RenderFrames.Length);
                }
                if (FrameLengths.Length != RenderFrames.Length) {
                    var fl = FrameLengths.Length;
                    System.Array.Resize(ref FrameLengths, RenderFrames.Length);
                    for (int i = fl; i < RenderFrames.Length; i++) {
                        FrameLengths[i] = 1;
                    }
                }
                return;
            }
            var oldCnt = RenderFrames.Length;
            System.Array.Resize(ref RenderFrames, IndividualFrameLength);
            System.Array.Resize(ref Events, IndividualFrameLength);
            System.Array.Resize(ref FrameLengths, IndividualFrameLength);
            if (RenderFrames.Length <= oldCnt) {
                return;
            }
            for (int i = oldCnt; i < RenderFrames.Length; i++) {
                RenderFrames[i] = true;
                Events[i] = AnimationEvents.None;
                FrameLengths[i] = 1;
            }
        }

        public int CalculateCurrentFrame(float animationTime) {
            CheckArraysLength();
            float timeCheck = 0;
            for (int i = 0; i < FrameLengths.Length; i++) {
                if (!RenderFrames[i]) {
                    continue;
                }
                timeCheck += SecondsPerFrame * FrameLengths[i];
                if (timeCheck >= animationTime) {
                    return i;
                }
            }
            return -1;
            //var frame = Mathf.FloorToInt(animationTime / SecondsPerFrame);
            //if (!RenderFrames.HasIndex(frame) || !RenderFrames[frame]) {
            //    WhileLoopLimiter.ResetInstance();
            //    while (WhileLoopLimiter.InstanceAdvance()) {
            //        frame = (frame + 1) % LastFrame;
            //        if (RenderFrames.HasIndex(frame) && RenderFrames[frame]) {
            //            break;
            //        }
            //    }
            //}
            //return frame;
        }

        public float ConvertFrameToAnimationTime(int frame) {
            //return Mathf.Min(frame * SecondsPerFrame, ClipLength);
            float timeCheck = 0;
            for (int i = 0; i < FrameLengths.Length; i++) {
                if (!RenderFrames[i]) {
                    continue;
                }
                if (i == frame) {
                    return timeCheck;
                }
                timeCheck += SecondsPerFrame * FrameLengths[i];

            }
            return Mathf.Min(timeCheck, ClipLength);
        }
        
        public float SecondsPerFrame { get { return 1 / Fps; } }
        public float AdjustedLength { get { return (NumberAnimationFrames * SecondsPerFrame) * PlaySpeedMultiplier; } }
        public int IndividualFrameLength { get { return ((int) (ClipLength * Fps)) + 1; } }
        public float ClipLength {
            get {
                return Clip != null ? Clip.length : 1;
            }
        }
        public int NumberAnimationFrames {
            get {
                CheckArraysLength();
                int animFrames = 0;
                for (int i = 0; i < RenderFrames.Length; i++) {
                    if (RenderFrames[i]) {
                        animFrames++;
                    }
                }
                return Mathf.Clamp(animFrames, 1, 199);
            }
        }
        public int LastFrame {
            get {
                CheckArraysLength();
                int lastFrame = 0;
                for (int i = 0; i < RenderFrames.Length; i++) {
                    if (RenderFrames[i]) {
                        lastFrame = i;
                    }
                }
                return lastFrame;
            }
        }
    }


    [System.Serializable]
    public class DirectionalAnimationClipHolder {
        
        [ValueDropdown("SignalsList")] public string Id;
        public DirectionalAnimation[] Clips;
        [HideInInspector] public bool EventTriggered = false;
        [HideInInspector] public bool Complete = false;
        [HideInInspector] public int CurrentIndex = 0;

        public void ResetBools() {
            EventTriggered = Complete = false;
        }

        private ValueDropdownList<string> SignalsList() {
            return AnimationIds.GetDropdownList();
        }

        public DirectionalAnimationClipHolder(){}

        public DirectionalAnimationClipHolder(string id, DirectionalAnimation[] clips) {
            Id = id;
            Clips = clips;
        }

        public void SetRandomIndex() {
            CurrentIndex = Clips.RandomIndex();
            Start = TimeManager.Time;
        }

        public DirectionalAnimation CurrentClip { get { return Clips[CurrentIndex]; } }
        public float Start { get; private set; }
        public float Length { get { return CurrentClip != null ? CurrentClip.LengthTime : 1; } }
        public float Remaining { get { return (Start + Length) - TimeManager.Time; } }
    }
}
