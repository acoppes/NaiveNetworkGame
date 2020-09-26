using UnityEngine;
using UnityEngine.UI;

namespace Development
{
    public class ChangelogWindow : MonoBehaviour
    {
        [SerializeField]
        private Text text;

        [SerializeField]
        private Toggle autoOpenToggle;

        private void Start()
        {
            autoOpenToggle.onValueChanged.AddListener(OnToggle);
        }

        private void OnToggle(bool newValue)
        {
            PlayerPrefs.SetInt("NaiveNetworkGame.ChangelogAutoOpen", newValue ? 1 : 0);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            var changelogTextAsset = Resources.Load<TextAsset>("Changelog");
            text.text = changelogTextAsset.text;
            
            var autoOpenChangelog = PlayerPrefs.GetInt("NaiveNetworkGame.ChangelogAutoOpen", 1);
            autoOpenToggle.isOn = autoOpenChangelog == 1;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
