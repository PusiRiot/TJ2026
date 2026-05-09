using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI element that shows different ordered images when the user clicks on the "previous" and "next" buttons. 
/// It needs to be assigned on the inspector to the "previous" and "next" buttons.
/// </summary>

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class CarouselBehaviour : MonoBehaviour
{
    [SerializeField] Sprite[] carouselOptionsInOrder;
    [SerializeField] Button previousButton;
    [SerializeField] Button nextButton;

    Image image;
    int currentCarouselOption;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        Debug.Log($"previousButton: {previousButton}, nextButton: {nextButton}");

        // reset image to starter
        currentCarouselOption = 0;
        image.sprite = carouselOptionsInOrder[currentCarouselOption];

        // set buttons
        previousButton.gameObject.SetActive(false);

        if (carouselOptionsInOrder.Length > 1)
        {
            nextButton.gameObject.SetActive(true);
        }
        else
        {
            nextButton.gameObject.SetActive(false);
        }

        // set listeners
        previousButton.onClick.AddListener(PreviousInstruction);
        nextButton.onClick.AddListener(NextInstruction);
    }

    private void OnDisable()
    {
        previousButton.onClick.RemoveListener(PreviousInstruction);
        nextButton.onClick.RemoveListener(NextInstruction);
    }

    private void PreviousInstruction()
    {
        //Audio
        Debug.Log("PrevInstruction called");
        AkUnitySoundEngine.PostEvent("Select_UI", gameObject);

        currentCarouselOption--;
        image.sprite = carouselOptionsInOrder[currentCarouselOption];

        if (currentCarouselOption == 0) // it has gone back to the first image
        {
            previousButton.gameObject.SetActive(false); // hide the "previous button"
        } 
        if (currentCarouselOption == carouselOptionsInOrder.Length - 2) // it has gone back to the second to last image
        {
            nextButton.gameObject.SetActive(true); // show the "next button"
        }
    }
    
    private void NextInstruction()
    {
        //Audio
        Debug.Log("NextInstruction called");
        AkUnitySoundEngine.PostEvent("Select_UI", gameObject);

        currentCarouselOption++;
        image.sprite = carouselOptionsInOrder[currentCarouselOption];

        if (currentCarouselOption == 1) // it has passed the first image
        {
            previousButton.gameObject.SetActive(true); // show the "previous button"
        }
        if (currentCarouselOption == carouselOptionsInOrder.Length - 1) // it's on the last image
        {
            nextButton.gameObject.SetActive(false); // hide the "next button"
        }
    }
}