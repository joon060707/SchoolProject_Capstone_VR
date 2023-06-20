using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.HandGrab;



public class CoreInteraction : MonoBehaviour
{
    public GameObject GazeHandVisual;
    public static CoreInteraction instance;

    public GameObject cube, sphere, sketchGroup, sketchPiece, screen, note, clock, timer;
    public GameObject prefabSet, grabberL, grabberR;
    public HandGrabInteractor leftInteractor, rightInteractor;
    public GameObject optionCanvas;

    // 0~7 ������� �� �� �� �� �� �� �� ��
    public Material[] color;
    public Color[] colorTint;

    public float initGazeHandOffset = 0.43f, editGazeHandOffset = 100f;

    [Header("AR ��忡���� üũ�ϼ���")]
    public bool allowsObjectSaving = true;

    public OVRSpatialAnchor SpatialAnchor;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;

    }

    // Update is called once per frame
    void Update()
    {

        grabberL.transform.position = leftInteractor.transform.position;
        grabberR.transform.position = rightInteractor.transform.position;

        GazeHandVisual.SetActive(!isEditing);   // ���� �߿��� �ڵ� �����

        SpatialAnchor = VOManager.SpatialAnchor;
    }

    #region create
    public void create()
    {
        Debug.Log("Create");


        // VR ��忡���� ���� �׸� ���
        if (!allowsObjectSaving)
            return;



        // ���� ���� �ĺ� ������Ʈ�� ���տ� 1�� �����Ǹ�
        // �ϳ��� ������ �������� �ڵ� �ı�. (���� �Ϸ�)
        // ũ�⳪ ������ ���� ���Ŀ� ���� ����
        prefabSet.SetActive(true);
        StartCoroutine(select());
    }

    public static bool isSelecting = false;

    IEnumerator select()
    {
        GameObject obj;

        isSelecting = true;
        grabberL.SetActive(true); // �ָ� ������� �Ǵ�
        grabberR.SetActive(true);

        // �� �� �� ���� ���� ������ ��ٸ� // wait until condition is false
        //yield return new WaitUntil(() => leftInteractor.HasSelectedInteractable || rightInteractor.HasSelectedInteractable);

        // Ư�� ��ü�� ������ ���·� �ָ��� �� ��
        yield return new WaitUntil(() => leftInteractor.IsGrabbing || rightInteractor.IsGrabbing);


        string name;
        HandGrabInteractor interactor;

        if (leftInteractor.IsGrabbing)
            interactor = leftInteractor;
        else
            interactor = rightInteractor;

        interactor.ForceRelease();
        grabberL.SetActive(false);
        grabberR.SetActive(false);
        prefabSet.SetActive(false);

        isSelecting = false;

        // �ٸ� ��
        if (!GazeHand.instance.gazedObject.CompareTag("Prefab"))
        {
            yield break;
        }


        //name = interactor.SelectedInteractable.gameObject.name;
        name = GazeHand.instance.gazedObject.name;
        Debug.Log("selected: "+name);

        switch(name){
            case "Screen":
                obj = Instantiate(screen, interactor.transform.position, Quaternion.identity,SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "Cube":
                obj = Instantiate(cube, interactor.transform.position, Quaternion.identity, SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "Note":
                obj = Instantiate(note, interactor.transform.position, Quaternion.identity, SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "Sphere":
                obj = Instantiate(sphere, interactor.transform.position, Quaternion.identity, SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "Timer":
                obj = Instantiate(timer, interactor.transform.position, Quaternion.identity, SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;
            case "Clock":
                obj = Instantiate(clock, interactor.transform.position, Quaternion.identity, SpatialAnchor.transform);
                interactor.ForceSelect(obj.GetComponent<HandGrabInteractable>());
                break;

            // ���� ���� ���� ���
            default:
                // â�� ������� �״�� ������
                obj = null;
                break;

        }

        // object �߰�
        if(obj != null && allowsObjectSaving)
        {
            obj.GetComponent<VirtualObject>().info.objectID = VOManager.curIndex;
            VOManager.keyList.keyList.Add($"{VOManager.curIndex}");
            VOManager.virtualObjects.Add($"{VOManager.curIndex++}", obj.GetComponent<VirtualObject>().info);
        }
            



        StartCoroutine(FingerMenu.instance.tempHide()); // �����ֱ�


    }

    #endregion

    #region sketch

    public void sketch()
    {
        Debug.Log("Sketch");
        // ���Լհ������� ����ġ ����
        // ��� ��� ��� ���� (���� ���� ��ǥ)
        // �����հ������� �β�
        // �����հ������� ����
        // �����հ������� ����

        StartCoroutine(whileSketching());
    }

    public static bool isSketching = false; // overall
    public static bool isDrawing = false; // piece

    GameObject sketchObj;
    Vector3 maxV, minV;
    List<string> subSketchID;
    int tempParentID;

    // �׸��� ��Ʈ ����
    IEnumerator whileSketching()
    {
        isSketching = true;
        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true;
        //FingerMenu.instance.detectPinch.GetComponent<MeshRenderer>().enabled = true;
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true;
        //FingerMenu.instance.detectRing.GetComponent<MeshRenderer>().enabled = true;
        sketchCanvas.SetActive(true);

        subSketchID = new List<string>();


        // ������? ��ġ ��ġ
        sketchObj = Instantiate(sketchGroup, FingerMenu.instance.index.position, Quaternion.identity,SpatialAnchor.transform);

        // Object �߰�

        if (allowsObjectSaving)
        {
            sketchObj.GetComponent<VirtualObject>().info.objectID = VOManager.curIndex;
            VOManager.keyList.keyList.Add($"{VOManager.curIndex}");
            tempParentID = VOManager.curIndex;
            VOManager.virtualObjects.Add($"{VOManager.curIndex++}", sketchObj.GetComponent<VirtualObject>().info);
        }



        maxV = new Vector3(-9999999, -9999999, -9999999);
        minV = new Vector3(9999999, 9999999, 9999999);

        Debug.Log("sketch is about to start");

        while (true)
        {
            if (FingerMenu.isPinching && !isDrawing)    // draw
            {
                Debug.Log("pinching");
                StartCoroutine(whileDrawing());
            }

            yield return null;

            if (!isDrawing && FingerMenu.isMiddle) // width
            {
                // sketch �β� ����
                StartCoroutine(slideLineWidth());
            }


            if (!isDrawing && FingerMenu.isPinky) // exit and confirm
            {
                break;
            }


            if (!isDrawing && FingerMenu.isRing) // exit without saving
            {
                Debug.Log("User wants to exit it, so destroy it");
                // ���� ����?�� ���� �������� ����
                for (int i = sketchObj.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(sketchObj.transform.GetChild(i).gameObject);
                }
                Destroy(sketchObj);

                sketchCanvas.SetActive(false);
                isSketching = false;
                //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = false;
                //FingerMenu.instance.detectPinch.GetComponent<MeshRenderer>().enabled = false;
                //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = false;
                //FingerMenu.instance.detectRing.GetComponent<MeshRenderer>().enabled = false;
                StartCoroutine(FingerMenu.instance.tempHide()); // �����ֱ�
                yield break;
            }
        }

        Debug.Log("sketch is completed");
        // subsketch�� ������ �ı��ϱ�
        if(sketchObj.GetComponentInChildren<LineRenderer>() == null)
        {
            Debug.Log("There is no subsketch, so destroy it");
            Destroy(sketchObj);

        }
        else
        {
            //�ڽ� ũ�� ���ϱ�
            Debug.Log(minV);
            Debug.Log(maxV);
            Vector3 center = (minV + maxV) / 2;
            sketchObj.GetComponent<BoxCollider>().center = center;
            // ������ 2���ε�, �ڽ����� ����Ͽ�(ȸ���� �𼭸� �κ��� ŭ) 1.4��� ����
            sketchObj.GetComponent<BoxCollider>().size = (center - minV) * 1.4f; // center - min

            sketchObj.transform.Find("Border").position = center + sketchObj.transform.position;
            sketchObj.transform.Find("Border").localScale = (center - minV) * 1.4f;

            Debug.Log(center);
            Debug.Log(center - minV);

            // �ణ ������ �ű��
            sketchObj.transform.position = sketchObj.transform.position + new Vector3(0, 0, 0.8f);


            // �ڽ� �߰�
            sketchObj.GetComponent<VirtualObject>().info.sketch_child = subSketchID;

        }
            isSketching = false;
            sketchCanvas.SetActive(false);
            //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = false;
            //FingerMenu.instance.detectPinch.GetComponent<MeshRenderer>().enabled = false;
            //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = false;
            //FingerMenu.instance.detectRing.GetComponent<MeshRenderer>().enabled = false;
            StartCoroutine(FingerMenu.instance.tempHide()); // �����ֱ�


        // finish
    }

    // �β� ����
    public GameObject sketchCanvas;
    public float currentSketchWidth = 10;       // 1000:1 �� 0.01 : 10 (������ ����)
    public TMPro.TMP_Text sketchWidthText;
    public LineRenderer sketchWidthVisual;

    IEnumerator slideLineWidth()
    {

        Vector3 pos = FingerMenu.instance.detectMiddle.position;

        while (FingerMenu.isMiddle)
        {
            float value = (FingerMenu.instance.detectMiddle.position.x - pos.x) * Time.deltaTime * 2; // 50cm = 1 

            currentSketchWidth += value;
            //Debug.Log(currentSketchWidth);

            if (currentSketchWidth < 1) currentSketchWidth = 1;
            if (currentSketchWidth > 100) currentSketchWidth = 100;


            //obj.GetComponent<MeshRenderer>().material = color[(int)Mathf.Round(clockSlider.value)];
            sketchWidthText.text = $"{(int)currentSketchWidth}";

            sketchWidthVisual.startWidth = currentSketchWidth / 500;
            sketchWidthVisual.endWidth = currentSketchWidth / 500;

            yield return null;
        }

        currentSketchWidth = (int)Mathf.Round(currentSketchWidth);
        sketchWidthText.text = $"{(int)currentSketchWidth}";
    }




    // pinch���� �� ���� �׸���
    List<Vector3> currentLine;
    IEnumerator whileDrawing()
    {
        isDrawing = true;
        currentLine = new List<Vector3>();

        Debug.Log("start drawing subsketch");

        GameObject subSketch = Instantiate(sketchPiece, sketchObj.transform.position, Quaternion.identity, sketchObj.transform);

        LineRenderer lineRenderer = subSketch.GetComponent<LineRenderer>();
        TubeRenderer tubeRenderer = subSketch.GetComponent<TubeRenderer>();
        tubeRenderer.SetRadius(currentSketchWidth / 1000);       // 1000:1 �� 0.01 : 10 (������ ����)

        int i = 0;
        while (true)
        {
            if (i++ % 3 != 0)   // 3�����Ӵ� 1��
            {
                yield return null;
                continue;
            }

            // local space�̹Ƿ� �̷��� ó��
            Vector3 p = FingerMenu.instance.index.position - sketchObj.transform.position;
            lineRenderer.SetPosition(lineRenderer.positionCount++, p);

            currentLine.Add(p);
            tubeRenderer.SetPositions(currentLine.ToArray());
            subSketch.GetComponent<MeshFilter>().mesh = tubeRenderer.GetMesh();
            subSketch.GetComponent<MeshCollider>().sharedMesh = tubeRenderer.GetMesh();

            // Box ���� ���ϱ�
            if (p.x < minV.x) minV.x = p.x;
            if (p.y < minV.y) minV.y = p.y;
            if (p.z < minV.z) minV.z = p.z;
            if (p.x > maxV.x) maxV.x = p.x;
            if (p.y > maxV.y) maxV.y = p.y;
            if (p.z > maxV.z) maxV.z = p.z;

            yield return null;

            if (!FingerMenu.isPinching)
            {
                break;
            }

        }

        Debug.Log("finish drawing subsketch");
        isDrawing = false;

        // object �߰�

        if (allowsObjectSaving)
        {
            subSketchID.Add($"{VOManager.curIndex}");
            subSketch.GetComponent<VirtualObject>().info.objectID = VOManager.curIndex;
            subSketch.GetComponent<VirtualObject>().info.width = currentSketchWidth;
            subSketch.GetComponent<VirtualObject>().info.sketch_dots = currentLine;
            subSketch.GetComponent<VirtualObject>().info.parent = tempParentID;
            subSketch.GetComponent<VirtualObject>().info.independent = false;
            VOManager.keyList.keyList.Add($"{VOManager.curIndex}");
            VOManager.virtualObjects.Add($"{VOManager.curIndex++}", subSketch.GetComponent<VirtualObject>().info);
        }





    }

    #endregion

    public void copy(GameObject obj)
    {
        if (obj.GetComponent<HandGrabInteractable>())
        {
            Debug.Log("Copy" + obj);

            obj.GetComponent<VirtualObject>().isCopied = true;  // �ӽ� �÷���
            obj.GetComponent<VirtualObject>().lastID = obj.GetComponent<VirtualObject>().info.objectID;

            Instantiate(obj, obj.transform.position + new Vector3(0.1f, 0.1f, 0.1f), obj.transform.rotation,SpatialAnchor.transform);

            obj.GetComponent<VirtualObject>().isCopied = false;
        }
        else
        {
            Debug.LogWarning("Cannot copy it because it does not have HandGrabInteractable");
        }
    }

    public void delete(GameObject obj)
    {
        if (obj.GetComponent<HandGrabInteractable>())
        {
            Debug.Log("Delete" + obj);

            // subsketch ����
            if (obj.GetComponentInChildren<LineRenderer>() != null)
            {
                for (int i = obj.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(obj.transform.GetChild(i).gameObject);
                }
            }

            Destroy(obj);
        }
        else
        {
            Debug.LogWarning("Cannot delete it because it does not have HandGrabInteractable");
        }
    }

    #region edit

    // for menu
    GameObject m1, m2, m3, m4;
    public void edit(GameObject obj)
    {
        optionCanvas.SetActive(true);
        

        if (m1 == null)
        {
            Transform panel = optionCanvas.transform.Find("MenuPanel");
            m1 = panel.Find("Menu1").gameObject;
            m2 = panel.Find("Menu2").gameObject;
            m3 = panel.Find("Menu3").gameObject;
            m4 = panel.Find("Menu4").gameObject;
        }

        if (obj.GetComponent<HandGrabInteractable>())
        {
            Debug.Log("Edit" + obj);
            
            // switch
            switch(obj.name.Substring(0, 5))
            {
                // Cube, Sphere, Clock, Timer �ϼ�    || Note, Screen �̿ϼ�

                // Basics
                case "GCube":
                case "GSphe":
                    StartCoroutine(editBasics(obj));             // ����
                    break;

                // Sketch
                case "GSket":                                                   // (��ġ ����, ����)

                    Debug.Log("������ �߰� �ɼ��� ������ �ڵ� ���� ����");
                    obj.GetComponent<HandGrabInteractable>().enabled = false;   // ������
                    obj.tag = "FixedObject";
                    Debug.Log("Fix " + obj.name);

                    optionCanvas.SetActive(false);
                    break;

                // Widgets
                case "GScre":

                    Debug.Log("������ �߰� �ɼ��� ������ �ڵ� ���� ����");
                    obj.GetComponent<HandGrabInteractable>().enabled = false;   // ������
                    obj.tag = "FixedObject";
                    Debug.Log("Fix " + obj.name);

                    optionCanvas.SetActive(false);
                    break;

                case "GCloc":                                   // �ð�
                    StartCoroutine(editClock(obj));             // ���� ����, ����
                    break;
                case "GNote":
                    StartCoroutine(editBasics(obj));            // ���� �ؽ�Ʈ, Ű���� �ؽ�Ʈ, ����
                    break;
                case "GTime":                                   // Ÿ�̸�(����, ��) -> �����հ���
                    StartCoroutine(editTimer(obj));            // �ð� ����, ���� 
                    break;

                default:
                    break;

            }

        }
        else
        {
            Debug.LogWarning("Cannot edit it because it does not have HandGrabInteractable");
        }
    }


    public static bool
        isEditing = false,
        isChangingColor = false,    // for all objects including cube and sphere except screen
        isChangingTextwithKey = false,     // for note
        isChangingTextwithVoice = false,     // for note
        isChangingTime = false,     // for timer
        isChangingRegion = false,   // for clock
        isChangingSketch = false;   // for sketch


    #region editForType



    // �� ������ ������
    // ���� ����� ������
    // �ɼ� ������ �չٴڸ鿡 UI �߰��ϱ�
    IEnumerator editBasics(GameObject obj)
    {
        GazeHand.instance.dist = editGazeHandOffset;
        isEditing = true;

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true;
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true;

        m1.SetActive(true);
        m2.SetActive(false);
        m3.SetActive(false);
        m4.SetActive(false);

        StartCoroutine(editColor(obj)); // �ʱ� �ɼ�

        // �޴� �߰��ϱ�

        while (true)
        {

            if (FingerMenu.isLongRing) // ��ü ����
            {
                obj.GetComponent<HandGrabInteractable>().enabled = false;   // ������
                obj.tag = "FixedObject";
                Debug.Log("Fix " + obj.name);
                break;  // ��ü ������ �ڵ����� �޴����� ������
            }

            if (FingerMenu.isPinky) // ����
                break;

            yield return null;
        }

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = false;
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = false;

        isEditing = false;
        GazeHand.instance.dist = initGazeHandOffset;
        StartCoroutine(FingerMenu.instance.tempHide()); // �����ֱ�
        optionCanvas.SetActive(false);
    }


    IEnumerator editNote(GameObject obj){ yield return null; }
    IEnumerator editClock(GameObject obj) {

        GazeHand.instance.dist = editGazeHandOffset;
        isEditing = true;

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true; // Exit
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true; // Change value

        m1.SetActive(true);
        m1.transform.Find("Text").GetComponent<TMPro.TMP_Text>().text = "�ð���\n����";
        m2.SetActive(false);
        m3.SetActive(false);
        m4.SetActive(false);

        //StartCoroutine(editColor(obj)); // �ʱ� �ɼ�
        StartCoroutine(editReg(obj));

        // �޴� �߰��ϱ�

        while (true)
        {


            if (FingerMenu.isLongRing) // ��ü ����
            {
                obj.GetComponent<HandGrabInteractable>().enabled = false;   // ������
                obj.tag = "FixedObject";
                Debug.Log("Fix " + obj.name);
                break;  // ��ü ������ �ڵ����� �޴����� ������
            }


            if (FingerMenu.isPinky)     // � �޴��� ���� �ֵ������� ����
                break;

            yield return null;
        }

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true; // Exit
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true; // Change value

        isEditing = false;
        GazeHand.instance.dist = initGazeHandOffset;
        StartCoroutine(FingerMenu.instance.tempHide()); // �����ֱ�
        optionCanvas.SetActive(false);

    }
    IEnumerator editTimer(GameObject obj) {


        GazeHand.instance.dist = editGazeHandOffset;
        isEditing = true;

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true; // Exit
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true; // Change value

        m1.SetActive(true);
        m1.transform.Find("Text").GetComponent<TMPro.TMP_Text>().text = "�ð�\n����";
        m2.SetActive(false);
        m3.SetActive(false);
        m4.SetActive(false);

        StartCoroutine(editHowLong(obj)); // �ʱ� �ɼ�

        // �޴� �߰��ϱ�

        while (true)
        {

            if (FingerMenu.isLongRing) // ��ü ����
            {
                obj.GetComponent<HandGrabInteractable>().enabled = false;   // ������
                obj.tag = "FixedObject";
                Debug.Log("Fix " + obj.name);
                break;  // ��ü ������ �ڵ����� �޴����� ������
            }

            if (FingerMenu.isPinky)     // � �޴��� ���� �ֵ������� ����
                break;

            yield return null;
        }

        //FingerMenu.instance.detectPinky.GetComponent<MeshRenderer>().enabled = true; // Exit
        //FingerMenu.instance.detectMiddle.GetComponent<MeshRenderer>().enabled = true; // Change value

        isEditing = false;
        GazeHand.instance.dist = initGazeHandOffset;
        StartCoroutine(FingerMenu.instance.tempHide()); // �����ֱ�
        optionCanvas.SetActive(false);


    }
    IEnumerator editSketch(GameObject obj) { yield return null; }

    #endregion

    #region editModule

    public int colorToInt(Material material)
    {
        for(int i=0; i<color.Length; i++)
        {
            if (color[i].color == material.color)
                return i;
        }
        return 0;   // not found -> default
    }

    public Material intToColor(int c)
    {
        if (c < color.Length) return color[c];
        else return color[0];
    }

    public GameObject colorCanvas;
    public UnityEngine.UI.Slider colorSlider;


    // ���� �����̴�
    // ���� ����
    IEnumerator editColor(GameObject obj)
    {
        // show color UI
        isChangingColor = true;
        colorCanvas.SetActive(true);

        // colorSlider.value�� VO�κ��� �̸� ����
        colorSlider.value = colorToInt(obj.GetComponent<MeshRenderer>().material);
        Debug.Log("Begin: " + colorSlider.value);

        
        while (true)
        {
            if (isChangingColor && FingerMenu.isMiddle) // ������ �� ����
                StartCoroutine(slideColor(obj));

            if (isChangingColor && FingerMenu.isPinky)  // ������ ���� ����
            {
                break;
            }

            if (FingerMenu.isLongRing) // ��ü ���� �޴� ����� ���� ����
            {
                break;  
            }

            /*
            // ������Ʈ�� �ٸ���
            if (obj.name.Substring(0, 5)=="GCloc")
            {
                if (m2.GetComponent<TriggerDetector>().touched)
                {
                    Debug.Log("M2");
                    StartCoroutine(editReg(obj));
                    break;
                    
                }
            }

            // ������Ʈ�� �ٸ���
            if (obj.name.Substring(0, 5) == "GTime")
            {
                if (m2.GetComponent<TriggerDetector>().touched)
                {
                    StartCoroutine(editHowLong(obj));
                    break;

                }
            }
            */
            yield return null;
        }

        isChangingColor = false;
        colorCanvas.SetActive(false);
    }

    IEnumerator slideColor(GameObject obj) {

        Vector3 pos = FingerMenu.instance.detectMiddle.position;

        while (FingerMenu.isMiddle)
        {
            float value = (FingerMenu.instance.detectMiddle.position.x - pos.x) * Time.deltaTime; // 50cm = 1 
          
            colorSlider.value += value;
            Debug.Log(colorSlider.value);

            //value = Mathf.Min(7, Mathf.Max(value, 0));

            obj.GetComponent<MeshRenderer>().material = color[(int)Mathf.Round(colorSlider.value)];


            if (obj.transform.Find("memo") != null)
            {
                obj.transform.Find("memo").GetComponent<SpriteRenderer>().color = colorTint[(int)Mathf.Round(colorSlider.value)];
            }



            yield return null;
        }

        Debug.Log("Final: "+colorSlider.value);
        colorSlider.value = (int)Mathf.Round(colorSlider.value);
        obj.GetComponent<VirtualObject>().info.color = (int)Mathf.Round(colorSlider.value);
    }

    //------------------------------------------------

    // from editClock
    public GameObject clockCanvas;
    public UnityEngine.UI.Slider clockSlider;
    public TMPro.TMP_Text timeShow;
    IEnumerator editReg(GameObject obj)
    {

        isChangingRegion = true;
        clockCanvas.SetActive(true);

        colorSlider.value = (int)obj.GetComponent<Clock>().region;  // should be changed
        timeShow.text = $"���� �ð�: {Clock.timeStr(0)}\n�ش� ����: {Clock.timeStr((Region)(int)Mathf.Round(clockSlider.value))}";

        while (true)
        {
            if (isChangingRegion && FingerMenu.isMiddle) // ������ �� ����
                StartCoroutine(slideReg(obj));

            if (isChangingRegion && FingerMenu.isPinky)  // ������ ���� ����
            {
                break;
            }

            if (FingerMenu.isLongRing) // ��ü ���� �޴� ����� ���� ����
            {
                break;
            }

            /*
            // ������Ʈ�� �ٸ���
            if (obj.name.Substring(0, 5) == "GCloc")
            {
                if (m1.GetComponent<TriggerDetector>().touched)
                {
                    StartCoroutine(editColor(obj));
                    break;

                }
            }
            */

            yield return null;

        }

        clockCanvas.SetActive(false);
        isChangingRegion = false;

    }


    IEnumerator slideReg(GameObject obj)
    {

        Vector3 pos = FingerMenu.instance.detectMiddle.position;

        while (FingerMenu.isMiddle)
        {
            float value = (FingerMenu.instance.detectMiddle.position.x - pos.x) * Time.deltaTime; // 50cm = 1 

            clockSlider.value += value;
            Debug.Log(clockSlider.value);

            //obj.GetComponent<MeshRenderer>().material = color[(int)Mathf.Round(clockSlider.value)];
            timeShow.text = $"���� �ð�: {Clock.timeStr(0)}\n�ش� ����: {Clock.timeStr((Region)(int)Mathf.Round(clockSlider.value))}";
            obj.GetComponent<Clock>().region = (Region)(int)Mathf.Round(clockSlider.value);
            yield return null;
        }

        Debug.Log("Final: " + clockSlider.value);
        clockSlider.value = (int)Mathf.Round(clockSlider.value);
    }

    //------------------------------------------------

    // for Timer
    public GameObject timerCanvas;
    public TMPro.TMP_Text timerText;
    float tempTimerValue;

    // ���� �ʿ�
    IEnumerator editHowLong(GameObject obj)
    {

        isChangingTime = true;
        timerCanvas.SetActive(true);

        //colorSlider.value
        timerText.text = $"{Timer.formatting(obj.GetComponent<Timer>().seconds)}";
        tempTimerValue = obj.GetComponent<Timer>().seconds;


        while (true)
        {
            if (isChangingTime && FingerMenu.isMiddle) // ������ �� ����
                StartCoroutine(slideHowLong(obj));

            if (isChangingTime && FingerMenu.isPinky)  // ������ ���� ����
            {
                break;
            }

            if (FingerMenu.isLongRing) // ��ü ���� �޴� ����� ���� ����
            {
                break;
            }

            /*
            if (isChangingTime && FingerMenu.isPinching)  // Ÿ�̸� �������� �ð� ���� �� �����հ��� ���ý� ���� ���� �� Ÿ�̸� ����
            {
                StartCoroutine(Timer.runTimer(obj.GetComponent<Timer>()));
                break;
            }
            */

            /*
            // ������Ʈ�� �ٸ���
            if (obj.name.Substring(0, 5) == "GTime")
            {
                if (m1.GetComponent<TriggerDetector>().touched)
                {
                    StartCoroutine(editColor(obj));
                    break;

                }
            }
            */
            yield return null;

        }

        timerCanvas.SetActive(false);
        isChangingTime = false;

    }

    
    IEnumerator slideHowLong(GameObject obj)
    {

        Vector3 pos = FingerMenu.instance.detectMiddle.position;

        while (FingerMenu.isMiddle)
        {
            float value = (FingerMenu.instance.detectMiddle.position.x - pos.x) * Time.deltaTime * 2; // 50cm = 1 

            tempTimerValue += value;
            Debug.Log(tempTimerValue);

            //obj.GetComponent<MeshRenderer>().material = color[(int)Mathf.Round(clockSlider.value)];
            timerText.text = $"{Timer.formatting((int)tempTimerValue)}";
            obj.GetComponent<Timer>().seconds = (int)tempTimerValue;

            yield return null;
        }

        Debug.Log("Final: " + clockSlider.value);
        tempTimerValue = (int)Mathf.Round(tempTimerValue) / 10 * 10;    // 10 ������ ����
        obj.GetComponent<Timer>().seconds = (int)tempTimerValue;
        timerText.text = $"{Timer.formatting((int)tempTimerValue)}";
    }

    #endregion

    #endregion


    public void settings()
    {
        Debug.Log("Settings");
        // ���� �˾� ����
    }


}
