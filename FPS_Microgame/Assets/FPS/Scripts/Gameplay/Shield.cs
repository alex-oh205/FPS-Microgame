using UnityEngine;

public class Shield : MonoBehaviour
{
    [Tooltip("Audio source used for active and destruction sound effects")]
    public AudioSource AudioSource;

    [Tooltip("Sound played when deactivated")]
    public AudioClip deactivateSfx;

    [Tooltip("Sound played when active")]
    public AudioClip activeSfx;

    public AudioSource destructionSource;

    private void Start()
    {
        destructionSource = transform.parent.GetComponent<AudioSource>();

        // play active vfx if generators exist
        if (transform.parent.childCount > 1)
        {
            AudioSource.clip = activeSfx;
            AudioSource.loop = true;
            AudioSource.Play();
        }
    }

    private void Update()
    {
        // destruction vfx
        if (transform.parent.childCount == 1)
        {
            destructionSource.PlayOneShot(deactivateSfx, 1f);
            Destroy(gameObject);
        }
    }
}
