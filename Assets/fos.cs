using UnityEngine;
using System.Collections;

public class fos : MonoBehaviour {
    private float dir = 1;
    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;
    private float syncanimspeed = 0f;
    private bool syncanimatk = false;
    private Animator m_animator;
    void Start() {
        m_animator = gameObject.GetComponent<Animator>();
    }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        Vector3 syncPosition = Vector3.zero;
        Vector3 syncVelocity = Vector3.zero;
        if (stream.isWriting) {
            syncPosition = transform.position;
            stream.Serialize(ref syncPosition);

            syncVelocity = gameObject.GetComponent<CharacterController>().velocity;
            stream.Serialize(ref syncVelocity);

            syncanimspeed = m_animator.GetFloat("Speed");
            stream.Serialize(ref syncanimspeed);
            syncanimatk = m_animator.GetBool("Attack");
            stream.Serialize(ref syncanimatk);
            
            stream.Serialize(ref dir);


        }
        else {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncVelocity);
            stream.Serialize(ref syncanimspeed);
            stream.Serialize(ref dir);
            stream.Serialize(ref syncanimatk);

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncEndPosition = syncPosition + syncVelocity * syncDelay;
            syncStartPosition = transform.position;
        }
    }
    private void SyncedMovement() {
        syncTime += Time.deltaTime;
        transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
        m_animator.SetFloat("Speed", syncanimspeed );
        gameObject.transform.localScale = new Vector3(Mathf.Sign(dir), gameObject.transform.localScale.y, gameObject.transform.localScale.z);

    }
    private void InputMovement() {
        gameObject.GetComponent<CharacterController>().Move(new Vector3(Input.GetAxis("Horizontal"), 0, 0) * Time.deltaTime);
        m_animator.SetFloat("Speed", 0.5f + Mathf.Abs(Input.GetAxis("Horizontal")));
        if (dir != Input.GetAxis("Horizontal") && Input.GetAxis("Horizontal") != 0) {
            dir = Input.GetAxis("Horizontal");
            gameObject.transform.localScale = new Vector3(Mathf.Sign(dir), gameObject.transform.localScale.y, gameObject.transform.localScale.z);
        }
        if (Input.GetKeyDown("space")) {
            Debug.Log("Attack");
            m_animator.SetBool("Attack", true);
        }
        else {
            m_animator.SetBool("Attack", false);
        }
    }
    // Update is called once per frame
    void Update() {
        if (gameObject.GetComponent<NetworkView>().isMine) {
            InputMovement();
        }
        else {
            SyncedMovement();
        }

    }
}
