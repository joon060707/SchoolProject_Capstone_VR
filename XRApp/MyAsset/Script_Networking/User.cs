using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Oculus.Interaction.HandGrab;


public struct UserTransform : INetworkInput
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 gazeDirection;

}

public class User : NetworkBehaviour
{
    public TMPro.TMP_Text txt;
    [Networked] public string userName { get; set; }


    // ���� ��ġ�κ��� �󸶳� �������°�
    [Networked] public Vector3 gazeOrigin { get; set; }
    [Networked] public Vector3 gazeDirection { get; set; }

    // ���� gaze pointer ��ġ
    [Networked] public Vector3 gazePointerPos { get; set; }

    [Networked] public Quaternion rotation { get; set; }

    [Networked] public float rayLength { get; set; }
    [Networked] public int playerId { get; set; }


    LineRenderer lineRenderer;
    Transform gazePointer;

    // ���� ��ġ
    [Networked] public Vector3 spawnPos { get; set; }
    [Networked] public Quaternion spawnRot { get; set; }

    public (Vector3, Quaternion) getSpawnPosRot(int id)
    {
        Vector3[] pos = {new Vector3(0.1f, 0, 1.3f), new Vector3(0.1f, 0, 4.8f),
            new Vector3(-1.5f, 0, 1.3f), new Vector3(-1.5f, 0, 1.3f) };
        Vector3 spawnP = pos[(id + 1) % 10];
        Quaternion spawnR = (id + 1) % 10 % 2 == 0 ?
            Quaternion.Euler(0, 0, 0) :  // 0, 2
            Quaternion.Euler(0, 180, 0);  // 1, 3

        return (spawnP, spawnR);
    }

    // AR���� �����״� ������Ʈ���� ������ �� �ִٰ� ����?
    // �׷� ����ġ�� �켱 ����
    public NetworkPrefabRef[] prefabs;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        gazePointer = gameObject.transform.Find("GazePointer");

        //VirtualObjectInfo VO = JsonUtility.FromJson<VirtualObjectInfo>(PlayerPrefs.GetString("199"));
        //NetworkObject obj = Runner.Spawn(prefabs[0], VO.position, VO.rotation);
        //obj.GetComponent<VirtualObject>().info = VO;

        //NetworkObject obj = Runner.Spawn(prefabs[0], new Vector3(2,1,2), Quaternion.identity, Object.InputAuthority);


        //obj.GetComponent<TubeRenderer>().SetPositions(VO.sketch_dots.ToArray());
        // obj.GetComponent<MeshFilter>().sharedMesh = obj.GetComponent<TubeRenderer>().GetMesh();


