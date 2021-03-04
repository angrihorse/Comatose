using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeline : MonoBehaviour
{
    public List<IRewindable> components { get; private set; } = new List<IRewindable>();
    int count;

    [SerializeField] bool disposable;

    // Start is called before the first frame update
    void Awake()
    {        
        foreach (MonoBehaviour mono in GetComponents<MonoBehaviour>())
        {
            if (mono is IRewindable)
            {                
                components.Add((IRewindable)mono);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Chronos.Instance.isRewinding)
        {
            if (count > 0)
            {
                count -= 1;
                foreach (IRewindable component in components)
                {
                    component.Rewind();
                }
            } else if (disposable)
            {
                Destroy(gameObject);
            }            
        }
        else if (Chronos.Instance.playerActive)
        {
            count += 1;
            foreach (IRewindable component in components)
            {
                component.Record();
            }
        }
    }
}
