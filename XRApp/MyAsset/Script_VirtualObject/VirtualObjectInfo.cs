using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VirtualObjectInfo
{

    //public string objectName;   // ��ü �̸�(����?)
    public int objectID;        // ���� ��ȣ
    public int objectType;      // ��ü ����

    public Vector3 initialScale;    // ���� scale
    public float scale = 1;             // �ű⿡ �󸶸� ���ߴ°�
    public Vector3 position;
    public Quaternion rotation;

    public bool isFix = false;  // ��ü ����

    // type-specific

    // all except sketch
    public int color;   // 0~7 ������� �� �� �� �� �� �� �� ��

    // all except subsketch
    public bool independent = true;    // ���� ��ü�ΰ�(��: ����ġ ������ ���� ��ü�� �ƴ�)

    // timer
    public int timer_seconds;

    // clock
    public int clock_region;

    // sketch
    public Vector3 minV, maxV;
    public List<string> sketch_child = new List<string>();   // ���� �ڽ��� �ִ� ��� �� id�� ���

    // subsketch
    public float width;             // ����ġ �β�
    public int parent;              // �θ� ����ġ
    public List<Vector3> sketch_dots = new List<Vector3>();     // ���� ��ǥ

    // note
    public string note_string;      // �޸� �ؽ�Ʈ

    // screen

    // calender


    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }


    //public VirtualObjectInfo this[int i]
    public string this[int i]
    {
        get
        {
            return sketch_child[i];
            //return JsonUtility.FromJson<VirtualObject>(child[i]);
        }
        set
        {
            sketch_child[i] = value;
            //child[i] = value.ToString();
        }
    }



    /*
    public void setValue(VirtualObjectInfo other)
    {
        objectID = other.objectID;        // ���� ��ȣ
        objectType = other.objectType;      // ��ü ����

        initialScale = other.initialScale;    // ���� scale
        scale = other.scale;             // �ű⿡ �󸶸� ���ߴ°�
        position = other.position;
        rotation = other.rotation;

        color = other.color;   // 0~7 ������� �� �� �� �� �� �� �� ��
        isFix = other.isFix;  // ��ü ����

        // type-specific
        // all except subsketch
        independent = other.independent;    // ���� ��ü�ΰ�(��: ����ġ ������ ���� ��ü�� �ƴ�)

        // timer
        timer_seconds = other.timer_seconds;

        // clock
        clock_region = other.clock_region;

        // sketch
        sketch_child = other.sketch_child;   // ���� �ڽ��� �ִ� ��� �� id�� ���
        sketch_dots = other.sketch_dots;

        // note
        note_string = other.note_string;      // �޸� �ؽ�Ʈ

    }
    */

    
    
}
