using UnityEngine;

namespace PixelComrades {
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundSprite : MonoBehaviour {
        void Update() {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            float width = sr.sprite.bounds.size.x;
            float height = sr.sprite.bounds.size.y;
            float scrHeight = Camera.main.orthographicSize * 2f;
            float scrWidth = scrHeight / Screen.height * Screen.width;
            transform.localScale = new Vector3(scrWidth / width, scrHeight / height, 1f);
        }
    }
}
