using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _KMH_Framework
{
    public class UI_PopupDialog : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_PopupDialog]</b></color> {0}";

        protected static UI_PopupDialog _instance;
        public static UI_PopupDialog Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [Header("PopupDialog Panel")]
        [SerializeField]
        protected GameObject popupDialogPanel;

        [Header("Title Panel")]
        [SerializeField]
        protected GameObject titlePanel;

        [Header("Texts")]
        [SerializeField]
        protected TMP_Text titleText;
        [SerializeField]
        protected TMP_Text contentText;

        [Header("Buttons")]
        [SerializeField]
        protected Button yesButton;
        [SerializeField]
        protected Button noButton;

        public delegate void ClickPopupDialogButtonResult(string type);
        public ClickPopupDialogButtonResult OnClickPopupDialogButtonResult;

        protected virtual void Awake()
        {
            Debug.Assert(popupDialogPanel.activeSelf == false);

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Instance Overlapped While <b>Awake()</b>");
                Destroy(this);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        public void Show(string content)
        {
            Show(content, "", 2);
        }

        public void Show(string content, int buttonCount = 2)
        {
            Show(content, "", buttonCount);
        }

        public void Show(string content, string title = "")
        {
            Show(content, title, 2);
        }

        public void Show(string content, string title = "", int buttonCount = 2)
        {
            Debug.LogFormat(LOG_FORMAT, "PopupDialog.Show()");

            if (string.IsNullOrEmpty(title) == true)
            {
                titlePanel.SetActive(false);
            }
            else
            {
                titleText.text = title;
                titlePanel.SetActive(true);
            }

            contentText.text = content;

            if (buttonCount == 1)
            {
                yesButton.gameObject.SetActive(true);
                noButton.gameObject.SetActive(false);
            }
            else if (buttonCount == 2)
            {
                yesButton.gameObject.SetActive(true);
                noButton.gameObject.SetActive(true);
            }
            else
            {
                Debug.Assert(false);
            }

            popupDialogPanel.SetActive(true);
        }

        public virtual void OnClickPopupDialogButton(string _type)
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<b>PopupDialog</b>Button(), _type : " + _type);

            Invoke_OnClickPopupDialogButtonResult(_type);
            popupDialogPanel.SetActive(false);
        }

        protected virtual void Invoke_OnClickPopupDialogButtonResult(string _buttonType)
        {
            if (OnClickPopupDialogButtonResult != null)
            {
                OnClickPopupDialogButtonResult(_buttonType);
            }
        }
    }
}