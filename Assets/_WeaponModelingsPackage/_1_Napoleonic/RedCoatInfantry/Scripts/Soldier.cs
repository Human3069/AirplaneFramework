using UnityEngine;
using System.Collections;

public class Soldier : MonoBehaviour {
    Animator soldier;
    private IEnumerator coroutine;
	// Use this for initialization
	void Start () {
        soldier = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.S))
        {
            soldier.SetBool("idle",true);
            soldier.SetBool("walk", false);
            soldier.SetBool("turnleft", false);
            soldier.SetBool("turnright", false);
        }
        if (Input.GetKey(KeyCode.W))
        {
            soldier.SetBool("walk", true);
            soldier.SetBool("idle", false);
            soldier.SetBool("turnleft", false);
            soldier.SetBool("turnright", false);
        }
        if (Input.GetKey(KeyCode.A))
        {
            soldier.SetBool("turnleft", true);
            soldier.SetBool("walk", false);
            soldier.SetBool("idle", false);
            soldier.SetBool("turnright", false);
        }
        if (Input.GetKey(KeyCode.D))
        {
            soldier.SetBool("turnright", true);
            soldier.SetBool("turnleft", false);
            soldier.SetBool("walk", false);
            soldier.SetBool("idle", false);
        }
        if (Input.GetKey(KeyCode.F))
        {
            soldier.SetBool("guard", true);
            soldier.SetBool("idle", false);
            soldier.SetBool("walk", false);
            soldier.SetBool("turnleft", false);
            soldier.SetBool("turnright", false);
            StartCoroutine("idle2");
            idle2();
        }
        if (Input.GetKey(KeyCode.W))
        {
            soldier.SetBool("run", true);
            soldier.SetBool("idle2", false);
            soldier.SetBool("runleft", false);
            soldier.SetBool("runright", false);
        }
        if (Input.GetKey(KeyCode.S))
        {
            soldier.SetBool("idle2", true);
            soldier.SetBool("run", false);
            soldier.SetBool("runleft", false);
            soldier.SetBool("runright", false);
        }
        if (Input.GetKey(KeyCode.A))
        {
            soldier.SetBool("runleft", true);
            soldier.SetBool("runright", false);
            soldier.SetBool("run", false);
            soldier.SetBool("idle2", false);
        }
        if (Input.GetKey(KeyCode.D))
        {
            soldier.SetBool("runright", true);
            soldier.SetBool("runleft", false);
            soldier.SetBool("run", false);
            soldier.SetBool("idle2", false);
        }
        if (Input.GetKey(KeyCode.Alpha1))
        {
            soldier.SetBool("shot", true);
            soldier.SetBool("idle2", false);
            soldier.SetBool("run", false);
            soldier.SetBool("runleft", false);
            soldier.SetBool("runright", false);
            StartCoroutine("idle2");
            idle2();
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            soldier.SetBool("attack", true);
            soldier.SetBool("idle2", false);
            soldier.SetBool("run", false);
            soldier.SetBool("runleft", false);
            soldier.SetBool("runright", false);
            StartCoroutine("idle2");
            idle2();
        }
        if (Input.GetKey(KeyCode.Alpha0))
        {
            soldier.SetBool("die", true);
            soldier.SetBool("idle2", false);
            soldier.SetBool("run", false);
            soldier.SetBool("runleft", false);
            soldier.SetBool("runright", false);
        }
        if (Input.GetKey(KeyCode.R))
        {
            soldier.SetBool("shoulder", true);
            soldier.SetBool("guard", false);
            soldier.SetBool("run", false);
            soldier.SetBool("runleft", false);
            soldier.SetBool("runright", false);
            soldier.SetBool("idle2", false);
            soldier.SetBool("attack", false);
            StartCoroutine("idle");
            idle2();          
        }
	}
    IEnumerator idle2()
    {
        yield return new WaitForSeconds(1.0f);
        soldier.SetBool("idle2", true);
        soldier.SetBool("guard", false);
        soldier.SetBool("shot", false);
        soldier.SetBool("attack", false);
    }
    IEnumerator idle()
    {
        yield return new WaitForSeconds(1.0f);
        soldier.SetBool("idle", true);
        soldier.SetBool("shoulder", false);
    }
}
