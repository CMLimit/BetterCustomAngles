using TMPro;
using UnityEngine;

namespace BetterCustomAngles
{
    public class AngleInputWatcher : MonoBehaviour
    {
        public TMP_InputField Input;

        private bool wasFocused;

        private void Start()
        {
            wasFocused = Input != null && Input.isFocused;
        }

        private void LateUpdate()
        {
            if (Input == null)
            {
                return;
            }

            bool focused = Input.isFocused;
            if (wasFocused && !focused && AngleUtil.TryParse(Input.text, out float value))
            {
                Input.text = AngleUtil.Format(value);
            }
            wasFocused = focused;
        }
    }
}
