using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public enum SpriteFacing {
        Fourway,
        Eightway,
        EightwayFlipped,
        FourwayFlipped,
    }

    [System.Serializable]
    public class DirectionalFrames {
        public DirectionsEight Side;
        public Sprite[] Frames = new Sprite[0];
        public Sprite this[int index] { get { return Frames[index]; } }
        public int Length { get { return Frames.Length; } }
    }

    [System.Serializable]
    public class DirectionalFrame {
        public DirectionsEight Side;
        public Sprite Frame;
    }

    [System.Serializable]
    public class AnimationState {
        public string StateName;
        public AnimationClip Clip;
        public float Fps = 12;
        public float PercentEvent = 1;
        public bool Loop;
        public bool GlobalFps = true;
        public float LengthMulti;
        public float Length { get { return Clip != null ? Clip.length * LengthMulti : LengthMulti; } }
    }

    public static class SpriteFacingControl {
        public const int MaxSideIndex = 8;
        public const int SideLength = 9;
        private const float HalfSideOffset = 0.75f;

        public static DirectionsEight GetCameraSide(SpriteFacing facing, Transform posTr, Transform forwardTr, float margin, out bool inMargin) {
            return GetSide(facing, GetAngle(posTr, forwardTr, Game.SpriteCamera.transform), margin, out inMargin);
        }

        public static DirectionsEight GetSide(Transform posTr, Transform forwardTr, Transform target, float margin, out bool inMargin) {
            return GetSide(SpriteFacing.Eightway, GetAngle(posTr, forwardTr, target), margin, out inMargin);
        }

        public static float GetAngle(Transform posTr, Transform forwardTr, Transform target) {
            Vector3 dir = Vector3.Normalize(new Vector3(target.position.x, 0, target.position.z) -
                                            new Vector3(posTr.position.x, 0, posTr.position.z));
            Vector3 trForward = forwardTr.forward;
            trForward.y = 0;
            var spriteFacingAngle = Vector3.Angle(dir, trForward) * -Mathf.Sign(Vector3.Cross(dir, trForward).y);
            return spriteFacingAngle;
        }

        public static DirectionsEight GetSide(SpriteFacing facing, float spriteFacingAngle, float margin, out bool inMargin) {
            switch (facing) {
                case SpriteFacing.Eightway:
                case SpriteFacing.EightwayFlipped:
                    return GetFacing(_dirRangesEight, spriteFacingAngle, margin, out inMargin);
                case SpriteFacing.Fourway:
                case SpriteFacing.FourwayFlipped:
                default:
                    return GetFacing(DirRangesFour, spriteFacingAngle, margin, out inMargin);
            }
        }

        private static DirectionsEight GetFacing(DirRange[] dirRanges, float spriteFacingAngle, float margin, out bool inMargin) {
            inMargin = false;
            for (int i = 0; i < dirRanges.Length; i++) {
                if (dirRanges[i].IsInRange(spriteFacingAngle)) {
                    inMargin = dirRanges[i].IsInMargin(margin, spriteFacingAngle);
                    return dirRanges[i].Dir;
                }
            }
            return DirectionsEight.Front;
        }

        private static DirRange[] _dirRangesEight = new DirRange[] {
            new DirRange(DirectionsEight.Front, -22.5f, 22.5f),
            new DirRange(DirectionsEight.FrontRight, 22.5f, 67.5f),
            new DirRange(DirectionsEight.Right, 67.5f, 112.5f),
            new DirRange(DirectionsEight.RearRight, 112.5f, 157.5f),
            new RearDirRange(157.5f, 190.0f),
            new DirRange(DirectionsEight.RearLeft, -157.5f, -112.5f),
            new DirRange(DirectionsEight.Left, -112.5f, -67.5f),
            new DirRange(DirectionsEight.FrontLeft, -67.5f, -22.5f),
            //new DirRange(DirectionsEight.Top, 0, 999),
            //new DirRange(DirectionsEight.Bottom, -999, 0),
        };

        public static DirRange[] DirRangesFour = new DirRange[] {
            new DirRange(DirectionsEight.Front, -FourwayForwardLimit, FourwayForwardLimit),
            new DirRange(DirectionsEight.Right, FourwayForwardLimit, FourwayRearLimit),
            new DirRange(DirectionsEight.Left, -FourwayRearLimit, -FourwayForwardLimit),
            new RearDirRange(135f, 190f), 
        };


        private const float FourwayForwardLimit = 45f;
        private const float FourwayRearLimit = 135f;

        public static bool ValidSide(this SpriteFacing facing, DirectionsEight side) {
            if (facing == SpriteFacing.Eightway) {
                return side != DirectionsEight.Top;
            }
            switch (side) {
                case DirectionsEight.Front:
                case DirectionsEight.Right:
                case DirectionsEight.Rear:
                    return true;
                case DirectionsEight.Left:
                    return facing == SpriteFacing.Eightway || facing == SpriteFacing.Fourway;
            }
            if (facing == SpriteFacing.EightwayFlipped) {
                switch (side) {
                    case DirectionsEight.FrontRight:
                    case DirectionsEight.RearRight:
                        return true;
                }
            }
            if (facing == SpriteFacing.Eightway) {
                return true;
            }
            return false;
        }

        public static DirectionsEight GetFlippedSide(this DirectionsEight facing) {
            switch (facing) {
                case DirectionsEight.FrontLeft:
                    return DirectionsEight.FrontRight;
                case DirectionsEight.Left:
                    return DirectionsEight.Right;
                case DirectionsEight.RearLeft:
                    return DirectionsEight.RearRight;
            }
            return facing;
        }

        public static bool RequiresFlipping(this SpriteFacing facing) {
            switch (facing) {
                case SpriteFacing.EightwayFlipped:
                case SpriteFacing.FourwayFlipped:
                    return true;
            }
            return false;
        }

        public static bool IsFlipped(this DirectionsEight facing) {
            switch (facing) {
                case DirectionsEight.FrontLeft:
                case DirectionsEight.Left:
                case DirectionsEight.RearLeft:
                    return true;
            }
            return false;
        }


        public static void SetCameraPos(Camera camera, DirectionsEight index, float viewOffset, float heightOffset) {
            switch (index) {
                case DirectionsEight.Top:
                    camera.transform.localPosition = new Vector3(0, viewOffset * 0.75f, 0);
                    camera.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    break;
                default:
                case DirectionsEight.Front:
                    camera.transform.localPosition = new Vector3(0, 0, viewOffset);
                    camera.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    break;
                case DirectionsEight.FrontRight:
                    camera.transform.localPosition = new Vector3(
                        viewOffset * HalfSideOffset, 0,
                        viewOffset * HalfSideOffset);
                    camera.transform.localRotation = Quaternion.Euler(0, -135, 0);
                    break;
                case DirectionsEight.Right:
                    camera.transform.localPosition = new Vector3(viewOffset, 0, 0);
                    camera.transform.localRotation = Quaternion.Euler(0, -90, 0);
                    break;
                case DirectionsEight.RearRight:
                    camera.transform.localPosition = new Vector3(
                        viewOffset * HalfSideOffset, 0,
                        -viewOffset * HalfSideOffset);
                    camera.transform.localRotation = Quaternion.Euler(0, -45, 0);
                    break;
                case DirectionsEight.Rear:
                    camera.transform.localPosition = new Vector3(0, 0, -viewOffset);
                    camera.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case DirectionsEight.RearLeft:
                    camera.transform.localPosition = new Vector3(
                        -viewOffset * HalfSideOffset, 0,
                        -viewOffset * HalfSideOffset);
                    camera.transform.localRotation = Quaternion.Euler(0, 45, 0);
                    break;
                case DirectionsEight.Left:
                    camera.transform.localPosition = new Vector3(-viewOffset, 0, 0);
                    camera.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    break;
                case DirectionsEight.FrontLeft:
                    camera.transform.localPosition = new Vector3(
                        -viewOffset * HalfSideOffset, 0,
                        viewOffset * HalfSideOffset);
                    camera.transform.localRotation = Quaternion.Euler(0, 135, 0);
                    break;
            }
            camera.transform.position += new Vector3(0, heightOffset, 0);
        }

        public class DirRange : FloatRange {
            public DirectionsEight Dir;

            public DirRange(DirectionsEight dir, float min, float max) {
                Dir = dir;
                Min = min;
                Max = max;
            }
        }

        public class RearDirRange : DirRange {
            public RearDirRange(float min, float max) : base(DirectionsEight.Rear, min, max) {
                Min = min;
                Max = max;
            }

            public override bool IsInRange(float input) {
                return base.IsInRange(Mathf.Abs(input));
            }

            public override bool IsInMargin(float margin, float input) {
                input = Mathf.Abs(input);
                if (input < Min || input > Max) {
                    return false;
                }
                if (input < (Min + margin)) {
                    return true;
                }
                return false;
            }
        }

        //private static DirectionsEight GetFourwayFacing(float spriteFacingAngle) {
        //    if (spriteFacingAngle > 0.0f && spriteFacingAngle < FourwayForwardLimit ||
        //        spriteFacingAngle < 0.0f && spriteFacingAngle > -FourwayForwardLimit) {
        //        return DirectionsEight.Front;
        //    }
        //    if (spriteFacingAngle >= FourwayForwardLimit && spriteFacingAngle <= FourwayRearLimit) {
        //        return DirectionsEight.Right;
        //    }
        //    if (spriteFacingAngle > FourwayRearLimit || spriteFacingAngle < -FourwayRearLimit) {
        //        return DirectionsEight.Rear;
        //    }
        //    if (spriteFacingAngle < -FourwayForwardLimit && spriteFacingAngle > -FourwayRearLimit) {
        //        return DirectionsEight.Left;
        //    }
        //    return DirectionsEight.Front;
        //}

        //private static DirectionsEight GetFourNoRear(float spriteFacingAngle) {
        //    if (spriteFacingAngle > 0.0f && spriteFacingAngle <= FourwayForwardLimit ||
        //        spriteFacingAngle < 0.0f && spriteFacingAngle >= -FourwayForwardLimit) {
        //        return DirectionsEight.Front;
        //    }
        //    if (spriteFacingAngle >= FourwayForwardLimit) {
        //        return DirectionsEight.Right;
        //    }
        //    if (spriteFacingAngle < -FourwayForwardLimit) {
        //        return DirectionsEight.Left;
        //    }
        //    return DirectionsEight.Front;
        //}

        //public static DirectionsEight GetEightwayFacing(float spriteFacingAngle, float margin, out bool inMargin) {
        //if (spriteFacingAngle > 0.0f && spriteFacingAngle < 22.5f) {
        //    return DirectionsEight.Front;
        //}

        //if (spriteFacingAngle > 22.5f && spriteFacingAngle < 67.5f) {
        //    return DirectionsEight.FrontRight;
        //}

        //if (spriteFacingAngle > 67.5f && spriteFacingAngle < 112.5f) {
        //    return DirectionsEight.Right;
        //}

        //if (spriteFacingAngle > 112.5f && spriteFacingAngle < 157.5f) {
        //    return DirectionsEight.RearRight;
        //}

        //if (spriteFacingAngle > 157.5f && spriteFacingAngle < 180.0f) {
        //    return DirectionsEight.Rear;
        //}

        //if (spriteFacingAngle < 0.0f && spriteFacingAngle > -22.5f) {
        //    return DirectionsEight.Front;
        //}

        //if (spriteFacingAngle < -22.5f && spriteFacingAngle > -67.5f) {
        //    return DirectionsEight.FrontLeft;
        //}

        //if (spriteFacingAngle < -67.5f && spriteFacingAngle > -112.5f) {
        //    return DirectionsEight.Left;
        //}

        //if (spriteFacingAngle < -112.5f && spriteFacingAngle > -157.5f) {
        //    return DirectionsEight.RearLeft;
        //}

        //if (spriteFacingAngle < -157.5f && spriteFacingAngle > -190.0f) {
        //    return DirectionsEight.Rear;
        //}
        //return DirectionsEight.Front;
        //}

    }
}
