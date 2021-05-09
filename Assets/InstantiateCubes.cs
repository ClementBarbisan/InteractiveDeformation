using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateCubes : MonoBehaviour
{
    private float _timeBetweenCubes = 2f;
    [SerializeField] private GameObject _gameObject;
    private float _timeElapsed = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _timeElapsed += Time.deltaTime;
        if (_timeElapsed > _timeBetweenCubes)
        {
            GameObject go = Instantiate(_gameObject, transform.position + new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f)), Quaternion.identity);
            // go.transform.localScale = transform.localScale;
            go.transform.SetParent(this.transform);
            _timeElapsed = 0;
            _timeBetweenCubes = Random.Range(0.1f, 0.5f);
        }
    }
}
