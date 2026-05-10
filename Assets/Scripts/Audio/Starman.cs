using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Starman : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{

    [SerializeField] Image BookshelfImage;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite hoverSprite;


    private void OnDisable()
    {
        BookshelfImage.sprite = defaultSprite;

        //Audio
        MusicManager.Instance.PlayTitleMusic();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BookshelfImage.sprite = hoverSprite;

        //Audio
        AkUnitySoundEngine.PostEvent("Book_Move", gameObject);
        MusicManager.Instance.PlayStarmanMusic();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BookshelfImage.sprite = defaultSprite;

        //Audio
        MusicManager.Instance.PlayTitleMusic();
    }

    public void OnSelect(BaseEventData eventData)
    {
        BookshelfImage.sprite = hoverSprite;

        //Audio
        AkUnitySoundEngine.PostEvent("Book_Move", gameObject);
        MusicManager.Instance.PlayStarmanMusic();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        BookshelfImage.sprite = defaultSprite;

        //Audio
        MusicManager.Instance.PlayTitleMusic();
    }
}
