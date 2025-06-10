using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LHS_Respawn2 : MonoBehaviour
{
    [SerializeField] float spawnValue;
    [SerializeField] GameObject player;

    //[SerializeField] private Transform player;
    [SerializeField] Transform respawnPoint;

    //���������� ������ ����
    Animator anim;

    private RaycastHit hit;
    private int layerMask;
    public float distance = 10;
    AudioSource resp;

    void Awake()
    {
        anim = player.GetComponentInChildren<Animator>();
        layerMask = 1 << 7;
        resp = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        /*
        if (player.transform.position.y < -spawnValue)
        {
            DownPlayer();
        }
        */
        
        // �÷��̾ �������� ���̸� ���µ�
        // RespawnTrigger�� �Ÿ��� Distance ���̶��
        // DownPlayer�� ���� ��Ű�� �ʹ�
        // DownPlayer �ִϸ��̼ǵ� �����Ű�� �ʹ�.

        if (player != null && Physics.Raycast(player.transform.position, -player.transform.up, out hit, distance, layerMask))
        {
            //RespSound();
            DownPlayer();
            resp.Play();
        }
        
    }

    void DownPlayer()
    {
        anim.SetBool("isFalling", true);
        //resp.Play();
    }

    // RaspawnTrigger�� �浹������ ������ �������� ���ư��� �ʹ�
    // �ִϸ��̼ǵ� ���� �ʹ�.
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {

            
            anim.SetBool("isFalling", false);

            player.transform.position = respawnPoint.transform.position;
            //player.transform.GetChild(0).transform.position = new Vector3(0, 0.09f, 0);
            // ��ȯ��������� ���������� ����
            //Physics.SyncTransforms();
            
        }
    }
    /*public void RespSound()
    {
        AudioSource resp = GetComponent<AudioSource>();
        resp.Play();
    }
    */
}
