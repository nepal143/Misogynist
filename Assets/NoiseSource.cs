using UnityEngine;

public class NoiseSource : MonoBehaviour
{
    public float noiseRadius = 10f;
    public string noiseMakerTag = "Player";
    public float noiseStrength = 0.5f;

    public void EmitNoise()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, noiseRadius);
        foreach (var hit in hits)
        {
            WifeAIController ai = hit.GetComponent<WifeAIController>();
            if (ai != null)
            {
                ai.OnSoundHeard(transform.position, noiseMakerTag, noiseStrength);
            }
        }
    }
}
