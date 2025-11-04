using UnityEngine;

public class SoundManager : MonoBehaviour {

    public static SoundManager Instance {get; set;} // This is a Singleton

    //---
    [Header("Background Music")]
    private AudioSource bgmChannel;
    public AudioClip[] bgm;
    public float bgmVolume = 0.5f;
    public int currentTrack = 0;

    [Header("Infantry")]
    private AudioSource infantryAttackChannel;
    public AudioClip infantryAttackSound;

    [Header("Unit")]

    public int poolSize = 2;
    private int unitPoolIndex = 0;
    private int constructionPoolIndex = 0;

    public AudioSource[] unitVoiceChannelPool;

    public AudioClip[] unitSelectionSounds;
    public AudioClip[] unitCommandSounds;

    [Header("Buildings")]
    private AudioSource destructionBuildingChannel;
    private AudioSource[] constructionBuildingChannel;
    private AudioSource extraBuildingChannel;

    public AudioClip sellingSound;
    public AudioClip buildingConstructionSound;
    public AudioClip buildingDestructionSound;

    // Makes sure this is the only one 
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        //---
        bgmChannel = gameObject.AddComponent<AudioSource>();
        bgmChannel.volume = bgmVolume;
        bgmChannel.loop = true;

        infantryAttackChannel = gameObject.AddComponent<AudioSource>();
        // infantryAttackChannel.volume = 1f;
        // infantryAttackChannel.playOnAwake = false;

        destructionBuildingChannel = gameObject.AddComponent<AudioSource>();
        extraBuildingChannel = gameObject.AddComponent<AudioSource>();

        // initialize an array of audio sources with a array size set in poolSize
        unitVoiceChannelPool = new AudioSource[poolSize]; 
        for (int i = 0; i < poolSize; i++) {
            unitVoiceChannelPool[i] = gameObject.AddComponent<AudioSource>();
        }

        constructionBuildingChannel = new AudioSource[poolSize]; 
        for (int i = 0; i < poolSize; i++) {
            constructionBuildingChannel[i] = gameObject.AddComponent<AudioSource>();
        }


    }

    private void Start() {
        PlayBackgroundMusic(); //---
    }

    public void PlayBackgroundMusic() { //---
        if (bgm != null && bgm.Length > 0) {
            bgmChannel.clip = bgm[currentTrack];
            bgmChannel.Play();
        }
    }

    public void PlayInfantryAttackSound() {
        if(infantryAttackChannel.isPlaying == false) {
            infantryAttackChannel.PlayOneShot(infantryAttackSound);
        }
    }

    public void PlayBuildingSellingSound() {
        if(extraBuildingChannel.isPlaying == false) {
            extraBuildingChannel.PlayOneShot(sellingSound);
        }
    }

    public void PlayBuidlingConstructionSound() {
        constructionBuildingChannel[constructionPoolIndex].PlayOneShot(buildingConstructionSound);
        constructionPoolIndex = (constructionPoolIndex +1) % poolSize;
    }

    // public void PlayBuidlingConstructionSound() {
    //     if(constructionBuildingChannel.isPlaying == false) {
    //         constructionBuildingChannel.PlayOneShot(buildingConstructionSound);
    //     }
    // }

    public void PlayBuildingDestructionSound() {
        if(destructionBuildingChannel.isPlaying == false) {
            destructionBuildingChannel.PlayOneShot(buildingDestructionSound);
        }
    }

    public void PlayUnitSelectionSound() {
        AudioClip randomClip = unitSelectionSounds[Random.Range(0, unitSelectionSounds.Length)];
        
        unitVoiceChannelPool[unitPoolIndex].PlayOneShot(randomClip);

        unitPoolIndex = (unitPoolIndex +1) % poolSize;
    }

    public void PlayUnitCommandSound() {
        AudioClip randomClip = unitCommandSounds[Random.Range(0, unitCommandSounds.Length)];
        
        unitVoiceChannelPool[unitPoolIndex].PlayOneShot(randomClip);

        unitPoolIndex = (unitPoolIndex +1) % poolSize; // this will loop back around by using % (mod)  the index set by poolSize and everytime it's called it'll switch
    
    }



}
