using System;
using System.Collections;
using System.Collections.Generic;
using Code.Scripts.EventSystems;
using Singleton;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class StoryManager : SingletonBase<StoryManager>
    {
        private List<StoryMarkerSo> _discoveredMarkers = new List<StoryMarkerSo>();

        private void Start()
        {
            _discoveredMarkers.Clear();
            EventManager.Instance?.Subscribe(this, (StoryMarkerUnlockEvent e) => AddMarker(e.storyMarkerSo));
        }
    
        private void AddMarker(StoryMarkerSo marker)
        {
            _discoveredMarkers.Add(marker);
        }

        public bool CheckMarker(StoryMarkerSo marker)
        {
            if (_discoveredMarkers.Contains(marker))
            {
                return true;
            }
            return false;
        }
    }
}
