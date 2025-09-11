using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioSource audioSource;     
    public AudioClip SoundOnButton;        
    public AudioClip SoundWhenClicked;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SoundOnButton != null)
            audioSource.PlayOneShot(SoundOnButton);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SoundWhenClicked != null)
            audioSource.PlayOneShot(SoundWhenClicked);
    }
}
