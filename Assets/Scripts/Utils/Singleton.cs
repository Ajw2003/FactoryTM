using UnityEngine;

namespace Singleton
{ // Generic Singleton base class to ensure a single instance of a MonoBehaviour-derived class
  public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour    
  {        
  // Static reference to the singleton instance
      private static T _instance;        // Property to access the singleton instance
      public static T Instance        
      {            
          get            
          {                
              // If the instance is not already set, create a new instance
              if (_instance == null)               
              {                    
                  _instance = FindFirstObjectByType<T>();
                  
                  if (_instance == null)
                  {
                      var singletonObject = new GameObject(typeof(T).Name);                    
                               
                      _instance = singletonObject.AddComponent<T>();                   
                               
                      DontDestroyOnLoad(singletonObject); // Make sure the singleton instance persists across scene loads 
                  }
              }               
              return _instance; // return the instance
          }
      }    
        // Protected METHODS: -----------------------------------------------------------------------                
        protected virtual void Awake()        
        {            
            // If an instance already exists AND it is not this specific object, destroy this one
            if (_instance != null && _instance != this)            
            {              
                Destroy(gameObject); 
                return; // Prevent any further execution in Awake for the destroyed object
            }           

            // Otherwise, this is the official instance
            _instance = this as T;                
            
            // If the object has a parent, detach it to prevent it from being destroyed with its parent
            if (transform.parent != null) 
            {
                transform.SetParent(null);
            }                
            
            // Ensure the instance isn't destroyed when loading a new scene     
            DontDestroyOnLoad(gameObject); 
        }       
  }
  
}