using NaiveNetworkGame.Common;
using UnityEngine;

namespace Scenes.UI
{
    public class TestFixedLabelSceneController : MonoBehaviour
    {
        public FixedNumberAnimator label1;
        public FixedNumberAnimator label2;

        public int number1;
        
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                number1 += 10;
            }
            
            if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                number1 += 25;
            }
            
            if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                number1 -= 10;
            }
            
            if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                number1 -= 25;
            }
            
            label1.SetNumber(number1);
            label2.SetNumber(number1);
        }
    }
}
