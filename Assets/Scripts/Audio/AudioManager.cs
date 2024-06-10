using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

public class AudioManager : MonoSingleton<AudioManager>
{
    public AudioMixer mixer;
    [SerializeField] private AudioSource battleMusicSource;
    [SerializeField] private AudioSource planningMusicSource;
    [SerializeField] private AudioClip[] battleMusicClips;
    [SerializeField] private AudioClip[] planningMusicClips;
    [SerializeField] private AudioClip[] buttonClips;
    [SerializeField] private AudioClip notEnoughGoldClip;
    [SerializeField] private GameObject sfxPrefab;
    private ObjectPool<PooledObject> tempSFXPool;
    private List<AudioSource> dynamicAudioSources = new List<AudioSource>();
    private List<AudioClip> activeSFX = new List<AudioClip>();

    public float masterVolume;
    public float sfxVolume;
    public float musicVolume;
    public enum MusicState
    {
        Planning,
        Playing,
        Victory,
        Defeat
    }

    private void Start()
    {
        GameObject SFXObject = new GameObject(sfxPrefab.name + " pool gameobject");
        SFXObject.transform.parent = transform.GetChild(0);
        tempSFXPool = SFXObject.AddComponent<GenericPool>().Init(sfxPrefab, 20).GetPool();
        TransitionMusic(MusicState.Planning, 1, false);
    }


    public void PauseAudio()
    {
        for (int i = 0; i < dynamicAudioSources.Count; i++)
        {
            if (dynamicAudioSources[i] != null) { dynamicAudioSources[i].Pause(); }
        }
    }

    public void UnPauseAudio()
    {
        for (int i = 0; i < dynamicAudioSources.Count; i++)
        {
            if (dynamicAudioSources[i] != null) { dynamicAudioSources[i].UnPause(); }
        }
    }
    public void PlayButtonCloseSFX()
    {
        PlaySFX(buttonClips[4]);
    }

    public void PlayButtonConfirmSFX()
    {
        PlaySFX(buttonClips[0]);
    }
    public void PlayButtonReturnSFX()
    {
        PlaySFX(buttonClips[1]);
    }
    public void PlayButtonUpgradeSFX()
    {
        PlaySFX(buttonClips[2]);
    }
    public void PlayButtonHoverSFX()
    {
        PlaySFX(buttonClips[3]);
    }
    public void PlayNotEnoughGoldSFX()
    {
        PlaySFX(notEnoughGoldClip);
    }

    //pass in an audioclip to play that doesnt allow another of that type to play until half its duration has ended
    //this stops stacking overlapping sfx getting louder e.g. rapid explosions or enemies dying
    public void PlaySFX(AudioClip audio)
    {
        if (masterVolume == 0.0001f && sfxVolume == 0.0001f) { return; }
        else if (activeSFX.Contains(audio)) { return; }
        AudioSource oneshot = tempSFXPool.Get().GetComponent<AudioSource>();
        oneshot.clip = audio;
        float clipLength = audio.length;
        oneshot.gameObject.SetActive(true);
        oneshot.Play();
        StartCoroutine(WaitThenAllowSFX(audio, clipLength / 2));
        oneshot.GetComponent<PooledAudio>().ReturnToPoolAfterTimer(clipLength);
    }

    private IEnumerator WaitThenAllowSFX(AudioClip clip, float maxTime)
    {
        activeSFX.Add(clip);
        float timer = maxTime;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        activeSFX.Remove(clip);
    }

    //transition between music so that it is less jarring
    public void TransitionMusic(MusicState newState, int waveCounter, bool fade = true)
    {
        if (newState == MusicState.Defeat ||
            newState == MusicState.Victory)
        {
            planningMusicSource.volume = 1;
            planningMusicSource.loop = false;
            //musicSource.clip = musicClips[(int)newState];
            planningMusicSource.Play();
        }
        else
        {
            // waveCounter/5 means how many rounds for each music clip
            int index = Mathf.FloorToInt(waveCounter / 5);
            AudioClip newMusic = planningMusicClips[index]; 
            if (newState == MusicState.Playing) { newMusic = battleMusicClips[index]; }

            AudioSource prevSource = (newState == MusicState.Planning ? battleMusicSource : planningMusicSource);
            AudioSource newSource = (newState == MusicState.Planning ? planningMusicSource : battleMusicSource);

            if (fade) { StartCoroutine(LerpMusicTransition(newMusic, prevSource, newSource)); }
            else
            {
                if (newSource.clip != newMusic) { newSource.clip = newMusic; }
                newSource.Play();
                prevSource.Pause();
            }
        }
    }

    private IEnumerator LerpMusicTransition(AudioClip newClip, AudioSource prevSource, AudioSource newSource)
    {
        float timer = 0;

        newSource.loop = true;
        newSource.volume = 0;
        if (newSource.clip != newClip) { newSource.clip = newClip; }
        newSource.Play();

        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            prevSource.volume = Mathf.Lerp(1, 0, timer / 0.5f);
            newSource.volume = Mathf.Lerp(0, 1, timer / 0.5f);
            yield return null;
        }

        prevSource.Pause();
        newSource.volume = 1;  
    }

    public void ChangeMasterVolume(float val)
    {
        masterVolume = val;
        if (masterVolume == 0.0001f) { mixer.SetFloat("MasterVolume", -80); }
        else { mixer.SetFloat("MasterVolume", Mathf.Log10(val) * 20); }
    }
    public void ChangeSFXVolume(float val)
    {
        sfxVolume = val;

        if (val == 0.0001f) { mixer.SetFloat("SFXVolume", -80); }
        else { mixer.SetFloat("SFXVolume", Mathf.Log10(val) * 20); }

    }
    public void ChangeMusicVolume(float val)
    {
        musicVolume = val;

        if (val == 0.0001f) { mixer.SetFloat("MusicVolume", -80); }
        else { mixer.SetFloat("MusicVolume", Mathf.Log10(val) * 20); }
    }
}
