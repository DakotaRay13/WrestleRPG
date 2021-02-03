using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class tst_CharacterCreation : MonoBehaviour
{
    public GameObject CharacterModel;
    private GameObject NewChar;
    
    //The ui objects for character creation
    public GameObject ColorChoicePanel;

    private Material CurrentMaterial;
    private Material CurrentAccentMaterial;

    private bool isAccented = false;
    private float AccentFactor = 0.5f;

    public Image ColorPreview;

    public Slider sld_RED;
    public Slider sld_GREEN;
    public Slider sld_BLUE;

    public Text txt_RED;
    public Text txt_GREEN;
    public Text txt_BLUE;

    //Previous Color
    public float PrevR, PrevG, PrevB;

    // Start is called before the first frame update
    void Start()
    {
        //Disable the Color Choice Panel
        ColorChoicePanel.SetActive(false);

        //Instantiate the Character Model and place it in world
        NewChar = Instantiate(CharacterModel);
        NewChar.transform.position = new Vector3(5, 0, 0);
        NewChar.transform.eulerAngles = new Vector3(-90, 230, 0);

        //Set the Accent Colors
        CurrentMaterial = NewChar.GetComponent<MeshRenderer>().materials[0];
        SetAccentMaterial(true, 1);
        UpdateAccentColor();
        SetAccentMaterial(false, 0);
    }
    
    ///////////////////////////////////////////////////////////////////////////////////
    
    //Activate the Color Choice Panel and change the model's skin color
    public void btnSkinColorClick()
    {
        ActivateColorPanel(NewChar.GetComponent<MeshRenderer>().materials[0]);
        SetAccentMaterial(true, 1);
    }

    //Activate the Color Choice Panel and change the model's eye color
    public void btnEyeColorClick()
    {
        ActivateColorPanel(NewChar.GetComponent<MeshRenderer>().materials[9]);
        SetAccentMaterial(false, 0);
    }

    //Activate the Color Choice Panel and change the model's attire color
    public void btnAttireColorClick()
    {
        ActivateColorPanel(NewChar.GetComponent<MeshRenderer>().materials[2]);
        SetAccentMaterial(false, 0);
    }

    ///////////////////////////////////////////////////////////////////////////////////
    
    //Call these functions when the slider is moved
    public void RedSliderChange()
    {
        txt_RED.text = (sld_RED.value * 100).ToString("0.00");
        CurrentMaterial.color = new Vector4(sld_RED.value, CurrentMaterial.color.g, CurrentMaterial.color.b);

        if (isAccented == true) UpdateAccentColor();
    }

    public void GreenSliderChange()
    {
        txt_GREEN.text = (sld_GREEN.value * 100).ToString("0.00");
        CurrentMaterial.color = new Vector4(CurrentMaterial.color.r, sld_GREEN.value, CurrentMaterial.color.b);

        if (isAccented == true) UpdateAccentColor();
    }

    public void BlueSliderChange()
    {
        txt_BLUE.text = (sld_BLUE.value * 100).ToString("0.00");
        CurrentMaterial.color = new Vector4(CurrentMaterial.color.r, CurrentMaterial.color.g, sld_BLUE.value);

        if (isAccented == true) UpdateAccentColor();
    }

    ///////////////////////////////////////////////////////////////////////////////////

    public void SetAccentMaterial(bool x, int i)
    {
        if (x)
        {
            isAccented = true;
            CurrentAccentMaterial = NewChar.GetComponent<MeshRenderer>().materials[i];
        }

        else
            isAccented = false;
    }

    public void UpdateAccentColor()
    {
        CurrentAccentMaterial.color = new Vector4(CurrentMaterial.color.r * AccentFactor, CurrentMaterial.color.g * AccentFactor, CurrentMaterial.color.b * AccentFactor);
    }

    ///////////////////////////////////////////////////////////////////////////////////

    //Activate the Color Change Panel and set the current material to the one you want to change
    public void ActivateColorPanel(Material x)
    {
        ColorChoicePanel.SetActive(true);
        CurrentMaterial = x;

        //Set Sliders to Color
        sld_RED.value = x.color.r;
        sld_GREEN.value = x.color.g;
        sld_BLUE.value = x.color.b;

        //Set Previous colors for if canceled
        PrevR = x.color.r;
        PrevG = x.color.g;
        PrevB = x.color.b;

        //Set Preview color to material color
        ColorPreview.material = x;
    }

    //Apply color to model and deactivate panel
    public void btn_AcceptClick()
    {
        ColorChoicePanel.SetActive(false);
        SetAccentMaterial(false, 0);
    }

    //Revert material color and deactivate panel
    public void btn_BackClick()
    {
        CurrentMaterial.color = new Vector4(PrevR, PrevG, PrevB);
        if(isAccented == true)
        {
            UpdateAccentColor();
            SetAccentMaterial(false, 0);
        }
        ColorChoicePanel.SetActive(false);
    }
}
