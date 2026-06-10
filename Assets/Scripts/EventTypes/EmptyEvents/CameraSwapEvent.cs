using UnityEngine;

namespace Code.Scripts.EventSystems.EventTypes.EmptyEvents
{
    public class CameraSwapEvent //classes are not interfaces. These are classes.
    {
        public Camera SwappingCamera { get; private set; }

    }
}
