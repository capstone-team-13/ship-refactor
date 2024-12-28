using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    public class WaitForSeconds : ActionTask
    {
        public BBParameter<float> waitTime = 1f;

        private float m_timeElapsed;

        protected override string info => $"Wait {waitTime} sec.";

        protected override void OnUpdate()
        {
            m_timeElapsed += Time.deltaTime;

            if (m_timeElapsed >= waitTime.value)
            {
                m_timeElapsed = 0f;
                EndAction(true);
            }
        }
    }
}