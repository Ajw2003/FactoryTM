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
                               
                  var singletonObject = new GameObject(typeof(T).Name);                    
                               
                  _instance = singletonObject.AddComponent<T>();                   
                               
                  DontDestroyOnLoad(singletonObject); // Make sure the singleton instance persists across scene loads   
                  
              }               
              return _instance; // return the instance
          }
      }    
        // Protected METHODS: -----------------------------------------------------------------------                
        protected virtual void Awake()        
        {            
            
            if (_instance == null)            
            {                
                _instance = this as T;                
                if (transform.parent != null) // If the object has a parent, detach it to prevent it from being destroyed
                {
                    transform.SetParent(null);
                }                
                DontDestroyOnLoad(gameObject); // Ensure the instance isn't destryoyed when loading a new scene     
            }            
            else            
            {              
                Destroy(gameObject); // If another instance already exists, destroy this one            
            }           
        }                
        
        // Clear the instance when the object is destroyed
        protected virtual void OnDestroy()       
        {
            if (_instance == this)
            {
                _instance = null;            
            }        
        }    
        
  }
  
}