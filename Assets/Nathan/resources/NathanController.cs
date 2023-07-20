using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Nathan
{
    public class NathanController : MonoBehaviour
    {

        Animator animator;

        DoorController[] door_controllers;

        // Vector3 bed_pos = new Vector3(5.15f, 0.5803499f, 2.08f);
        // Vector3 outside_pos = new Vector3(6.0f, 0.5803499f, -6.26f);
        // Vector3 door_pos = new Vector3(2.84f, 0.5803499f, -7.46f);
        // Vector3 pills_pos = new Vector3(4.81f, 0.5803499f, -4.65f);
        // Vector3 pre_pills_pos = new Vector3(3.81f, 0.5803499f, -4.65f);
        // Vector3 couch_pos = new Vector3(-.88f, 0.5803499f, -1.71f);
        public Transform bed_pos;
        public Transform outside_pos;
        public Transform door_pos;
        public Transform pills_pos;
        public Transform pre_pills_pos;
        public Transform couch_pos;
        public Transform eat_pos;
        public Transform pre_eating_pos;
        private int protocol = -1;
        private int loc = -1;
        private bool waited = true;
        private long wait_until = 0;
        private bool animation_played = false;

        void Start()
        {
            animator = GetComponent<Animator>();
            door_controllers = FindObjectsOfType<DoorController>();
            Component[] components = GetComponents<Animator>();
            foreach (Component component in components)
            {
                Debug.Log(component.GetType().Name);
            }
}
        // Update is called once per frame
        void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.RightArrow)&& Input.GetKey(KeyCode.RightShift))
            {
                var tmp = transform.eulerAngles;
                tmp.y += 350f*Time.deltaTime;
                transform.eulerAngles = tmp;
            }
            
            if (Input.GetKey(KeyCode.LeftArrow)&& Input.GetKey(KeyCode.RightShift))
            {
                var tmp = transform.eulerAngles;
                tmp.y -= 350f*Time.deltaTime;
                transform.eulerAngles = tmp;
            }
            
            if (Input.GetKey(KeyCode.UpArrow)&& Input.GetKey(KeyCode.RightShift))
            {
                var tmp = transform.forward; 
                tmp = 2f*tmp.normalized*Time.deltaTime;
                transform.position += tmp;
                animator.SetFloat("speed", 1.0f);
                return;
            }
            
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (milliseconds > wait_until && Input.GetKey(KeyCode.N) && Input.GetKey(KeyCode.RightShift))
            {
                animator.SetBool("take_pills", true);
                wait_until = milliseconds + 1000;
                return;
            }
            if (milliseconds > wait_until && Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.RightShift))
            {
                animator.SetBool("eating", true);
                wait_until = milliseconds + 1000;
                return;
            }
            
            if (milliseconds > wait_until && Input.GetKey(KeyCode.O) && Input.GetKey(KeyCode.RightShift))
            {
                door_controllers[0].open = !door_controllers[0].open;
                animator.SetBool("open_door", true);
                wait_until = milliseconds + 1000;
                return;
            }
   
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
            {
                // waited = true;
                animator.SetBool("take_pills", false);
                animator.SetBool("open_door", false);
                animator.SetBool("eating", false);
                // animator.SetFloat("speed", 0);
            }
            
            animator.SetFloat("speed", 0);
        }
    }
}
