using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace NaiveNetworkGame.Common
{
    public class FixedNumbersLabel : MonoBehaviour
    {
        public Image[] numbers;

        // in order, like 0, 1, 2, ..., 9
        public Sprite[] numberSprites;

        public bool showZero = false;

        public void SetNumber(int number)
        {
            Assert.IsTrue(number >= 0, "FixedNumberLabel doesnt support negative numbers.");
            
            var hundreds = number / 100;
            var tens = (number % 100) / 10;
            var ones = (number % 10);

            var hundredsActive = hundreds != 0;
            var tensActive = hundredsActive || tens != 0;
            var onesActive = number > 0;

            if (showZero)
                onesActive = true;

            if (numbers[0] != null)
            {
                numbers[0].gameObject.SetActive(hundredsActive);

                if (hundredsActive)
                    numbers[0].sprite = numberSprites[hundreds];
            }

            numbers[1].gameObject.SetActive(tensActive);
            numbers[2].gameObject.SetActive(onesActive);

            if (tensActive)
                numbers[1].sprite = numberSprites[tens];

            if (onesActive)
                numbers[2].sprite = numberSprites[ones];
        }
    }
}