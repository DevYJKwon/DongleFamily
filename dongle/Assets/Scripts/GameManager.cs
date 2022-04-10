using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("-------[Core]")]
    public int maxLevel;
    public int score;
    public bool isOver;

    [Header("-------[Object Pooling]")]
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    public List<ParticleSystem> effectPool;
    [Range(1,30)]
    public int poolSize;
    public int poolCursor;
    public Dongle lastDongle;
    public GameObject effectPrefab;
    public Transform effectGroup;

    [Header("-------[Audio]")]    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    int sfxCursor;
    public AudioClip[] sfxClip;
    public enum Sfx {LevelUp,Next,Attach,Button,Over};

    [Header("-------[UI]")]
    public GameObject startGroup;
    public Text scoreText;
    public Text maxScoreText;
    public GameObject endGroup;
    public Text subScoreText;

    [Header("-------[ETC]")]
    public GameObject line;
    public GameObject floor;

    void Awake(){
        Application.targetFrameRate = 60;
        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for(int i=0; i<poolSize; i++){
            MakeDongle();
        }
        if(!PlayerPrefs.HasKey("MaxScore")){
            PlayerPrefs.SetInt("MaxScore",0);
        }
        maxScoreText.text="Max Score: "+PlayerPrefs.GetInt("MaxScore").ToString();
    }
    // Start is called before the first frame update
    public void GameStart()
    {
        line.SetActive(true);
        floor.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);
        bgmPlayer.Play();
        SfxPlay(Sfx.Button);
        Invoke("NextDongle",0.5f);
    }

    Dongle MakeDongle(){
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect"+effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle"+donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.effect = instantEffect;
        instantDongle.manager = this;
        donglePool.Add(instantDongle);

        return instantDongle;
    }
    Dongle GetDongle(){
        for(int i=0; i<donglePool.Count; i++){
            poolCursor = (poolCursor+1) % donglePool.Count;
            if(!donglePool[poolCursor].gameObject.activeSelf){
                return donglePool[poolCursor];
            }
        }
        return MakeDongle();
    }
    void NextDongle(){
        if(isOver){
            return;
        }
        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0,maxLevel);
        lastDongle.gameObject.SetActive(true);
        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }

    IEnumerator WaitNext(){
        while(lastDongle !=null){
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);
        NextDongle();
    }
    public void TouchDown(){
        if(lastDongle == null){
            return;
        }
        lastDongle.Drag();
    }

    public void TouchUp(){
        if(lastDongle == null){
            return;
        }
        lastDongle.Drop();
        lastDongle=null;
    }

    public void GameOver(){
        if(isOver){
            return;
        }
        isOver=true;
        StartCoroutine(GameOverRoutine());
        bgmPlayer.Stop();
    }

    IEnumerator GameOverRoutine(){
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        for(int i=0; i<dongles.Length; i++){
            dongles[i].rigid.simulated=false;
        }

        for(int i=0; i<dongles.Length; i++){
            dongles[i].Hide(Vector3.up*100);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);

        int maxScore = Mathf.Max(score,PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore",maxScore);

        subScoreText.text += score.ToString();
        endGroup.SetActive(true);
        SfxPlay(Sfx.Over);
    }

    public void Reset(){
        SfxPlay(Sfx.Button);
        StartCoroutine(ResetCoRoutine());
    }

    IEnumerator ResetCoRoutine(){
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(0);
    }

    public void SfxPlay(Sfx type){
        switch(type){
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0,3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }
            sfxPlayer[sfxCursor].Play();
            sfxCursor = (sfxCursor+1) % sfxPlayer.Length;
    }
    void Update() {
        if(Input.GetButtonDown("Cancel")){
            Application.Quit();
        }    
    }
    void LateUpdate() {
       scoreText.text = "Score: "+score.ToString(); 
    }
}
