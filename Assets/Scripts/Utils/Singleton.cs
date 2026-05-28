using UnityEngine;

namespace Singleton
{ // Generic Singleton base class to ensure a single instance of a MonoBehaviour-derived class 
  public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour    //the syntax for creating a new a singleton is Classname : Singleton<Classname> 
  {        
  // Static reference to the singleton instance
      private static T _instance;       
      public static T Instance // Property to access the singleton instance 
      {            
          get            
          {                
              // If the instance is not already set
              if (_instance == null)               
              {                    
                  _instance = FindFirstObjectByType<T>();// try finding an existing singleton of the same type
                  
                  if (_instance == null)//if singleton of same type does not exist create new instance
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

            // Otherwise, set this as the private reference to the instance
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