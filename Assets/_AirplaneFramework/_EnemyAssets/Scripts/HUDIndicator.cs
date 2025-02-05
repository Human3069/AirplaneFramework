using NPOI.SS.Formula.Functions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AFramework
{
    [RequireComponent(typeof(Damagable))]
    public class HUDIndicator : MonoBehaviour
    {
        [SerializeField]
        protected Camera mainCamera;
        [SerializeField]
        protected RectTransform inheritedPanel;

        [Space(10)]
        [SerializeField]
        protected Sprite hudSprite;
        [SerializeField]
        protected Vector2 spriteSize;

        protected Damagable inheritedDamagable;
        protected RectTransform instantiatedRectT;

        protected void OnEnable()
        {
            inheritedDamagable = this.GetComponent<Damagable>();

            GameObject _obj = new GameObject("HUD_" + this.gameObject.name);
            _obj.transform.SetParent(inheritedPanel);

            instantiatedRectT = _obj.AddComponent<RectTransform>();
            instantiatedRectT.sizeDelta = spriteSize;

            Image _image = _obj.AddComponent<Image>();
            _image.sprite = hudSprite;
            _image.color = new Color(1f, 1f, 1f, 0.3f);

            StartCoroutine(PostOnEnable());
        }

        protected IEnumerator PostOnEnable()
        {
            while (inheritedDamagable.IsDead == false)
            {
                instantiatedRectT.position = mainCamera.WorldToScreenPoint(this.transform.position);
                // instantiatedRectT.position = new Vector3(instantiatedRectT.position.x, instantiatedRectT.position.y, instantiatedRectT.position.z / 10f);

                yield return null;
            }

            instantiatedRectT.gameObject.SetActive(false);
        }
    }
}