using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class LinkOpener : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] bool _hyperlink;

    public void OnPointerClick(PointerEventData eventData)
    {
        TMP_Text pTextMeshPro = GetComponent<TMP_Text>();
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, eventData.position, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];
            Debug.Log(linkInfo.GetLinkID());
            OnLinkClicked(linkInfo.GetLinkID());
        }
    }

    void OnLinkClicked(string id)
    {
        if (_hyperlink)
        {
            Application.OpenURL(id);
            return;
        }

        switch (id)
        {
            case "Send_code":
                Debug.Log("Send code one more time");
                break;
            default:
                break;
        }
    }
}
