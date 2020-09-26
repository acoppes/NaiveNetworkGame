using UnityEngine;
using UnityEngine.UI;

namespace Development
{
    public class ChangelogWindow : MonoBehaviour
    {
        [SerializeField]
        private Text text;

        public void Open()
        {
            gameObject.SetActive(true);
            var changelogTextAsset = Resources.Load<TextAsset>("Changelog");
            text.text = changelogTextAsset.text;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
