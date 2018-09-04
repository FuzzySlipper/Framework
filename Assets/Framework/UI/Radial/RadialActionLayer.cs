using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace PixelComrades {
    public class RadialActionLayer : MenuActionLayer {
        private static GenericPool<RadialActionLayer> _actionPool = new GenericPool<RadialActionLayer>(15, action => { action.Clear(); });

        public static RadialActionLayer Get(string descr, System.Action cancelDel) {
            var menuRequest = _actionPool.New();
            menuRequest.Description = descr;
            menuRequest.CancelDel = cancelDel;
            return menuRequest;
        }

        private float _cursorOffset;
        private float _elementAngleDeg;
        private float _elementAngleRad;
        //private float _elementArcSize;
        //private float _savedIconScale = 1;

        public System.Action OnClose;

        private void Clear() {
            MenuAction.ClearActions(Actions);
            ClearList();
            CancelDel = null;
            Description = "";
            CurrentStatus = Status.Disabled;
            OnClose = null;
        }

        public override void Pool() {
            _actionPool.Store(this);
        }

        public override void Init(UIRadialMenu radialMenu, float targetAngle) {
            base.Init(radialMenu, targetAngle);
            ClearList();
            Radial.SetCenterText(Description);
            Populate(targetAngle);
            _cursorOffset = _elementAngleDeg / 2.0f;
            TransitionSetup(true);
        }

        public override void Confirm() {
            if (Actions[SelectedIndex].Del()) {
                Radial.StartCloseRadial();
            }
        }

        public override void Cancel() {
            //SelectedIndex = 0;
            if (!Radial.TransitionToPreviousLayer()) {
                if (CancelDel != null) {
                    CancelDel();
                }
            }
        }

        public override void Skip() {
            if (CurrentStatus == Status.Opening) {
                for (var i = 0; i < CurrentRadials.Count; i++) {
                    CurrentRadials[i].SetAlpha(1);
                    CurrentRadials[i].Lerp(1);
                }
                CheckTransitionStart(2);
            }
            else if (CurrentStatus == Status.Closing) {
                CheckTransitionEnd(2);
            }
        }

        public override void UpdateLayer(float targetAngle, UIRadialMenu.ControlMethod method) {
            int tempSelected = HandleCursor(targetAngle);
            if (tempSelected != SelectedIndex) {
                SelectedIndex = tempSelected;
                UpdateDisplayedText(Actions[SelectedIndex].Description, Actions[SelectedIndex].Icon, Color.white);
            }
            SetSelectedElementSize(targetAngle);
        }

        public override void Confirm(int overrideSelect) {
            SelectedIndex = Mathf.Clamp(overrideSelect, 0, Actions.Count-1);
            UpdateDisplayedText(Actions[SelectedIndex].Description, Actions[SelectedIndex].Icon, Color.white);
            Confirm();
        }

        public override void StartClose() {
            base.StartClose();
            if (OnClose != null) {
                OnClose();
            }
            Vector2[] positions = new Vector2[Actions.Count];
            for (int i = 0; i < positions.Length; i++) {
                positions[i] = new Vector2();
                var tempAngle = i * _elementAngleRad;
                positions[i].x = Radial.ScreenSize * Mathf.Sin(tempAngle);
                positions[i].y = Radial.ScreenSize * Mathf.Cos(tempAngle);
            }
            for (int i = 0; i < CurrentRadials.Count; i++) {
                var element = CurrentRadials[i];
                if (i == SelectedIndex) {
                    element.AssignPoints(null);
                    continue;
                }
                var pnts = new Vector2[4];
                bool greater = i > SelectedIndex;
                if (greater) {
                    pnts[0] = i < Actions.Count - 1 ? positions[i + 1] : positions[0];
                    pnts[3] = SelectedIndex > 0 ? positions[SelectedIndex - 1] : positions.LastElement();
                }
                else {
                    pnts[0] = i > 0 ? positions[i - 1] : positions.LastElement();
                    pnts[3] = SelectedIndex < Actions.Count-1 ? positions[SelectedIndex + 1] : positions[0];
                }
                pnts[1] = positions[i];
                pnts[2] = positions[SelectedIndex];
                element.AssignPoints(pnts);
            }
            TransitionSetup(false);
            CurrentStatus = Status.Closing;
        }

        public override bool TransitionComplete() {
            if (CurrentStatus == Status.Closing) {
                if (CurrentRadials.Count == 0 || Radial == null) {
                    return true;
                }
                //CheckCursorTop();
                return AnimationComplete(false);
            }
            if (CurrentStatus == Status.Opening) {
                if (CurrentAnimationPercent >= Radial.TransitionLength * 2.0f) {
                    return true;
                }
                return AnimationComplete(true);
            }
            return true;
        }
        
        private bool AnimationComplete(bool isStart) {
            CurrentAnimationPercent += Time.deltaTime;
            var percent = CurrentAnimationPercent / Radial.TransitionLength;
            CursorImage.fillAmount = Mathf.Lerp(SavedCursorFill, _elementAngleDeg / 360.0f, percent);
            //translate the percent progess into a rotation aeound the z axis to move in a clockwise direction
            //var zRot = isStart ? Mathf.Lerp(360.0f, 0.0f, percent) : Mathf.Lerp(0.0f, -360.0f, percent);
            //Radial.ElementsParent.transform.localRotation = Quaternion.Euler(0, isStart ? 0.0f : 360, zRot);
            for (int i = 0; i < CurrentRadials.Count; i++) {
                CurrentRadials[i].SetAlpha(Mathf.Lerp(isStart ? 0 : 1,  isStart ? 1 : 0, percent));
                CurrentRadials[i].Lerp(percent);
            }
            return isStart ? CheckTransitionStart(percent) : CheckTransitionEnd(percent);

        }

        private bool CheckTransitionStart(float percent) {
            if (percent < 1.0f) {
                return false;
            }
            if (Cursor.transform.GetSiblingIndex() != 0) {
                Cursor.transform.SetSiblingIndex(0);
            }
            SavedCursorRotation = SavedCursorFill = 0;
            //CursorIconHolder.gameObject.SetActive(false);
            CurrentStatus = Status.Open;
            return true;
        }

        private bool CheckTransitionEnd(float percent) {
            if (percent < 1 && CurrentRadials.Count > 0) {
                return false;
            }
            ClearList();
            SavedCursorRotation = 0;
            CurrentStatus = Status.Disabled;
            return true;
        }

        protected void Populate(float targetAngle) {
            if (Actions.Count <= 0) {
                Debug.LogError("There are no elements in the layer " + Description);
                return;
            }
            _elementAngleDeg = 360.0f / Actions.Count;
            _elementAngleRad = _elementAngleDeg * Mathf.Deg2Rad;
            //_elementArcSize = 2.0f * Mathf.PI * Radial.ScreenSize * (_elementAngleDeg / 360.0f);
            Vector2[] positions = new Vector2[Actions.Count];
            for (int i = 0; i < positions.Length; i++) {
                positions[i] = new Vector2();
                var tempAngle = i * _elementAngleRad;
                positions[i].x = Radial.ScreenSize * Mathf.Sin(tempAngle);
                positions[i].y = Radial.ScreenSize * Mathf.Cos(tempAngle);
            }
            SelectedIndex = HandleCursor(targetAngle);
            UpdateDisplayedText(Actions[SelectedIndex].Description, Actions[SelectedIndex].Icon, Color.white);
            int prevIndex = SelectedIndex == 0 ? Actions.LastIndex() : SelectedIndex - 1;
            for (var i = 0; i < Actions.Count; i++) {
                var element = Radial.CreateElement();
                CurrentRadials.Add(element);
                element.AssignParent(Radial, i, Actions[i]);
                element.RectTr.localPosition = positions[SelectedIndex];
                if (i == SelectedIndex) {
                    element.AssignPoints(null);
                    continue;
                }
                var pnts = new Vector2[4];
                pnts[0] = positions[prevIndex];
                pnts[1] = element.RectTr.localPosition;
                pnts[2] = positions[i];
                pnts[3] = i < Actions.Count - 1 ? positions[i + 1] : positions[0];
                element.AssignPoints(pnts);
            }
        }

        private void ClearList() {
            for (int i = 0; i < CurrentRadials.Count; i++) {
                ItemPool.Despawn(CurrentRadials[i].gameObject);
            }
            CurrentRadials.Clear();
        }

        private void SetSelectedElementSize(float angle) {
            for (var i = 0; i < CurrentRadials.Count; i++) {
                //Work out the degrees that this element is at round the circle
                var elementDegrees = i * _elementAngleDeg;
                //Work out the difference between the cursor angle and this element
                var angleDifference = Mathf.Abs(angle - elementDegrees);

                //if the difference is greater than 180 degrees then it is the wrong side of he circle and must be corrected
                if (angleDifference > 180.0f) {
                    angleDifference = 360.0f - angleDifference;
                }
                //if this is the selected element then it needs its scale setting based on the difference from the cursor
                if (i == SelectedIndex) {
                    CurrentRadials[i].SetScale(Mathf.Lerp(Radial.ElementGrowthFactor, 1.0f, angleDifference / _elementAngleDeg));
                }
                //otherwise it needs to be reset to its starting scale
                else {
                    CurrentRadials[i].ResetScale();
                }
            }
        }

        private int HandleCursor(float targetAngle) {
            //work out what the selected element is based on the target angle
            var selectedElement = (int)Mathf.Round(targetAngle / _elementAngleDeg);
            if (selectedElement >= Actions.Count || selectedElement < 0) {
                selectedElement = 0;
            }
            if (Radial.ElementSnapping) {
                CursorImage.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, 360.0f - (selectedElement * _elementAngleDeg - _cursorOffset));
            }
            else {
                CursorImage.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, 360.0f - (targetAngle - _cursorOffset));
            }
            return selectedElement;
        }

        //private void CheckCursorTop() {
        //    if (Cursor.transform.GetSiblingIndex() == 1) {
        //        return;
        //    }
            //reset the scale of the selected element
            //CurrentRadials[SelectedIndex].ResetScale();

            //get the position of the selected element
            //var tempPosition = new Vector2();
            //var tempAngle = SelectedIndex * _elementAngleRad;
            //tempPosition.x = Radial.ScreenSize * Mathf.Sin(tempAngle);
            //tempPosition.y = Radial.ScreenSize * Mathf.Cos(tempAngle);
            //set teh image on the cursor to that position
            //CursorIconImageRect.localPosition = tempPosition;
            //set the size of the image on the cursaor
            //CursorIconImageRect.Resize(Radial.ElementMaxSize, _elementArcSize);
            //set the scale to match the icon that it is replacing
            //CursorIconImageRect.localScale = new Vector3(_savedIconScale, _savedIconScale, _savedIconScale);
            //Copy the sprite from the selected element onto the image on the cursor
            //CursorIconImage.sprite = Actions[SelectedIndex].Icon;
            //Copy the sprite colour from the selected element onto the image on the cursor
            //CursorIconImage.color = Actions[SelectedElement].m_SpriteColour;
            //hide the selected element so that it doesn't fly round with the other elements during clean up
            //_currentRadials[_selectedElement].gameObject.SetActive(false);
            //set the cursor to be on top of the elements 
            //Cursor.transform.SetSiblingIndex(1);
            //show the icon on the cursor so that nothing visibly changes
            //CursorIconHolder.gameObject.SetActive(true);
        //}

        private void TransitionSetup(bool inTrans) {
            var percent = MathEx.Min(CurrentAnimationPercent / Radial.TransitionLength, 0.5f) * 2.0f;
            SavedCursorRotation = Cursor.transform.localRotation.eulerAngles.z;
            SavedCursorFill = CursorImage.fillAmount;
            if (!inTrans) {
                SelectedIndex = Mathf.Clamp(SelectedIndex, 0, CurrentRadials.Count - 1);
                //_savedIconScale = CurrentRadials[SelectedIndex].transform.GetChild(0).localScale.x;
            }
            // work out the percent progress as double the tracker variable over the limit effectively doubling the speed of the process
            CurrentAnimationPercent += Time.deltaTime;
            //if at the end of the setup
            //if (percent >= 0.99f) {
            //    percent = 1.0f;
            //    CurrentAnimationPercent = 0.0f;
            //    _currentElementIndex = inTrans ? 0 : _currentRadials.Count;
            //    //_setupPhase = false;
            //}
            float targetRotation;
            if (inTrans) {
                targetRotation = _elementAngleDeg / 2.0f;
                //lerp the cursor image fill amount to the new size denoted by the amount of elements
                CursorImage.fillAmount = Mathf.Lerp(SavedCursorFill, _elementAngleDeg / 360.0f, percent);
            }
            else {
                targetRotation = 360.0f - SelectedIndex * (360.0f / CurrentRadials.Count) + SavedCursorFill * 180.0f;
                if (Mathf.Abs(targetRotation - SavedCursorRotation) > 180) {
                    targetRotation -= 360.0f;
                }
                //CursorIconImageRect.localScale = Vector3.Lerp(new Vector3(_savedIconScale, _savedIconScale, _savedIconScale), Vector3.one, percent);
            }
            //lerp the cursor to the top of the menu
            Cursor.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Lerp(SavedCursorRotation, targetRotation, percent));
            //work out the angle to the center of the cursor in radians
            //var tempTheta = (360.0f - (Cursor.transform.localRotation.eulerAngles.z - CursorImage.fillAmount * 180.0f)) * Mathf.Deg2Rad;
            //position the image on the middle of the cursor in local space
            //CursorIconImageRect.localPosition = new Vector2(Radial.ScreenSize * Mathf.Sin(tempTheta), Radial.ScreenSize * Mathf.Cos(tempTheta));
            //correct the icon layer so that it has a z rotation of 0
            //CursorIconHolder.localRotation = Quaternion.Euler(0.0f, 0.0f, -Cursor.transform.localRotation.eulerAngles.z);
            //for (int i = 0; i < _currentRadials.Count; i++) {
            //    _currentRadials[i].SetAlpha(Mathf.Lerp(inTrans ? 0 : 1, inTrans ? 1 : 0, 0));
            //}
        }
    }
}
