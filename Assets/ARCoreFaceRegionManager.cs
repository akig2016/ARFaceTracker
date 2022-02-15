using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation.Samples
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    using UnityEngine.XR.ARCore;
    #endif

    [RequireComponent(typeof(ARFaceManager))]
    [RequireComponent(typeof(ARSessionOrigin))]
    public class ARCoreFaceRegionManager : MonoBehaviour
    {
        [SerializeField]
        GameObject _regionPrefab;
        [SerializeField]
        Button _changeGlassButton;
        [SerializeField]
        Button _changeFaceButton;
        int count = 0;
        [SerializeField]
        GameObject[] _glassesPrefab;
        [SerializeField]
        Texture[] _faceTextures;
        public GameObject RegionPrefab
        {
            get { return _regionPrefab; }
            set { _regionPrefab = value; }
        }

        ARFaceManager faceManager;

        ARSessionOrigin sessionOrigin;

        NativeArray<ARCoreFaceRegionData> faceRegions;

        Dictionary<TrackableId,  GameObject> InstantiatedPrefabs;
  
        void Start()
        {
            faceManager = GetComponent<ARFaceManager>();
            sessionOrigin = GetComponent<ARSessionOrigin>();  
            InstantiatedPrefabs = new Dictionary<TrackableId, GameObject>();
            _changeGlassButton.onClick.AddListener(ChangeGlasses);
            _changeFaceButton.onClick.AddListener(ChangeFace);
        }
        void ChangeGlasses()
        {
            foreach (var face in faceManager.trackables)
            {
                GameObject regionGos;
                if (!InstantiatedPrefabs.TryGetValue(face.trackableId, out regionGos))
                {                    
                    Instantiate(_glassesPrefab[count],regionGos.transform);
                }
                else
                {
                    if(regionGos.transform.GetChild(0)) Destroy(regionGos.transform.GetChild(0).gameObject);
                    Instantiate(_glassesPrefab[count], regionGos.transform);
                }
            }                    
            count++;
            if ( count>= _glassesPrefab.Length)
            {
                count = 0;
            }
        }
        int faceSpriteIndex = 0;
        void ChangeFace()
        {
            foreach (var face in faceManager.trackables)
            {                
                face.GetComponent<MeshRenderer>().material.mainTexture = _faceTextures[faceSpriteIndex];
                if (face.GetComponent<MeshRenderer>().material.mainTexture == null)
                {
                    face.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0);
                }
                else
                {
                    face.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 1);
                }                
            }
            faceSpriteIndex++;
            if (faceSpriteIndex >= _faceTextures.Length)
            {
                faceSpriteIndex = 0;
            }
        }

        void Update()
        {
            var subsystem = (ARCoreFaceSubsystem)faceManager.subsystem;
            if (subsystem == null)
                return;

            foreach (var face in faceManager.trackables)
            {
                GameObject regionGos;
                if (!InstantiatedPrefabs.TryGetValue(face.trackableId, out regionGos))
                {
                    regionGos = Instantiate(RegionPrefab, sessionOrigin.trackablesParent);
                    InstantiatedPrefabs.Add(face.trackableId, regionGos);
                    ChangeGlasses();
                }

                subsystem.GetRegionPoses(face.trackableId, Allocator.Persistent, ref faceRegions);
                regionGos.transform.localPosition = //m_FaceRegions[0].pose.position;
                    new Vector3(0.5f * (faceRegions[1].pose.position.x + faceRegions[2].pose.position.x),
                    0.5f * (faceRegions[1].pose.position.y+ faceRegions[0].pose.position.y),
                    faceRegions[1].pose.position.z);
                    //0.5f * (0.5f*(m_FaceRegions[1].pose.position+ m_FaceRegions[2].pose.position) + m_FaceRegions[0].pose.position);
                regionGos.transform.localRotation = faceRegions[0].pose.rotation;               
            }
        }

        void OnDestroy()
        {  
            if (faceRegions.IsCreated)
                faceRegions.Dispose();
            _changeGlassButton.onClick.RemoveListener(ChangeGlasses);
            _changeFaceButton.onClick.RemoveListener(ChangeFace);
        }
    }
}
