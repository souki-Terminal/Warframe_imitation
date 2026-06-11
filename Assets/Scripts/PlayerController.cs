using UnityEngine;

public class PlayerController : MonoBehaviour {

    private Rigidbody rb;
    private Transform tf;
    private Animator anim;
    private float horizontal= 0;
    private float vertical = 0;
    private Vector3 velocity;
    private float speed = 1f;

    void Start() {
        rb = GetComponent<Rigidbody>();
        tf = GetComponent<Transform>();
        anim = GetComponent<Animator>();
    }


    void FixedUpdate() {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");


        velocity = new Vector3(horizontal, 0, vertical).normalized;
        rb.linearVelocity = velocity * speed;


        if (velocity.magnitude > 0.1f) {
            anim.SetBool("walking", true);
        } else {
        anim.SetBool("walking", false);
        }


        // 追記
        if (velocity != Vector3.zero) {
            tf.rotation = Quaternion.LookRotation(velocity);
        }
    }
}