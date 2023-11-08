using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArmor : MonoBehaviour
{
    [SerializeField] private List<GameObject> armor = new List<GameObject>();

    public int _armorValue;

    [SerializeField] private Collider armorCollider;
    [SerializeField] private float _radius;
    [SerializeField] private float _force;
    [SerializeField] private float _upwards;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_armorValue < 0)
        {
            _armorValue = 0;
        }


        SetArmor();
    }

    private void PickUpArmor()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("ArmorPlate"))
        {
            if (_armorValue < 30)
            {
                _armorValue += 1;
                Destroy(collision.gameObject);
            }
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("ArmorPlate"))
    //    {
    //        if (_armorValue < 30)
    //        {
    //            _armorValue += 1;
    //            Destroy(other.gameObject);
    //        }
    //    }
    //}

    private void SetArmor()
    {
        for (int i = 0; i < armor.Count; i++)
        {
            armor[i].SetActive(_armorValue >= (i+1) * 3);
        }
    }
}
