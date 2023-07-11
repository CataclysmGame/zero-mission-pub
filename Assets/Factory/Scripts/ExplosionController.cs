using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    public void Reset()
    {
        transform.localScale = Vector3.one;
        GetComponent<Animator>().Play(0);
    }

    public void OnExplosionEnd()
    {
        GameManager.Instance.ExplosionsPool.ReturnToPool(gameObject);
        //Destroy(gameObject);
    }
}
