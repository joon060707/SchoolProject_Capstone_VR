using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class FingerMenu : MonoBehaviour
{
    public static FingerMenu instance;

    public Transform index, middle, pinky, ring, thumb,
        detectPinch, detectMiddle, detectRing, detectPinky, indexRight;
    public OVRHand OVRHandPrefabLeft, OVRHandPrefabRight;

    public IList<OVRBone> boneList;

    public SkinnedMeshRenderer left, right, remoteLeft, remoteRight;
    public Material original, selected, transparent;

    public UnityEngine.UI.Image gazePointer;

    // [Header("aaa")]
    //public TouchDetection grip, pinch; 
    public Oculus.Interaction.HandGrab.HandGrabInteractor LInteractor, RInteractor;

    public GameObject
        // main
        mSketch, mRun,      // index
        mCreate, mCopy,     // middle
        mUnpin, mOption,      // ring
        mSetting, mRemove,  // pinky
                            // sub
        mDraw,              // index
        mSlider, mWidth,    // middle
        mPin, mCancel,      // ring
        mApply;             // pinky

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {

        // for debugging
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayerPrefs.SetString("SessionName", UserManagement.loginedUserID_s);
                PlayerPrefs.Save();
                ARtoVRLoading.Instance.startLoadScene("VRScene");
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (ARLobby.sessionList.Count > 0)
                {
                    PlayerPrefs.SetString("SessionName", ARLobby.sessionList[0].Name); // 0��
                    PlayerPrefs.Save();
                    ARtoVRLoading.Instance.startLoadScene("VRScene");
                }
                else
                {
                    Debug.Log("No Room");
                }
            }
        }





        // Hand Tracking
        if (boneList == null && OVRHandPrefabLeft.GetComponent<OVRSkeleton>().Bones.Count > 0)
            boneList = OVRHandPrefabLeft.GetComponent<OVRSkeleton>().Bones;
        
        if (boneList != null && boneList.Count > 0)
        {
            // main menu
            thumb.position = boneList[(int)OVRSkeleton.BoneId.Hand_ThumbTip].Transform.position;
            index.position = boneList[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
            middle.position = boneList[(int)OVRSkeleton.BoneId.Hand_MiddleTip].Transform.position;
            ring.position = boneList[(int)OVRSkeleton.BoneId.Hand_RingTip].Transform.position;
            pinky.position = boneList[(int)OVRSkeleton.BoneId.Hand_PinkyTip].Transform.position;

            thumb.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_ThumbTip].Transform.rotation;
            index.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.rotation;
            middle.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_MiddleTip].Transform.rotation;
            ring.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_RingTip].Transform.rotation;
            pinky.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_PinkyTip].Transform.rotation;

            // sub menu
            detectPinch.position = boneList[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
            detectMiddle.position = boneList[(int)OVRSkeleton.BoneId.Hand_MiddleTip].Transform.position;
            detectRing.position = boneList[(int)OVRSkeleton.BoneId.Hand_RingTip].Transform.position;
            detectPinky.position = boneList[(int)OVRSkeleton.BoneId.Hand_PinkyTip].Transform.position;

            detectPinch.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.rotation;
            detectMiddle.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_MiddleTip].Transform.rotation;
            detectRing.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_RingTip].Transform.rotation;
            detectPinky.rotation = boneList[(int)OVRSkeleton.BoneId.Hand_PinkyTip].Transform.rotation;
        }

        if(OVRHandPrefabLeft.GetComponent<OVRSkeleton>().Bones.Count > 0)
        {
            indexRight.position = OVRHandPrefabRight.GetComponent<OVRSkeleton>()
                .Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
        }

        // �޴� ������ ǥ��
        foreach (var g in new[] { mSketch, mRun, mCreate, mCopy, mUnpin, mOption, mSetting, mRemove, mDraw, mSlider, mWidth, mPin, mCancel, mApply })
            g.SetActive(false);

        // ���� �޴��� ǥ��
        if (CoreInteraction.isEditing)
        {
            mApply.SetActive(true);
            mPin.SetActive(true);
            mSlider.SetActive(true);
        }

        // ����ġ �޴��� ǥ��
        if (CoreInteraction.isSketching)
        {
            mWidth.SetActive(true);
            mDraw.SetActive(true);
            mCancel.SetActive(true);
            mApply.SetActive(true);
        }

        // �⺻ ȯ��
        if (GazeHand.instance.gazedObject != null)
        {
            // Interactable Object
            if (GazeHand.instance.gazedObject.CompareTag("Object") && selectedObject != null)     
            {
                mRun.SetActive(true);
                mCopy.SetActive(true);
                mOption.SetActive(true);
                mRemove.SetActive(true);

            }
            else if(!GazeHand.instance.gazedObject.CompareTag("Object") && selectedObject == null)    // Just Collider
            {
                mSketch.SetActive(true);
                mCreate.SetActive(true);
                if (GazeHand.instance.gazedObject.CompareTag("FixedObject"))
                    mUnpin.SetActive(true);
                mSetting.SetActive(true);
                
            }
            // else Nothing appeared

        }
        else    // No collision
        {
            mSketch.SetActive(true);
            mCreate.SetActive(true);
            //mUnpin.SetActive(true);
            mSetting.SetActive(true);
        }



        // ��ü�� �߰��� ��
        if (CoreInteraction.isSelecting || CoreInteraction.isSketching || CoreInteraction.isEditing)
        {
            index.gameObject.SetActive(false);
            middle.gameObject.SetActive(false);
            ring.gameObject.SetActive(false);
            pinky.gameObject.SetActive(false);

            return;
        }
       

        // ��ü ���� ���� �� ��
        if ((LInteractor.Candidate != null && LInteractor.Candidate.State == Oculus.Interaction.InteractableState.Hover) ||
            (RInteractor.Candidate != null && RInteractor.Candidate.State == Oculus.Interaction.InteractableState.Hover))
        {

            //Debug.Log(interactor.Candidate.name);

            index.gameObject.SetActive(false);
            middle.gameObject.SetActive(false);
            ring.gameObject.SetActive(false);
            pinky.gameObject.SetActive(false);

            //Debug.Log("selected");
            if(LInteractor.Candidate != null && LInteractor.Candidate.State == Oculus.Interaction.InteractableState.Hover)
            {
                //left.material = selected;
                remoteLeft.material = selected;
            }
            if(RInteractor.Candidate != null && RInteractor.Candidate.State == Oculus.Interaction.InteractableState.Hover)
            {
                //right.material = selected;
                remoteRight.material = selected;
            }


        }
        else
        {
            if (!tempHidden)
            {
                index.gameObject.SetActive(true);
                middle.gameObject.SetActive(true);
                ring.gameObject.SetActive(true);
                pinky.gameObject.SetActive(true);
            }

            if(LInteractor.Candidate == null)
            {
                //left.material = original;
                remoteLeft.material = original;
            }
            if (RInteractor.Candidate == null)
            {
                //right.material = original;
                remoteRight.material = original;
            }

        }

        // ���õ� ��� �׵θ� �׸���
        if(selectedObject != null)
        {
            if (isSelected && selectedObject.GetComponent<ParticleSystem>() != null)
            {
                selectedObject.GetComponent<ParticleSystem>().Play();
            }
            
        }

        // Gaze Pointer
        if (GazeHand.instance.gazedObject != null && (
            GazeHand.instance.gazedObject.CompareTag("Object") ||
            GazeHand.instance.gazedObject.CompareTag("3DButton")
            ))
            gazePointer.color = Color.yellow;
        else
            gazePointer.color = Color.white;

    }



    bool isSelected = false;
    GameObject selectedObject;

    bool tempHidden = false;

    public IEnumerator tempHide()
    {
        index.gameObject.SetActive(false);
        middle.gameObject.SetActive(false);
        ring.gameObject.SetActive(false);
        pinky.gameObject.SetActive(false);
        tempHidden = true;


        yield return new WaitForSeconds(0.5f);


        tempHidden = false;



    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HandTrigger"))
        {
            
            Debug.Log("Gazed Object: "+ (GazeHand.instance.gazedObject == null? "None" : GazeHand.instance.gazedObject.name));


            
            if (GazeHand.instance.gazedObject != null)      // ��ü�� ����Ų ����
            {
                if(GazeHand.instance.gazedObject.CompareTag("Object")) // Object�̸�
                { 
                    if (isSelected)         // �̹� �ѹ� ȸ������ ���� �ִ� ����
                    {
                        if (selectedObject.GetComponent<ParticleSystem>() != null && 
                            selectedObject == GazeHand.instance.gazedObject)    // �ٸ� �ɷ� �� �ٲ� �Ÿ� ��Ȯ�� ������ ����
                        {
                            switch (other.name)
                            {
                                case "Ring":
                                    Debug.Log("Edit");
                                    if (CoreInteraction.instance.allowsObjectSaving)
                                        CoreInteraction.instance.edit(selectedObject);
                                    else
                                        Debug.Log("NetworkEdit");
                                    break;
                                case "Middle":
                                    Debug.Log("Copy");
                                    if (CoreInteraction.instance.allowsObjectSaving)
                                        CoreInteraction.instance.copy(selectedObject);
                                    else
                                    {
                                        Debug.Log("NetworkCopy");
                                        Spawner.instance.beginCopyObj(selectedObject);
                                    }
                                        
                                    break;
                                case "Pinky":
                                    Debug.Log("Delete");
                                    if (CoreInteraction.instance.allowsObjectSaving)
                                        CoreInteraction.instance.delete(selectedObject);
                                    else
                                    {
                                        Debug.Log("NetworkDespawn");
                                        Spawner.instance.beginRemoveObj(selectedObject);
                                    }
                                        
                                    break;
                                default:        // index
                                    // �����ġ�� ������ ��쿡 �����Ͽ� ���
                                    if (selectedObject.name[..5] == "GTime" && !CoreInteraction.isEditing)
                                    {
                                        Timer timer = selectedObject.GetComponent<Timer>();
                                        if (timer.isRunning)
                                            timer.shouldStop = true;
                                        else
                                            StartCoroutine(Timer.runTimer(timer));
                                    }

                                    // �޸� ������ ��쿡 ����
                                    else if (selectedObject.name[..5] == "GNote" && !CoreInteraction.isEditing)
                                    {
                                        Oculus.Voice.Voice_Handler handler = selectedObject.GetComponent<Oculus.Voice.Voice_Handler>();
                                        handler.ToggleActivation();
                                    }

                                    else
                                        Debug.Log("Index finger may not be used");

                                    break;

                                    
                            }
                            isSelected = false;
                            selectedObject.GetComponent<ParticleSystem>().Stop();
                            selectedObject = null;
                        }
                        else        // �ϳ��� ������ ���¿��� ���ڱ� �ٸ� Object�� �����Ͽ� ������ ���̽�
                        {
                            selectedObject.GetComponent<ParticleSystem>().Stop();
                            selectedObject = GazeHand.instance.gazedObject;
                            return;
                        }

                    }
                    else       // ó�� ������ ������. (isSelected = false)
                    {
                        selectedObject = GazeHand.instance.gazedObject;
                        isSelected = true;
                    }


                }
                else if (GazeHand.instance.gazedObject.CompareTag("3DButton"))    // �̵� ��ư  
                {
                    if(GazeHand.instance.gazedObject.name.StartsWith("New Meeting"))
                    {
                        PlayerPrefs.SetString("SessionName", UserManagement.loginedUserID_s);
                        PlayerPrefs.Save();
                        ARtoVRLoading.Instance.startLoadScene("VRScene");
                        //UnityEngine.SceneManagement.SceneManager.LoadScene("VRScene");
                    }
                    else if(GazeHand.instance.gazedObject.name.StartsWith("MemUp"))
                    {
                        GameObject.Find("MembersContent").transform.position -= new Vector3(0, 0.13f, 0);
                    }
                    else if (GazeHand.instance.gazedObject.name.StartsWith("MemDown"))
                    {
                        GameObject.Find("MembersContent").transform.position += new Vector3(0, 0.13f, 0);
                    }
                    else if (GazeHand.instance.gazedObject.name.StartsWith("RoomUp"))
                    {
                        GameObject.Find("RoomsContent").transform.position -= new Vector3(0, 0.18f, 0);
                    }
                    else if (GazeHand.instance.gazedObject.name.StartsWith("RoomDown"))
                    {
                        GameObject.Find("RoomsContent").transform.position += new Vector3(0, 0.18f, 0);
                    }
                    else if (GazeHand.instance.gazedObject.name.StartsWith("Meeting"))    // �̹� ������ ��
                    {
                        PlayerPrefs.SetString("SessionName", GazeHand.instance.gazedObject.name[8..]); // Meeting_ ����
                        PlayerPrefs.Save();
                        ARtoVRLoading.Instance.startLoadScene("VRScene");
                    }
                    else
                    {
                        Debug.Log("Other menu");
                    }

                }
                else  // �� �Ǵ� ������ ������Ʈ
                {

                    if (isSelected)         // �ϳ��� ������ ���¿��� ����� �� ���
                    {
                        isSelected = false;
                        if (selectedObject != null && selectedObject.GetComponent<ParticleSystem>())
                            selectedObject.GetComponent<ParticleSystem>().Stop();   // �� ���� ������ �� ����
                        selectedObject = null;
                        return;
                    }
                    else                  // �������� ���� ���¿��� ����� �� ���
                    {

                        if (GazeHand.instance.gazedObject.CompareTag("FixedObject") && isLongRing) // ������ ������Ʈ�� ��� ���� ����
                        {
                            Debug.Log("Unfix " + GazeHand.instance.gazedObject.name);
                            GazeHand.instance.gazedObject.tag = "Object";
                            GazeHand.instance.gazedObject.GetComponent<Oculus.Interaction.HandGrab.HandGrabInteractable>().enabled = true;
                        }

                        if (GazeHand.instance.gazedObject.CompareTag("FixedObject") && isLongIndex) // ���� ������ ȣ��
                        {
                            if (GazeHand.instance.gazedObject.name[..5] == "GTime" && !CoreInteraction.isEditing)
                            {
                                Timer timer = GazeHand.instance.gazedObject.GetComponent<Timer>();
                                if (timer.isRunning)
                                    timer.shouldStop = true;
                                else
                                    StartCoroutine(Timer.runTimer(timer));

                                return; // ����ġ ���� ����
                            }

                            // �޸� ������ ��쿡 ����
                            else if (GazeHand.instance.gazedObject.name[..5] == "GNote" && !CoreInteraction.isEditing)
                            {
                                Oculus.Voice.Voice_Handler handler = GazeHand.instance.gazedObject.GetComponent<Oculus.Voice.Voice_Handler>();
                                handler.ToggleActivation();

                                return; // ����ġ ���� ����
                            }

                            else
                                Debug.Log("Index finger may not be used");

                            
                        }


                        switch (other.name)
                        {
                            case "Middle":
                                Debug.Log("Create an Object");      // 3D ��ü, �޸�, 
                                if (CoreInteraction.instance.allowsObjectSaving)
                                    CoreInteraction.instance.create();
                                else
                                    StartCoroutine(Spawner.instance.select());
                                break;

                            case "Ring":                            // ������ ��ü�� ���� ����
                                Debug.Log("Not yet used");
                                break;
                            case "Pinky":
                                Debug.Log("Settings");

                                // �ӽ�
                                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "VRScene")
                                {
                                    if(isLongPinky)  finish();
                                }

                                CoreInteraction.instance.settings();
                                break;
                            default:        // index

                                if (CoreInteraction.instance.allowsObjectSaving)
                                {
                                    if (isLongIndex)
                                    {
                                        Debug.Log("Create Sketch");         // 3D ����ġ
                                        CoreInteraction.instance.sketch();
                                    }
                                    else
                                        Debug.Log("Sketch not started due to short touch");
                                }
                                else
                                {
                                    Debug.Log("Sketch for networking");
                                    Spawner.instance.spawnSketch();
                                  
                                }
                                break;
                        }
                    }



                }

            }
            else    // ���� ���
            {
                if (isSelected)
                {
                    isSelected = false;
                    if (selectedObject != null) selectedObject.GetComponent<ParticleSystem>().Stop();
                    selectedObject = null;
                }
                else                 // �������� ���� ���¿��� ����� �� ���
                {
                    switch (other.name)
                    {
                        case "Middle":
                            Debug.Log("Create an Object");      // 3D ��ü, �޸�, 
                            CoreInteraction.instance.create();
                            break;
                        case "Ring":
                            Debug.Log("Not yet used");
                            break;
                        case "Pinky":
                            Debug.Log("Settings");


                            // �ӽ�
                            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "VRScene")
                            {
                                if (isLongPinky) finish();
                            }
                                

                            CoreInteraction.instance.settings();
                            break;
                        default:


                            if (CoreInteraction.instance.allowsObjectSaving)
                            {
                                if (isLongIndex)
                                {
                                    Debug.Log("Create Sketch");         // 3D ����ġ
                                    CoreInteraction.instance.sketch();
                                }
                                else
                                    Debug.Log("Sketch not started due to short touch");
                            }
                            else
                            {
                                Debug.Log("Sketch for networking");
                                Spawner.instance.spawnSketch();
                                
                            }


                            break;
                    }
                }



            }


            StartCoroutine(tempHide());

        }  
    }


    async void finish()
    {
        //if (Spawner.instance._runner.IsServer)
            await Spawner.instance._runner.Shutdown();

       // ARtoVRLoading.Instance.startLoadScene("SampleScene");

    }


    public static bool isPinching = false, isMiddle = false, isRing = false, isPinky = false;


    public static double longRing, longIndex, longPinky = 0; // ���� ������ ���� ��Ŭ�� ���
    public static bool isLongRing { get { return longRing > 1; } }
    public static bool isLongIndex { get { return longIndex > 1; } }
    public static bool isLongPinky { get { return longPinky > 1; } }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "PinchDetector")
        {
            isPinching = true;
            longIndex += Time.deltaTime;
        }
        if (other.gameObject.name == "MiddleDetector")
        {
            isMiddle = true;
        }
        if (other.gameObject.name == "RingDetector")
        {
            isRing = true;
            longRing += Time.deltaTime;
            
        }
        if (other.gameObject.name == "PinkyDetector")
        {
            isPinky = true;
            longPinky += Time.deltaTime;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PinchDetector")
        {
            isPinching = false;
            longIndex = 0;
        }
        if (other.gameObject.name == "MiddleDetector")
        {
            isMiddle = false;
        }
        if (other.gameObject.name == "RingDetector")
        {
            isRing = false;
            longRing = 0;
        }
        if (other.gameObject.name == "PinkyDetector")
        {
            isPinky = false;
            longPinky = 0;
        }
    }
}
