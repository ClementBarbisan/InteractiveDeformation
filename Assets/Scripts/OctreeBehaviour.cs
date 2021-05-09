using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class OctreeBehaviour : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private BoundsOctree<int> _octreePoints;
    private List<Vector3> _initPos;
    private List<Color> _colors;
    private List<Vector3> _pos;
    private ComputeBuffer _buffInitPos;
    private ComputeBuffer _buffPos;
    private ComputeBuffer _impactPos;
    private ComputeBuffer _buffColor;
    [SerializeField]private ComputeShader _computeKernel;
    private int _countVertices = 0;
    private List<int> _currentIndexes = new List<int>();
    private List<Vector3> _impactList;
    private int _kernelId;
    private ComputeBuffer _buffInitNormal;
    private List<Vector3> _initNormal;
    [SerializeField] private float _drag = 0.1f;
    [SerializeField] private float _elasticity = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _computeKernel = Instantiate(_computeKernel);
        _countVertices = _meshFilter.mesh.vertexCount;
        _initPos = new List<Vector3>();
        _initNormal = new List<Vector3>();
        _impactList = new List<Vector3>();
        _pos = new List<Vector3>();
        _colors = new List<Color>();
        _buffInitPos = new ComputeBuffer(_countVertices, sizeof(float) * 3);
        _buffInitNormal = new ComputeBuffer(_countVertices, sizeof(float) * 3);
        _buffPos = new ComputeBuffer(_countVertices, sizeof(float) * 3);
        _impactPos = new ComputeBuffer(_countVertices, sizeof(float) * 3); 
        _buffColor = new ComputeBuffer(_countVertices, sizeof(float) * 4);
        _kernelId = _computeKernel.FindKernel("CSMain");
        _computeKernel.SetBuffer(_kernelId, "_VertexBuffer", _buffPos);
        _computeKernel.SetBuffer(_kernelId, "_InitialPositionBuffer", _buffInitPos);
        _computeKernel.SetBuffer(_kernelId, "_InitialNormalBuffer", _buffInitNormal);
        _computeKernel.SetBuffer(_kernelId, "_ImpactPositionBuffer", _impactPos);
        _computeKernel.SetFloat("_drag", _drag);
        _computeKernel.SetFloat("_elacticity", _elasticity);
        _computeKernel.SetBuffer(_kernelId, "_ColorsBuffer", _buffColor);
        _meshFilter = GetComponent<MeshFilter>();
        _octreePoints = new BoundsOctree<int>(1, Vector3.zero, 0.01f, 1);
        Mesh tmpMesh = _meshFilter.mesh;
        MeshRenderer render = GetComponent<MeshRenderer>();
        for (int i = 0; i < tmpMesh.vertexCount; i++)
        {
            _initPos.Add(tmpMesh.vertices[i]);
            _pos.Add(tmpMesh.vertices[i]);
            _initNormal.Add(tmpMesh.normals[i]);
            _impactList.Add(Vector3.zero);
            _colors.Add(((Texture2D)render.material.GetTexture("_MetallicGlossMap")).GetPixelBilinear(i % render.material.mainTexture.width, i / render.material.mainTexture.height));
            _octreePoints.Add(i, new Bounds((Quaternion.Euler(transform.rotation.eulerAngles) * Vector3.Scale(tmpMesh.vertices[i], transform.localScale)) + transform.position, Vector3.one * 0.01f));
        }
        _buffInitPos.SetData(_initPos.ToArray());
        _buffInitNormal.SetData(_initNormal.ToArray());
        _buffPos.SetData(_pos.ToArray());
        _buffColor.SetData(_colors.ToArray());
        _meshFilter.mesh.MarkDynamic();
        
    }

    private void OnApplicationQuit()
    {
        _buffColor.Release();
        _buffPos.Release();
        _buffInitPos.Release();
        _buffInitNormal.Release();
        _impactPos.Release();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3[] tmpVertices = new Vector3[_countVertices];
        Vector3[] tmpImpact = _impactList.ToArray();
        _impactPos.SetData(tmpImpact);
        _computeKernel.Dispatch(_kernelId, 512, 1,1);
        _buffPos.GetData(tmpVertices);
        _impactPos.GetData(tmpImpact);
        _impactList = tmpImpact.ToList();
        _meshFilter.mesh.SetVertices(tmpVertices);
        _meshFilter.mesh.MarkModified();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            _octreePoints.DrawAllBounds();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        List<int> tmpIndexes = new List<int>();
        _octreePoints.GetColliding(tmpIndexes, new Bounds(other.transform.position, other.transform.localScale / 5));
        foreach (int index in tmpIndexes)
        {
            _impactList[index] = other.transform.position;
           
        }
    }

    private void OnCollisionStay(Collision other)
    {
        List<int> tmpIndexes = new List<int>();
        _octreePoints.GetColliding(tmpIndexes, new Bounds(other.transform.position, other.transform.localScale / 5));
        foreach (int index in tmpIndexes)
        {
            _impactList[index] = other.transform.position;
          
        }
    }
    
    private void OnCollisionExit(Collision other)
    {
        List<int> tmpIndexes = new List<int>();
        _octreePoints.GetColliding(tmpIndexes, new Bounds(other.transform.position, other.transform.localScale / 5));
        foreach (int index in tmpIndexes)
        {
            _impactList[index] = Vector3.zero;
        }
    }
}
