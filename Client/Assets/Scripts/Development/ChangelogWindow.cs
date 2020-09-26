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
            var changelogTextAsset = Resources.Load("Changelog", typeof(TextAsset)) as TextAsset;
            text.text = changelogTextAsset.text;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
