using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ePowerUpDropType
{
    normalCrate,
    bossCrate
}

[RequireComponent(typeof(BoundsCheck))]                                     // a
public class PowerUp : MonoBehaviour
{
    [Header("Inscribed")]
    // This is an unusual but handy use of Vector2s.                          // b
    [Tooltip("x holds a min value and y a max value for a Random.Range() call.")]
    public Vector2 rotMinMax = new Vector2(15, 90);
    [Tooltip("x holds a min value and y a max value for a Random.Range() call.")]
    public Vector2 driftMinMax = new Vector2(.25f, 2);
    public float lifeTime = 10; // PowerUp will exist for # seconds
    public float fadeTime = 4;  // Then it fades over # seconds

    [Header("Dynamic")]
    public ePowerUpDropType dropType = ePowerUpDropType.normalCrate;
    public GameObject cube;          // Reference to the PowerCube child
    public TextMesh letter;        // Reference to the TextMesh
    public Vector3 rotPerSecond;  // Euler rotation speed for PowerCube 
    public float birthTime;     // The Time.time this was instantiated

    private Rigidbody rigid;
    private BoundsCheck bndCheck;
    private Material cubeMat;

    void Awake()
    {
        // Find the Cube reference (there’s only a single child)
        cube = transform.GetChild(0).gameObject;
        // Find the TextMesh and other components
        letter = GetComponent<TextMesh>();
        rigid = GetComponent<Rigidbody>();
        bndCheck = GetComponent<BoundsCheck>();
        cubeMat = cube.GetComponent<Renderer>().material;

        // Set a random velocity
        Vector3 vel = Random.onUnitSphere; // Get Random XYZ velocity          // c
        vel.z = 0;         // Flatten the vel to the XY plane
        vel.Normalize();   // Normalizing a Vector3 sets its length to 1m

        vel *= Random.Range(driftMinMax.x, driftMinMax.y);                    // d
        rigid.velocity = vel;

        // Set the rotation of this PowerUp GameObject to R:[ 0, 0, 0 ]
        transform.rotation = Quaternion.identity;
        // Quaternion.identity is equal to no rotation.

        // Randomize rotPerSecond for PowerCube using rotMinMax x & y         // e
        rotPerSecond = new Vector3(Random.Range(rotMinMax[0], rotMinMax[1]),
        Random.Range(rotMinMax[0], rotMinMax[1]),
        Random.Range(rotMinMax[0], rotMinMax[1]));

        birthTime = Time.time;
    }

    // void Start() {…}  // Please delete the unused Start() method

    void Update()
    {
        if (Main.GAME_PAUSED) return;

        cube.transform.rotation = Quaternion.Euler(rotPerSecond * Time.time); // f

        // Fade out the PowerUp over time
        // Given the default values, a PowerUp will exist for 10 seconds
        //   and then fade out over 4 seconds.
        float u = (Time.time - (birthTime + lifeTime)) / fadeTime;              // g
        // If u >= 1, destroy this PowerUp
        if (u >= 1)
        {
            Destroy(this.gameObject);
            return;
        }
        // If u>0, decrease the opacity (i.e., alpha) of the PowerCube & Letter
        if (u > 0)
        {
            Color c = cubeMat.color;
            c.a = 1f - u;          // Set the alpha of PowerCube to 1-u
            cubeMat.color = c;
            // Fade the Letter too, just not as much
            c = letter.color;
            c.a = 1f - (u * 0.5f);   // Set the alpha of the letter to 1-(u/2)
            letter.color = c;
        }

        if (!bndCheck.isOnScreen)
        {
            // If the PowerUp has drifted entirely off screen, destroy it
            Destroy(gameObject);
        }
    }

    public void SetDropType(ePowerUpDropType dt)
    {
        dropType = dt;

        if (dt == ePowerUpDropType.bossCrate)
        {
            cubeMat.color = Color.yellow;
            letter.text = "B";
        }
        else
        {
            cubeMat.color = Color.cyan;
            letter.text = "U";
        }
    }

    /// <summary>
    /// This function is called by the Hero class when a PowerUp is collected.
    /// </summary>
    /// <param name="target">The GameObject absorbing this PowerUp</param>
    public void AbsorbedBy(GameObject target)
    {                              // i
        Destroy(this.gameObject);
    }

}