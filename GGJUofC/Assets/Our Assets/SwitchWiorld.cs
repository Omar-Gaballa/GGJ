using UnityEngine;

public class SwitchWiorld : MonoBehaviour
{
    [SerializeField] private GameObject m1;
    [SerializeField] private GameObject m2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Switch()
    {
        if (m1.activeSelf)
        {
            m1.SetActive(false);
            m2.SetActive(true);
        }
        else
        {
            m1.SetActive(true);
            m2.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Switch();
        }
        
    }
}