        //Debug.Log(obj.InputAuthority.PlayerId);
    }

    // Update is called once per frame
    void Update()
    {
        txt.text = userName;
    }

    // temp RPC for spawn
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_username(PlayerRef id, string username, RpcInfo info = default)
    {

        User[] users = FindObjectsByType<User>(FindObjectsSortMode.InstanceID);
        foreach (User u in users)
        {
            Debug.Log("User " + id.PlayerId + " name will be " + username);
            if (u.playerId == id)
                u.userName = username;

        }

        
        
        //Runner.GetPlayerObject(id).gameObject.GetComponent<User>().userName = username;
        //Runner.SessionInfo.Properties.TryAdd(id.ToString(), SessionProperty.Convert(username));
    }



    public override void Spawned()
    {
        Debug.Log("Local: "+Runner.LocalPlayer+" Auth: "+ Object.InputAuthority);
        User[] users = FindObjectsByType<User>(FindObjectsSortMode.InstanceID);
        foreach (User u in users)
        {
            if(u.playerId == 9)
                RPC_username(Runner.LocalPlayer, PlayerPrefs.GetString("Username")); // �ڽ��� �̸��� ����

        }
        

    }

    

    public override void FixedUpdateNetwork()
    {
        

        // �ڽ��� ��ǥ�� ���ؼ��� ����
        if (Object.InputAuthority == Runner.LocalPlayer)
        {

            // local camera
            Spawner.instance.localUser.transform.SetPositionAndRotation(spawnPos, spawnRot);

            var ray = EyeTest.instance.centerEyeRay;
            gazeDirection = ray.direction;

            var localpos = Spawner.instance.localCenterEyeAnchor.transform.localPosition;
            
            gazeOrigin = (playerId + 1) % 10 % 2 == 0 ?
                spawnPos + new Vector3(localpos.x, 0, localpos.z):  // 0, 2
                spawnPos - new Vector3(localpos.x, 0, localpos.z);  // 1, 3

            gazePointerPos = Spawner.instance.localGazePointer.transform.position;

            rotation = spawnRot * Spawner.instance.localCenterEyeAnchor.transform.localRotation;

            rayLength = GazeHand.instance.length;

            // user spawn
            Object.GetComponent<NetworkTransform>().TeleportToPositionRotation(gazeOrigin, rotation);

            // request moving user
            RPC_user(gazeDirection, gazeOrigin, gazePointerPos, rotation, rayLength, Runner.LocalPlayer.PlayerId);
        }

    }

    
    // ���� �����̱�
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_user(Vector3 gazeDir, Vector3 gazeOrg, Vector3 gazeP, Quaternion rot, float l, int id, RpcInfo info = default)
    {

        // remote user's info
        gazeDirection = gazeDir;
        gazeOrigin = gazeOrg;
        gazePointerPos = gazeP;
        rotation = rot;
        rayLength = l;

        // only one user
        if(Object.InputAuthority == id)
        {
            GetComponent<NetworkTransform>().TeleportToPositionRotation(gazeOrg, rotation);

            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, gazeOrigin);
                lineRenderer.SetPosition(1, gazeOrigin + rayLength * gazeDirection);
            }

            if (gazePointer != null)
            {
                gazePointer.position = gazeP;
                gazePointer.LookAt(gazeDirection);
            }

        }

    }


    // temp RPC for spawn
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_spawn(string objInfo, Vector3 position, string prefix, int hand, int id, RpcInfo info = default)
    {
        Debug.Log("Call spawn from player " + id);
        Spawner.instance.spawn(objInfo, position, prefix, hand, id);
    }

    // temp RPC for spawn sketch
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_spawnSketch(int id, RpcInfo info = default)
    {
        Debug.Log("Call spawn from player " + id);
        Spawner.instance.spawnSketch2(id);
    }



    // ����ġ�� �����Ǿ����� �˸�
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_beginSketch(NetworkId id, RpcInfo info = default)
    {
        Debug.Log("beginSketch");
        Debug.Log("Sketch id: " + id);
        NetworkObject sketch = Runner.FindObject(id);

        if (FingerMenu.isPinching)  // ���� ����ġ�� �׸��� ����� 
            StartCoroutine(whileSketch(sketch));
        
    }

    // ���� ����ġ�� �׸��� ����� ����
    IEnumerator whileSketch(NetworkObject sketch)
    {
        Debug.Log("whileSketch");
        CoreInteraction.isSketching = true;
        int i = 0, j = 0;

        while (FingerMenu.isPinching)
        {
            if (i++ % 3 != 0)   // 3�����Ӵ� 1��
            {
                yield return null;
                continue;
            }

            // �� ��ǥ ����
            sketch.GetComponent<VirtualObjectNet>().RPC_sketchLive(playerId, j++, FingerMenu.instance.index.position);
            yield return null;
        }

        CoreInteraction.isSketching = false;

        if (j < 3) // empty sketch
            Runner.Despawn(sketch);


    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_remove(NetworkObject obj, RpcInfo info = default)
    {
        if (obj != null)
        {
            Runner.Despawn(obj);
            Debug.Log("Despawned");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_copy(NetworkObject obj, RpcInfo info = default)
    {
        if (obj != null)
        {

            NetworkObject cp = Runner.Spawn(Spawner.instance.netPrefabs[obj.GetComponent<VirtualObjectNet>().objectType],
                obj.transform.position + new Vector3(0.1f, 0.1f, 0.1f), obj.transform.rotation);

            // apply

            VirtualObjectNet cpData = cp.GetComponent<VirtualObjectNet>();
            VirtualObjectNet obData = obj.GetComponent<VirtualObjectNet>();

            cp.GetComponent<VirtualObject>().info = obj.GetComponent<VirtualObject>().info;

            cpData.playerID = obData.playerID;

            cpData.objectType = obData.objectType;
            cpData.color = obData.color;
            if (cp.GetComponent<MeshRenderer>() != null)
            {
                cp.GetComponent<MeshRenderer>().material = CoreInteraction.instance.intToColor(obData.color);
            }

            switch (cpData.objectType)
            {

                case (int)ObjectType.OBJ_TIMER:
                    cpData.timer_seconds = obData.timer_seconds;
                    cp.GetComponent<Timer>().seconds = cpData.timer_seconds;
                    break;

                case (int)ObjectType.OBJ_CLOCK:
                    cpData.clock_region = obData.clock_region;
                    cp.GetComponent<Clock>().region = (Region)cpData.clock_region;
                    break;

                case (int)ObjectType.OBJ_SUBSKETCH:


                    cpData.width = obj.GetComponent<TubeRenderer>()._radiusOne;
                    cpData.color = obj.GetComponent<VirtualObject>().info.color;

                    cpData.sketch_dots.CopyFrom(obData.sketch_dots.ToArray(), 0, obData.sketch_dots.Length);

                    TubeRenderer tubeRenderer = cp.GetComponent<TubeRenderer>();

                    //cp.GetComponent<MeshRenderer>().material = Spawner.instance.pureColors[0];
                    //cp.GetComponent<LineRenderer>().SetPositions(cpData.sketch_dots.ToArray());

                    List<Vector3> s = new List<Vector3>();
                    for (int i = 1; i < 300; i++)
                        if (!obData.sketch_dots[i].Equals(Vector3.zero))
                            s.Add(obData.sketch_dots[i]);
                        else break;

                    tubeRenderer.SetPositions(s.ToArray());
                    
                    tubeRenderer.SetRadius(obj.GetComponent<TubeRenderer>()._radiusOne);       // 1000:1 �� 0.01 : 10 (������ ����)

                    cp.GetComponent<MeshFilter>().mesh = tubeRenderer.GetMesh();
                    cp.GetComponent<MeshCollider>().sharedMesh = tubeRenderer.GetMesh();
                    break;

                case (int)ObjectType.OBJ_NOTE:
                    cpData.note_string = obData.note_string;
                    cp.transform.Find("Canvas").Find("Text").GetComponent<TMPro.TMP_Text>().text = cpData.note_string.ToString();
                    break;
                default:
                    break;
            }

            // end


            Debug.Log("Copied");
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        
    }


}
