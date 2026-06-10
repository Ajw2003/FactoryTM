using System.Collections;
using Code.Scripts.EventSystems;
using Singleton;
using UnityEngine;
using UnityEngine.Rendering;

namespace Managers
{
    public class GlobalVolumeController : SingletonBase<GlobalVolumeController>
    {
        public Volume globalVolume; // Assign your Global Volume in the Inspector

        public float startingVolume;

        private Coroutine _coroutine;

        private float _tolerance = 0.05f;

        protected bool PersistBetweenScenes => false;

        private void Start()
        {
            startingVolume = globalVolume.weight;
            EventManager.Instance?.Subscribe(this, (GlobalVolumeEvent e) => StartAjustVolumeRoutine(e.Target, e.FadeIncrement));
            persistBetweenScenes = PersistBetweenScenes;
        }

        private void StartAjustVolumeRoutine(float targetWeight, float fadeIncrement)
        {
            if (_coroutine == null)
            {
                _coroutine = StartCoroutine(AjustVolumeRoutine(targetWeight, fadeIncrement));
            }
            else
            {
                StopCoroutine(_coroutine);
                _coroutine = StartCoroutine(AjustVolumeRoutine(targetWeight, fadeIncrement));
            }
        
        }

        private IEnumerator AjustVolumeRoutine(float targetWeight, float fadeIncrement)
        {
            if (globalVolume == null) yield return null;

            while (globalVolume.weight < targetWeight + _tolerance || globalVolume.weight > targetWeight - _tolerance)
            {
                globalVolume.weight = Mathf.Lerp(globalVolume.weight, targetWeight, fadeIncrement);
                yield return new WaitForSeconds(fadeIncrement);
            }
        }
    }
}
