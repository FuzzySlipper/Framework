using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UIPanelManager : MonoBehaviour {

        private int _openParameterId;
        private Animator _open;
        private GameObject _previouslySelected;

        private const string OpenTransitionName = "Open";
        private const string ClosedStateName = "Closed";

        void Awake() {
            _openParameterId = Animator.StringToHash(OpenTransitionName);
        }

        public void OpenPanel(Animator anim) {
            if (_open == anim) {
                return;
            }

            anim.gameObject.SetActive(true);
            var newPreviouslySelected = EventSystem.current.currentSelectedGameObject;

            anim.transform.SetAsLastSibling();

            CloseCurrent();

            _previouslySelected = newPreviouslySelected;

            _open = anim;
            _open.SetBool(_openParameterId, true);

            GameObject go = FindFirstEnabledSelectable(anim.gameObject);

            SetSelected(go);
        }

        static GameObject FindFirstEnabledSelectable(GameObject gameObject) {
            GameObject go = null;
            var selectables = gameObject.GetComponentsInChildren<Selectable>(true);
            for (int i = 0; i < selectables.Length; i++) {
                if (selectables[i].IsActive() && selectables[i].IsInteractable()) {
                    go = selectables[i].gameObject;
                    break;
                }
            }
            return go;
        }

        public void CloseCurrent() {
            if (_open == null) {
                return;
            }

            _open.SetBool(_openParameterId, false);
            SetSelected(_previouslySelected);
            TimeManager.StartUnscaled(DisablePanelDeleyed(_open));
            _open = null;
        }

        IEnumerator DisablePanelDeleyed(Animator anim) {
            bool closedStateReached = false;
            bool wantToClose = true;
            while (!closedStateReached && wantToClose) {
                if (!anim.IsInTransition(0)) {
                    closedStateReached = anim.GetCurrentAnimatorStateInfo(0).IsName(ClosedStateName);
                }
                wantToClose = !anim.GetBool(_openParameterId);
                yield return new WaitForEndOfFrame();
            }
            if (wantToClose) {
                anim.gameObject.SetActive(false);
            }
        }

        private void SetSelected(GameObject go) {
            EventSystem.current.SetSelectedGameObject(go);
        }
    }
}
