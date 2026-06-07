using UnityEngine;

/// <summary>
/// A single physics "muscle": every FixedUpdate it applies a torque to
/// drive <see cref="bone"/> toward <see cref="targetAngle"/> (in degrees).
/// The force is proportional to the angular error (P-controller).
/// </summary>
public class RagdollMuscle
{
    public Rigidbody2D bone;
    public float       targetAngle;   // world-space degrees
    public float       strength;      // N·m per degree of error
    public float       maxTorque;     // clamp to avoid explosion

    public RagdollMuscle(Rigidbody2D bone, float strength = 60f, float maxTorque = 200f)
    {
        this.bone      = bone;
        this.strength  = strength;
        this.maxTorque = maxTorque;
    }

    public void Activate()
    {
        if (bone == null) return;
        float err    = Mathf.DeltaAngle(bone.rotation, targetAngle);
        float torque = Mathf.Clamp(err * strength, -maxTorque, maxTorque);
        bone.AddTorque(torque);
    }
}
