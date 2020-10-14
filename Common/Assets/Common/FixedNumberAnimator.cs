using UnityEngine;

namespace NaiveNetworkGame.Common
{
    public class FixedNumberAnimator : MonoBehaviour
    {
        [SerializeField]
        private FixedNumbersLabel label;
        
        public float numberPerSecond = 10000;

        public float speedMultiplier = 0.25f;
        
        private float currentNumber = -1;
        private float desiredNumber;

        public void SetNumber(int number)
        {
            desiredNumber = number;

            if (currentNumber == -1)
            {
                currentNumber = number;
            }
        }

        private void LateUpdate()
        {
            var distance = Mathf.Abs(desiredNumber - currentNumber);
            
            if (distance < 0.01f)
                return;

            var value = (desiredNumber - currentNumber) * speedMultiplier;

            if (Mathf.Abs(value) < 1)
            {
                if (value > 0)
                    value = 1;
                else
                    value = -1;
            }

            // var dir = Mathf.Sign(desiredNumber - currentNumber);
            var nextNumber = currentNumber + value * numberPerSecond * Time.deltaTime;

            if (Mathf.Abs(nextNumber - currentNumber) > distance)
            {
                nextNumber = desiredNumber;
            }

            currentNumber = nextNumber;

            var number = Mathf.RoundToInt(currentNumber);
            if (number > 0)
            label.SetNumber(number);
        }

    }
}