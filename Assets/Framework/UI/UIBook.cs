using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace PixelComrades {
    public class UIBook : MonoSingleton<UIBook> {

        public static void ReadBook(StaticTextHolder text) {
            main.StartNewText(text);
        }

        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private TextMeshProUGUI _titleText = null;
        [SerializeField] private TextMeshProUGUI _bodyText = null;
        [SerializeField] private GameObject _nextButton = null;
        [SerializeField] private GameObject _prevButton = null;
        [SerializeField] private int _maxPageCharCount = 850;
        [SerializeField] private AudioClip _flipNoise = null;
        [SerializeField] private AudioClip _openNoise = null;
        [SerializeField] private AudioClip _closeNoise = null;

        private List<string> _activePages = new List<string>();
        private int _pageNo;

        private void StartNewText(StaticTextHolder text) {
            _titleText.text = text.Label;
            if (text.Text.Length <= _maxPageCharCount) {
                _bodyText.text = text.Text;
                _prevButton.gameObject.SetActive(false);
                _nextButton.gameObject.SetActive(false);
            }
            else {
                _activePages.AddRange(SplitOnLength(text.Text, _maxPageCharCount));
                LoadCurrentPage();
            }
            _canvasGroup.SetActive(true);
            if (_openNoise) {
                AudioSource.PlayClipAtPoint(_openNoise, transform.position);
            }
        }

        private void LoadCurrentPage() {
            _nextButton.gameObject.SetActive(_pageNo != _activePages.Count - 1);
            _prevButton.gameObject.SetActive(_pageNo > 0);
            _bodyText.text = _activePages[_pageNo];
        }

        public void Close() {
            _activePages.Clear();
            _pageNo = 0;
            _canvasGroup.SetActive(false);
            if (_closeNoise) {
                AudioSource.PlayClipAtPoint(_closeNoise, transform.position);
            }
        }

        public void NextPage() {
            if (_pageNo == _activePages.Count-1) {
                return;
            }
            if (_flipNoise) {
                AudioSource.PlayClipAtPoint(_flipNoise, transform.position);
            }
            _pageNo++;
            LoadCurrentPage();
        }

        public void PrevPage() {
            if (_pageNo < 0) {
                return;
            }
            if (_flipNoise) {
                AudioSource.PlayClipAtPoint(_flipNoise, transform.position);
            }
            _pageNo--;
            LoadCurrentPage();
        }

        public IEnumerable<string> SplitOnLength(string input, int length) {
            int index = 0;
            while (index < input.Length) {
                if (index + length < input.Length) {
                    yield return input.Substring(index, length);
                }
                else {
                    yield return input.Substring(index);
                }
                index += length;
            }
        }

    }
}
