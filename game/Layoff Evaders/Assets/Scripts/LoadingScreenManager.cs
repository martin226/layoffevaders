using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;
    public GameObject m_LoadingScreenObject;
    public Slider ProgressBar;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    public void SwitchToScene(int id)
    {
        m_LoadingScreenObject.SetActive(true);
        ProgressBar.value=0;
        StartCoroutine(SwitchToSceneAsync(id));
    }
    IEnumerator SwitchToSceneAsync(int id)
    {
        AsyncOperation asyncLoad=SceneManager.LoadSceneAsync(id);
        while(!asyncLoad.isDone)
        {
            ProgressBar.value=asyncLoad.progress;
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        m_LoadingScreenObject.SetActive(false);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
