using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    OBJ_CUBE = 0,
    OBJ_SPHERE = 1,
    OBJ_TIMER = 2,
    OBJ_CLOCK = 3,
    OBJ_SKETCH = 4,
    OBJ_SUBSKETCH = 5,
    OBJ_NOTE = 6,
    OBJ_SCREEN = 7,
    OBJ_CALENDER = 8
}

public class VirtualObject : MonoBehaviour
{

    public int objectType;
    public VirtualObjectInfo info;
    public Vector3 initScale;
    public bool isCopied = false;
    public int lastID;

    public bool isGrabbing
    {
        get
        {
            if(GetComponent<Oculus.Interaction.HandGrab.HandGrabInteractable>().SelectingInteractors.Count > 0)
            {
                return true;
            }
            return false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        initScale = transform.localScale;

        objectType = name[..5] switch
        {
            // Basics
            "GCube" => (int)ObjectType.OBJ_CUBE,
            "GSphe" => (int)ObjectType.OBJ_SPHERE,

            "GSket" => (int)ObjectType.OBJ_SKETCH,
            "SubSk" => (int)ObjectType.OBJ_SUBSKETCH,

            "GScre" => (int)ObjectType.OBJ_SCREEN,
            "GCloc" => (int)ObjectType.OBJ_CLOCK,
            "GNote" => (int)ObjectType.OBJ_NOTE,
            "GTime" => (int)ObjectType.OBJ_TIMER,
            "GCale" => (int)ObjectType.OBJ_CALENDER,
            _ => (int)ObjectType.OBJ_CUBE
        };

        // ����� ���
        if (isCopied)
        {

            if(VOManager.keyList != null)
            {
                info.objectID = VOManager.curIndex;
                VOManager.keyList.keyList.Add($"{VOManager.curIndex}");
                VOManager.virtualObjects.Add($"{VOManager.curIndex++}", info);

                // �ڽ��� ��ȣ�� �÷��ֱ�
                if (info.objectType == (int)ObjectType.OBJ_SKETCH)
                {
                    int delta = info.objectID - lastID;
                    for (int i = 0; i < info.sketch_child.Count; i++)
                    {
                        info.sketch_child[i] = $"{int.Parse(info.sketch_child[i]) + delta}";
                    }
                }
            }
            else
            {

            }




        }
    }

    // Update is called once per frame
    void Update()
    {

        if (CoreInteraction.instance.allowsObjectSaving)    // only AR
        {

            setValue();



            if(info.objectID == 0)  // ��ȿ���� ���� ������Ʈ(�ӽ�)
            {
                return;
            }

            /*
            if (info.objectType == (int)ObjectType.OBJ_SUBSKETCH)
            {
                if (VOManager.virtualObjects.ContainsKey($"{info.parent}")) // �θ� �̹� ������
                {
                    Destroy(gameObject);
                }
            }
            */
        
            // ���� ������ ���� �ű� �������� �ʾҴٸ�
            if (VOManager.virtualObjects !=null && VOManager.virtualObjects[$"{info.objectID}"].position != transform.position)
            {
                info.objectID = VOManager.curIndex;
                VOManager.keyList.keyList.Add($"{VOManager.curIndex}");
                VOManager.virtualObjects.Add($"{VOManager.curIndex++}", info);
            }
        
        }


    }

    

    void setValue()
    {

        if (info != null)
        {
            info.objectType = objectType;

            info.position = transform.position;
            info.rotation = transform.rotation;
            info.scale = transform.localScale.magnitude / initScale.magnitude;
            info.initialScale = transform.localScale / info.scale;

            if(info.objectType != (int)ObjectType.OBJ_SKETCH && info.objectType != (int)ObjectType.OBJ_CALENDER)
                info.color = CoreInteraction.instance.colorToInt(GetComponent<MeshRenderer>().material);
       
            info.isFix = !GetComponent<Oculus.Interaction.HandGrab.HandGrabInteractable>().enabled; // false�̸� üũ, true�̸� üũ ����
            info.independent = true;

            // timer
            if (info.objectType == (int)ObjectType.OBJ_TIMER)
                info.timer_seconds = GetComponent<Timer>().seconds;

            // clock
            if (info.objectType == (int)ObjectType.OBJ_CLOCK)
                info.clock_region = (int)GetComponent<Clock>().region;

            // memo
            if (info.objectType == (int)ObjectType.OBJ_NOTE)
                info.note_string = GetComponent<Oculus.Voice.Voice_Handler>().textArea.text;

            // sketch
            if (info.objectType == (int)ObjectType.OBJ_SKETCH)
            {
                var b = GetComponent<BoxCollider>();
                info.maxV = b.center + b.size / 1.4f;
                info.minV = b.center - b.size / 1.4f;
            }

            // subsketch
            if (info.objectType == (int)ObjectType.OBJ_SUBSKETCH)
            {
                info.width = GetComponent<TubeRenderer>()._radiusOne * 1000;           // 1000:1 �� 0.01 : 10 (������ ����)
                info.independent = false;
            }

        }



    }

    private void OnDestroy()
    {
        if(VOManager.keyList != null)
        {
            VOManager.keyList.keyList.Remove($"{info.objectID}");       // ���⼭�� Ű�� ����
            //VOManager.virtualObjects.Remove($"{info.objectID}");
            // ���⼭�� Prefs �����ϸ� �ȵ�!
        }

    }
}
