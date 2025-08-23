using Zenject;
using UnityEngine;

public class TriggerScuffRustles : MonoBehaviour {
    public AudioSource SFX;
    public int clip;
    private float finished;

    [Inject] private Const _consts;
    
    void OnTriggerEnter (Collider col) {
        if (clip < 0 || clip >= _consts.sounds.Length) return;
        if (!(col.gameObject.CompareTag("Player"))) return;
        if (finished > Time.time) return;

		finished = Time.time + Random.Range(3f,5f);
        SFX.PlayOneShot(_consts.sounds[clip],Random.Range(0.5f,0.75f));
	}
}
