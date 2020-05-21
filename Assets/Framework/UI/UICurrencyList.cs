using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UICurrencyList : MonoBehaviour, IOnCreate {

        [SerializeField] private PrefabEntity _currencyPrefab = null;
        [SerializeField] private Transform _currencyPivot = null;

        public void OnCreate(PrefabEntity entity) {
            var currencies = Currencies.GetIDs();
            for (int i = 0; i < currencies.Count; i++) {
                var prefab = ItemPool.SpawnUIPrefab(_currencyPrefab, _currencyPivot);
                prefab.GetComponentInChildren<Image>().overrideSprite = SpriteDatabase.CurrencyIcons.SafeAccess(i);
                var text = prefab.GetComponentInChildren<TextMeshProUGUI>();
                text.text = "";
                var currency = Player.GetCurrency(currencies[i]);
                currency.OnResourceChanged += () => { text.text = currency.Value.ToString("F0"); };
            }
        }
    }
}
